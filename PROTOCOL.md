# Protocol

## Header  
4 byte (Int32): Id  
*Packet Id

4 byte (Single): Timestamp  
*Time elapsed from the start of the server (in seconds)

1 byte: Packet Infos  
*Most Significant Bit: 1 if reliable, otherwise 0; 
*Other 7 bits: command

##Payload

### JOIN (command 0)
Json Payload:  
*Json deserialized from class JoinHandlerJson: https://github.com/aiv01/brawler-server/blob/master/Brawler-server/Server/JoinHandler.cs

### KICK (command 1)
Not yet implemented

### LEAVE (command 2)
Json Payload:  
*Json deserialized from class LeaveHandlerJson: https://github.com/aiv01/brawler-server/blob/master/Brawler-server/Server/LeaveHandler.cs

### UPDATE (command 3)
Binary Payload:  
*X (float): X position of the player;  
*Y (float): Y position of the player;  
*Z (float): Z position of the player;  
*Rx (float): X component of the quaternion rotation;  
*Ry (float): Y component of the quaternion rotation;  
*Rz (float): Z component of the quaternion rotation;  
*Rw (float): W component of the quaternion rotation;  

## Host info
Hostname: unbit0016.uwsgi.it (server's local 10.0.0.238)

Port: 20234
