# Protocol

## Header  
4 byte (UInt32): Id  
* Packet Id

4 byte (uint): Timestamp  
* Time elapsed from the start of the server (in milliseconds)

1 byte: Packet Infos  
* Most Significant Bit: 1 if reliable, otherwise 0;
* Other 7 bits: command

## Payload

### JOIN (command 0) Client > Server
Json Payload:  
* Empty

### CLIENT JOINED (command 1) Server > Client - Reliable
Json Payload:
* string Name: Player Name
* uint Id: Client Unique Identifier

### LEAVE (command 2) Client > Server
Json Payload:  
* Empty

### CLIENT LEFT (command 3) Server > Client - Reliable
Json Payload:  
* uint Id: Client Unique Identifier
* string Reason: Reason for leaving (kicked, quit, ...)

### MOVE (command 4) Client > Server
Binary Payload:  
* MoveType (byte): Movement type of the player (like camera lock)
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;

### CLIENT MOVED (command 5) Server > Client
Binary Payload:  
* Id (uint): Client Unique Identifier;
* MoveType (byte): Movement type of the player (like camera lock)
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;

### DODGE (command 6) Client > Server
Binary Payload:
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;

### CLIENT DODGED (command 7) Server > Client - Reliable
Binary Payload:
* Id (uint): Client Unique Identifier
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;

### TAUNT (command 8) Client > Server
Binary Payload:
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;

### CLIENT TAUNTED (command 9) Server > Client - Reliable
Binary Payload:
* Id (uint): Client Unique Identifier
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;

### PING (command 121) Client > Server
Binary Payload:
* Empty

### PING (command 121) Server > Client
Binary Payload:
* uint Id: Client (PINGED) Unique Identifier

### PONG (command 122) Client > Server
Binary Payload:
* uint Id: Client (PINGED) Unique Identifier

### PONG (command 122) Server > Client
Binary Payload:
* uint Id: Client (PONGED) Unique Identifier

### CHAT (command 123) Client > Server
Json Payload:
* string Text: Chat text (max 128 characters)

### CLIENT CHATTED (command 124) Server > Client
Json Payload:
* string Text: Chat text (max 128 characters)
* string Name: Client name that sent text

### AUTH (command 125) Client > Server
Json Payload:
* string AuthToken: Authentication Token

### CLIENTAUTHED (command 126) Server > Client - Reliable
Json Payload:
* string Ip: Client Ip
* string Port: Client Port

### ACK (command 127) Client > Server && Server > Client
Binary Payload:
* Id (UInt): Packet id to check

## Host info
Hostname: unbit0016.uwsgi.it (server's local 10.0.0.238)

Port: 20234
