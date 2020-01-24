FROM dist.hosts.rfi:5000/dotnet/core/sdk:3.1-alpine AS build
COPY / /src
RUN dotnet publish /src/microservice-framework -c Release -o /app \
 && dotnet nuget push /src/microservice-framework/bin/Release/*.nupkg --source http://dist.hosts.rfi:5555 --skip-duplicate