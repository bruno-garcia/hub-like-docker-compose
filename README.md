# PoC for a docker-compose containing all dependencies of the Hub project

The objective of this repo is to serve as a PoC for a set of Dockerfile/compose that could aid 
in bootstrapping an integration environment for automated tests for a non public project called Hub.
For this PoC, a simplified version of this project will be recreated. The goal is simply to reach 
all dependencies. In other words: it's all dummy code.

The repo is composed by a Angular 4 SPA backed by a web API project written 
in C# and .NET Core 2.0. This API backend has 3 external dependencies: MongoDB, Redis and yet 
another web API called Envoy. Envoy is backed by a SQL Server database.

        +--------------------+
        | SPA                |
        | Angular CLI 1.3.0  |
        | Docker: nginx      |
        +----------+---------+
                   |
                   |                  +---------------------+
    +--------------v--------------+-->+ Redis               |
    | API                         |   | Docker: redis:alpine|
    | .NET Core 2.0 Preview 2     |   +---------------------+
    | Docker: dotnet:2.0-runtime  |   +---------------------+
    +--------------+--------------+-->+ MongoDB             |
                   |                  | Docker: mongo:3.4.7 |
                   |                  +---------------------+
    +--------------v--------------+
    | Envoy                       |
    | .NET Core 1.1               |
    | Docker: dotnet:1.1-runtime  |
    +--------------+--------------+
                   |
                   |
    +--------------v--------------+
    | SQL Server                  |
    | mssql on Linux              |
    | Docker: mssql-server-linux  |
    +-----------------------------+

### A few findings while settings this up:

#### StackExchange.Redis on .NET Core 1.1:
The Api project could not be done with .NET Core 1.1 due to [StackExchange.Redis not supporting DNS 
resolution on Linux](/StackExchange/StackExchange.Redis/issues/463).
That's an issue due to the container linking by name. It will fail with:

`This platform does not support connecting sockets to DNS endpoints via the instance Connect and ConnectAsync methods, due to the potential for a host name to map to multiple IP addresses and sockets becoming invalid for use after a failed connect attempt. Use the static ConnectAsync method, or provide to the instance methods the specific IPAddress desired.`

.NET Core 2.0 solves this problem but it's still Preview 2. 

#### Angular CLI 1.3.0 environment configuration
Been able to set environment variables on docker-compose.yml or at Dockerfile or finally directly on 
the shell before running something is extremely powerful. With the .NET Core apps one could even override 
those settings with command-line arguments. 

Unfortunately, Angular CLI 1.3.0 doesn't support accessing environment variables.
Ideally, access to `process.env` at build time from `environment.ts` would do the job.
There's [a discussion going on](/angular/angular-cli/issues/4318) on Angular CLI repo, hopefully support will
be added soon.
Until that, one of the work around proposed could be used. For this PoC I decided to add a `docker` environment.
This way, before running `docker-compose up`, the configuration file `environment.docker.ts` can be patched 
with the desired external api endpoint (e.g: http://docker-host-address:5000)

#### Exposing containers
Notice all containers listed on `docker-compose.yml` have a `ports:` section. That is simply to ease
testability from outside of the docker network. It allows hitting any of those services through the 
docker host address.


#### Dockerfile: Empty array on CMD
For the .NET Core applications, it's possible to override the configuration by passing arguments to the 
executable. For example, the 'api' project could run from a development machine while using all 
dependencies bootstrapped through `docker-compose up`:

`C:\>dotnet run -- Redis=athens.lan:6379 Envoy=http://athens.lan:5001 Mongo=mongodb://athens.lan:27017/^?connectTimeoutMS=2000`

- `athens.lan` is the hostname of the Docker host.
- ^ is the escape character for the Windows cmd

To achieve that, the .NET application has to take command line arguments into its ConfigurationBuilder:

`.AddCommandLine(args)` from `Microsoft.Extensions.Configuration.CommandLine` package.
- [See example](/bruno-garcia/hub-like-docker-compose/blob/master/api/Program.cs)

In order to pass arguments to a container, after the `ENTRYPOINT`, it's required to define a `CMD`.
With an empty array:

`CMD []`
- [See example](/bruno-garcia/hub-like-docker-compose/blob/master/api/Dockerfile)

Nothing is provided to the executable by default. While still able to change configuration without
the need to rebuild the image:

`$ docker run -ti hublike_api Redis=athens.lan:6379 Envoy=http://athens.lan:5001 Mongo=mongodb://athens.lan:27017/\?connectTimeoutMS=2000`
- \ is the escape character in bash

  
