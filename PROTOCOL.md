# Protocol

## Header  
4 byte (UInt32): Id  
* Packet Id;

4 byte (uint): Timestamp  
* Time elapsed from the start of the server (in milliseconds);

1 byte: Packet Infos  
* Most Significant Bit: 1 if reliable, otherwise 0;
* Other 7 bits: command;

## Payload

### JOIN (command 0) Client > Server
Json Payload:
* int MatchId: Match Identifier;

### CLIENT JOINED (command 1) Server > Client - Reliable
Json Payload:
* bool CanJoin: Has player joined;
* string Reason: Reason why player can't join (May be empty);
* string Name: Player Name;
* uint Id: Client Unique Identifier;
* bool IsReady: Is Client Ready to join battle;

### LEAVE (command 2) Client > Server
Empty Payload

### CLIENT LEFT (command 3) Server > Client - Reliable
Json Payload:  
* uint Id: Client Unique Identifier;
* string Reason: Reason for leaving (kicked, quit, ...);

### MOVE (command 4) Client > Server
Binary Payload:  
* MoveType (byte): Movement type of the player (like camera lock);
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;
* Health (int): Player health;

### CLIENT MOVED (command 5) Server > Client
Binary Payload:  
* Id (uint): Client Unique Identifier;
* MoveType (byte): Movement type of the player (like camera lock);
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;
* Health (int): Player health;

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
* Id (uint): Client Unique Identifier;
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
* TauntId (byte): Taunt type identifier

### CLIENT TAUNTED (command 9) Server > Client 
Binary Payload:
* Id (uint): Client Unique Identifier
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;
* TauntId (byte): Taunt type identifier

### LIGHT ATTACK (command 10) Client > Server
Binary Payload:
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;

### CLIENT LIGHT ATTACKED (command 11) Server > Client
Binary Payload:
* Id (uint): Client Unique Identifier;
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;

### HEAVY ATTACK (command 12) Client > Server
Binary Payload:
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;

### CLIENT HEAVY ATTACKED (command 13) Server > Client
Binary Payload:
* Id (uint): Client Unique Identifier;
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;

### HIT (command 14) Client > Server
Binary Payload:
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;
* Damage (float): Damage got

### CLIENT HITTED (command 15) Server > Client
Binary Payload:
* Id (uint): Client Unique Identifier;
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;
* Health (float): Client Health After Hit;

### SWAP WEAPON (command 16) Client > Server
Binary Payload:
* Id (uint): Weapon Object Id;

### CLIENT SWAPPED WEAPON (command 17) Server > Client
Binary Payload:
* Id (uint): Weapon Object Id;
* Id (uint): Client Id;

### SPAWN OBJECT (command 98) Server > Client
Binary Payload:
* ObjectId (byte): The object type to spawn;
* X (float): X position of the player;
* Y (float): Y position of the player;
* Z (float): Z position of the player;
* Rx (float): X component of the quaternion rotation;
* Ry (float): Y component of the quaternion rotation;
* Rz (float): Z component of the quaternion rotation;
* Rw (float): W component of the quaternion rotation;
* Id (uint): Object Unique Identifier

### CLIENT WON (command 99) Server > Client
Binary Payload:
* Id (uint): Client unique identifier;

### EMPOWER PLAYER (command 100) Web Service > Server
Json Payload:
* string Ip: Player Ip to empower;
* int Port: Player port;
* int EmpowerType: Type of bonus to give to the player;

### EMPOWER PLAYER (command 100) Server > Client
Binary Payload:
* id (uint): Client unique identifier;
* fury (int): Fury amount;

### ENTER ARENA (command 115) Server > Client
Json Payload:
* uint Id: Client Unique Identifier;
* float X: X position of the player;
* float Y: Y position of the player;
* float Z: Z position of the player;
* float Rx: X component of the quaternion rotation;
* float Ry: Y component of the quaternion rotation;
* float Rz: Z component of the quaternion rotation;
* float Rw: W component of the quaternion rotation;
	
### LEAVE ARENA (command 116) Server > Client
Empty Payload

### READY (command 117) Client > Server
Json Payload:
* int PrefabId: Client selected character Id;

### CLIENT READY (command 118) Server > Client - Reliable
Json Payload:
* int PrefabId: Client selected character Id;
* uint Id: Client Unique Identifier;

### NOTREADY (command 119) Client > Server
Empty Payload

### CLIENT NOTREADY (command 120) Server > Client - Reliable
Json Payload:
* uint Id: Client Unique Identifier;

### PING (command 121) Client > Server
Empty Payload

### PING (command 121) Server > Client
Binary Payload:
* uint Id: Client (PINGED) Unique Identifier;

### PONG (command 122) Client > Server
Binary Payload:
* uint Id: Client (PINGED) Unique Identifier;

### PONG (command 122) Server > Client
Binary Payload:
* uint Id: Client (PONGED) Unique Identifier;

### CHAT (command 123) Client > Server
Json Payload:
* string Text: Chat text (max 128 characters);
* string Name: (from mobile App) Client name;

### CLIENT CHATTED (command 124) Server > Client
Json Payload:
* string Text: Chat text (max 128 characters);
* string Name: Client name that sent text;

### AUTH (command 125) Client > Server
Json Payload:
* string AuthToken: Authentication Token;

### CLIENTAUTHED (command 126) Server > Client - Reliable
Json Payload:
* string Ip: Client Ip;
* string Port: Client Port;

### ACK (command 127) Client > Server && Server > Client
Binary Payload:
* Id (UInt): Packet id to check;

## Host info
Hostname: unbit0016.uwsgi.it (server's local 10.0.0.238)

Port: 20234
