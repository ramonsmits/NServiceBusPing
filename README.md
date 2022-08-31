# NServiceBusPing

Simple utility that to ping a destination via NServiceBus using the RabbitMQ transport.

## Usage

```cmd
nservicebusping.exe endpointName destination interval count [concurrencyLimit]

    endpointName        Endpoint name of this instance.

    destination         Destination endpoint name.

    interval            Time between sends in milliseconds.

    count               Number of message to be send to destination.

    concurrencyLimit    Amount of maximum allowed incoming messages to be
                        processed concurrently.
```

## Example

Host `pong` that response to ping requests. Will send a message to itself to warmup the receive pipeline:

```cmd
nservicebusping.exe pong pong 1 1
```

Host `ping` that sends 1000 ping requests 50 milliseconds apart to endpoint `pong`:

```cmd
nservicebusping.exe ping pong 50 count [concurrencyLimit]
```
