# hotelbooking
Hotel booking solution

The booking API was crate to simulate a scalable and reliable services to simulate a hotel room booking. It's composed by two projects.

Booking.Api:
  - A WEBAPI project that provides the functionality needed to pre-reserve a room, confirm a reservation, update it and delete it.

QueueService:
  - A service worker project, responsible to receive messages from a RabbitMQ queue and update, delete, create registrations in the mongo database.

**Technial Overview**

In order to provide a better response time and try to avoid concurrency I decided to create the PreReserve endpoint, this endpoint basically locks a date for reservation for period of one minute in the redis database. Whithin this time, it's possible to the API user to send a POST reservation request to confirm his reservation and stores it in the database. A basic overflow is:

1) client -----------> call PreReserve -----------------------> Locks the desired dates in redis databse if they are available.
2) client -----------> call POST to confirm reservation ------> Looks on redis if those dates are locked for the user -------> Post a message in RabbitMQ to persist the reservation and update the redis cache to maintain the dates locked for the time of the reservation.
3) client -----------> if other user tries to lock a conflict date range -------> the request only goes to redis to avoid an huge overload on the database.
4) if the client does not confirm his reservation whitin 1 minute, the cache expires and other user is ok to lock those dates.

The idea is to hit Redis in the create and update calls in since it's performatic, ligth and reliable letting the mongo database used only for confirmed transactions.
The decision for: Mongo, Redis, RabbitMQ were made due to scalability and performance, due to the requirements of the application.

**How to run the project**

Just go to the repository folder and run 'docker-compose up'

**Possible next steps**
- Separate most used endpoints in different services in order to be able to scale just the most demanded operation
- Apply an API Gateway such as Ocelot
- Prepare the solution to a k8s deployment

**How to test**
For simple test, there's a postman collection that can be used in the repository.
