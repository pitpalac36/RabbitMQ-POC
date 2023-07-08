DROP TABLE IF EXISTS Tickets;
DROP TABLE IF EXISTS Movies;

create table Movies (
    id uuid DEFAULT uuid_generate_v4(),
    title varchar(100),
    price integer,
    run_date date,
    free_seats int,
	PRIMARY KEY (id)
);

create table Tickets (
    id uuid DEFAULT uuid_generate_v4(),
    movie_id uuid,
	PRIMARY key	(id),
    constraint fk_movie foreign key (movie_id) references Movies(id)
);

insert into movies 
	(title, price, run_date, free_seats) VALUES
	('Titanic', 17, '2023-05-27', 25),
	('Black Swan', 25, '2023-05-11', 25),
	('Brave', 19, '2023-03-03', 30);
	
select * from movies;