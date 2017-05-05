# brawler-server
Server Side Of the Second Year Project 2016/2017 (2A)

Sprint 1 - March 20 - April 3
-

* Makefile for mono
* JOIN, LEAVE, KICK, UPDATE

- [x] 20170320 Absolute position and rotation at specific rate
- [ ] 20170320 Action replication from authority client to all clients


(sprint meeting)


Sprint 2 - April 3 - April 19
-

* Non-blocking http/https communication with the services server
* add the AUTH command (reliable), it contains the token, check ip address and token on the services
* the services server confirms the authorization givin the nickname/battlename to the server
* allow JOIN by checking the stored endpoint
* ignore unauth packets
* ensure non blocking sockets

(sprint meeting)

Sprint 3 - April 19 - May 3
-

* Implement packet text serialization (json ?)
* Store serialized packets into elasticsearch (spare)
* Send data to elasticsearch at regular interval (spare)
* Implement interface for sending json data to external storages (like elasticsearch)

20170428 - Install a Linux System (Ubuntu) and install elasticsearch

(sprint meeting)

Sprint 4 - May 3 - May 17
-

* (TD) study elasticsearch
* true non-blocking http calls
* (TD) lobbying and movements
* CHAT command

(sprint meeting)

Sprint 5 - May 17 - May 31
-

(sprint meeting)

Sprint 6 - May 31 - Jun 14 
-

(sprint meeting)

Sprint 7 - Jun 14 - Jun 28
-

(sprint meeting)
