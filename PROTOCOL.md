# Protocol

## Header  
4 byte (UInt32): Id  
* Packet Id

4 byte (Single): Timestamp  
* Time elapsed from the start of the server (in seconds)

1 byte: Packet Infos  
* Most Significant Bit: 1 if reliable, otherwise 0; 
* Other 7 bits: command

## Payload

### JOIN (command 0) Client > Server
Json Payload:  
* Empty

### CLIENT JOINED (command 1) Server > Client
Json Payload:
* string Name: Player Name
* uint Id: Client Unique Identifier

### LEAVE (command 2) Client > Server
Json Payload:  
* Empty

### CLIENT LEFT (command 3) Server > Client  
Json Payload:  
* uint Id: Client Unique Identifier
* string: Reason

### MOVE (command 4) Client > Server
Binary Payload:  
* X (float): X position of the player;  
* Y (float): Y position of the player;  
* Z (float): Z position of the player;  
* Rx (float): X component of the quaternion rotation;  
* Ry (float): Y component of the quaternion rotation;  
* Rz (float): Z component of the quaternion rotation;  
* Rw (float): W component of the quaternion rotation;   

### CLIENT MOVED (command 5) Server > Client
Binary Payload:  
* X (float): X position of the player;  
* Y (float): Y position of the player;  
* Z (float): Z position of the player;  
* Rx (float): X component of the quaternion rotation;  
* Ry (float): Y component of the quaternion rotation;  
* Rz (float): Z component of the quaternion rotation;  
* Rw (float): W component of the quaternion rotation;  
* Id (uint): Client Unique Identifier;

### AUTH (command 125) Client > Server
Json Payload:
* string AuthToken: Authentication Token

### CLIENTAUTHED (command 126) Server > Client
Json Payload:
* string Ip: Client Ip
* string Port: Client Port

### ACK (command 127) Client > Server && Server > Client
Binary Payload:
* Id (UInt): Packet id to check

## Host info
Hostname: unbit0016.uwsgi.it (server's local 10.0.0.238)

Port: 20234
