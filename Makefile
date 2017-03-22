all: build restart

build:
	cd Brawler-server/ && mcs Program.cs Server/*.cs Utilities/*.cs /reference:References/Newtonsoft.Json.dll

restart:
	touch Brawler-server/Brawler-server.ini