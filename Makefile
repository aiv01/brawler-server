all: build restart

build:
	mcs Brawler-server/Program.cs

restart:
	touch Brawler-server/Brawler-server.ini