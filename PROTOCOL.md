# Protocol

## Header
4 byte: id

4 byte: timestamp

1 byte: (primo bit se reliable o no): se json ignora il resto del byte, altrimenti AND con 0x7f (00-) e converti a intero

resto : PAYLOAD

## JOIN
Payload: Json deserialized from class JoinHandlerJson: https://github.com/aiv01/brawler-server/blob/master/Brawler-server/Server/JoinHandler.cs

## Host info
Hostname: unbit0016.uwsgi.it (server's local 10.0.0.238)

Port: 20234
