all: build restart

build:
	mcs Program.cs

restart:
	touch Brawler-server.ini

tests:
	mcs Brawler-server-tests/
	nunit 