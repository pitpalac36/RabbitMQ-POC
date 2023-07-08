using System;
using Dapper;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Worker.Helpers;
using Worker.Infrastructure;
using Worker.Model;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var requestChannel = connection.CreateModel();
using var responseChannel = connection.CreateModel();

// requestChannel.QueueBind("", "requestQueue", "");
// responseChannel.QueueBind("", "responseQueue", "");
requestChannel.QueueDeclare("requestQueue", durable: false, exclusive: false);
responseChannel.QueueDeclare("responseQueue", durable: false, exclusive: false);

var dbConnection = new NpgsqlConnection(connectionString: "Server=localhost;Port=5432;User Id=postgres;Password=postgres;Database=movies;");
dbConnection.Open();

var consumer = new EventingBasicConsumer(requestChannel);
consumer.Received += async (model, eventArgs) =>
{
    Console.WriteLine("Got a message");
    var body = eventArgs.Body.ToArray();
    Request request = Serializer.Deserialize<Request>(body);
    Console.WriteLine("Got a request with id: " + request.OrderId);

    var response = new Response {
        OrderId = request.OrderId,
        RequestType = request.RequestType,
        ResponseType = ResponseType.Ack,
        Success = true
    };
    responseChannel.SendSerialized(response);

    response.ResponseType = ResponseType.Res;
    switch (request.RequestType)
    {
        case RequestType.Get:
            {
                response.Movies = (await dbConnection.QueryAsync<Movie>("select * from movies")).ToList();
                foreach (var movie in response.Movies)
                {
                    Console.WriteLine($"{movie.Title} : {movie.FreeSeats}");
                }
                break;
            }

        case RequestType.Buy:
            {
                using (var transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        var movies = (await dbConnection.QueryAsync<Movie>("select * from movies where id = @id", new { id = request.MovieId })).ToList();
                        if (movies.Count == 0)
                        {
                            response.ErrorMessage = "Movie does not exist";
                            response.Success = false;
                            break;
                        }
                        var movie = movies[0];
                        if (movie.FreeSeats == 0)
                        {
                            response.ErrorMessage = "No more seats available";
                            response.Success = false;
                            break;
                        }
                        await dbConnection.ExecuteAsync("update movies set free_seats = (free_seats - 1) where id = @id", new { id = movie.Id });
                        var ticketId = await dbConnection.ExecuteScalarAsync<Guid>("insert into tickets (movie_id) values (@id) returning id", new { id = movie.Id });
                        movie = dbConnection.QuerySingle<Movie>("select * from movies where id = @id", new { id = movie.Id });
                        response.Ticket = new Ticket(ticketId, movie );
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        response.ErrorMessage = "Could not buy ticket. " + e.Message;
                        response.Success = false;
                        transaction.Rollback();
                    }
                }
                break;
            }

        case RequestType.Cancel:
            {
                using (var transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        var movie_ids = (await dbConnection.QueryAsync<Guid>("select movie_id from tickets where id = @id", new { id = request.TicketId })).ToList();
                        if (movie_ids.Count == 0)
                        {
                            response.ErrorMessage = "Ticket does not exist";
                            response.Success = false;
                            transaction.Rollback();
                            break;
                        }
                        var movie_id = movie_ids[0];
                        await dbConnection.ExecuteAsync("delete from tickets where id = @id", new { id = request.TicketId });
                        await dbConnection.ExecuteAsync("update movies set free_seats = (free_seats + 1) where id = @id", new { id = movie_id });
                        response.Movie = dbConnection.QuerySingle<Movie>("select * from movies where id = @id", new { id = movie_id });
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        response.ErrorMessage = "Could not cancel ticket. " + e.Message;
                        response.Success = false;
                        transaction.Rollback();
                    }
                }
                break;
            }

        default: break;
    }
    responseChannel.SendSerialized(response);
};
requestChannel.BasicConsume(queue: requestChannel.CurrentQueue, autoAck: true, consumer: consumer);
Console.ReadLine();
