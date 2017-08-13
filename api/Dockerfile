# 2.0 preview to avoid redis DNS resolve issue (StackExchange.Redis/issues/463):
# This platform does not support connecting sockets to DNS endpoints via the instance Connect and ConnectAsync methods, due to the potential for a host name to map to multiple IP addresses and sockets becoming invalid for use after a failed connect attempt. Use the static ConnectAsync method, or provide to the instance methods the specific IPAddress desired.

FROM microsoft/dotnet:2.0-sdk AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out -r debian.8-x64

# build runtime image
FROM microsoft/dotnet:2.0-runtime

WORKDIR /app
COPY --from=build-env /app/out ./
EXPOSE 5000/tcp
ENTRYPOINT ["dotnet", "api.dll"]
# To be able to pass parameters (e.g.: override configuration)
CMD []