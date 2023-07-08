## Description 
Small distributed app for buying tickets to the cinema. The server receives client requests through an ASP.NET Core Controller and delegates tasks to the workers through a RabbitMQ message queue. Another queue is used by workers for sending acknowledgements and responses back to the server.
The worker communicates with a postgres local database. Both Postgres and RabbitMQ are started as services described in the docker-compose file.

## Requirements
At least the following requirements should be met:
  - [x] The project should use web services technology (e.g. SOAP, REST) to allow clients to access the server's functionality.
  - [x] The MOM framework used should support load balancing to distribute incoming requests across multiple servers to ensure that no single server becomes overloaded.
  - [x] The MOM framework used should support failover to ensure that if one server fails, requests can be automatically redirected to another available server to ensure continuous service.
  - [] The MOM framework used should provide message persistence to ensure that messages are not lost in the event of a server failure.
  - [x] The MOM framework used should provide reliable message delivery to ensure that messages are delivered to their intended recipients, even in the event of a network failure or other disruption.
  - [x] The MOM framework used should support message queuing to enable asynchronous communication between clients and servers.
  - [x] The project should be designed to scale horizontally to handle increasing numbers of clients and data volume.
  - [x] The MOM framework used should provide security features to ensure the confidentiality, integrity, and availability of messages.
  - [x] The project should be designed to work with existing MOM frameworks, such as Apache ActiveMQ, IBM MQ, or RabbitMQ.
