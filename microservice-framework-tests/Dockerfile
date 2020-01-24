FROM dist.hosts.rfi:5000/dotnet/core/sdk:3.1-alpine AS build
COPY / /src
RUN dotnet test /src/proceficator-tests