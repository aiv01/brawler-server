# Protocol

## Header
4 byte: Id

4 byte: Timestamp

1 byte: most significant bit: Reliable or not; other 7 bits: command

then: PAYLOAD

## JOIN (command 0)
Payload: Json deserialized from class JoinHandlerJson: https://github.com/aiv01/brawler-server/blob/master/Brawler-server/Server/JoinHandler.cs

## LEAVE (command 2)
Payload: Json deserialized from class LeaveHandlerJson: https://github.com/aiv01/brawler-server/blob/master/Brawler-server/Server/LeaveHandler.cs

## Update (command 2)
Payload: Json deserialized from class UpdateHandlerJson: https://github.com/aiv01/brawler-server/blob/master/Brawler-server/Server/UpdateHandler.cs


## Host info
Hostname: unbit0016.uwsgi.it (server's local 10.0.0.238)

Port: 20234
