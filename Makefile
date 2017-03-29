all: build tests restart

tests: build-tests run-tests

dotnet=${HOME}/.dotnet/dotnet
export FrameworkPathOverride=/usr/lib/mono/4.5/
nunit-console=mono ${HOME}/.nuget/packages/nunit.consolerunner/3.6.1/tools/nunit3-console.exe

build:
	cd Brawler-server/ && $(dotnet) build

build-tests:
	cd Brawler-server-tests/ && $(dotnet) build

run-tests:
	cd Brawler-server-tests/ && $(nunit-console) bin/Debug/Brawler-server-tests.dll

restart:
	touch Brawler-server/Brawler-server.ini