all: build tests restart

tests: build-tests run-tests

build:
	cd Brawler-server/ && dotnet build

build-tests:
	cd Brawler-server-tests/ && dotnet build

run-tests:
	cd Brawler-server-tests/ && nunit-console bin/Debug/Brawler-server-tests.dll

restart:
	touch Brawler-server/Brawler-server.ini