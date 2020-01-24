FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
COPY / /src
RUN dotnet publish /src/proceficator -c Release -o /app
RUN dotnet nuget push /app/*.nupkg -source http://dist.hosts.rfi:5555 --skip-duplicate
