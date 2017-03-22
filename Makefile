all: build restart

build:
	mcs Program.cs

restart:
	touch Brawler-server.ini