#!/bin/bash

JOB_STAGE=${1}
PROJECT_NAME=${2}
PROJECT_DIR=${3}
PIPELINE_ID=${4}
DOCKERFILE="/${PROJECT_DIR}/Dockerfile"


if [[ $JOB_STAGE == "build" ]]; then

tee $DOCKERFILE > /dev/null << ---
FROM dist.hosts.rfi:5000/dotnet/core/sdk:3.1-alpine
COPY / /src
RUN dotnet publish /src/${PROJECT_NAME} -c Release
---
docker pull dist.hosts.rfi:5000/${PROJECT_NAME}:builder || true
docker build /${PROJECT_DIR} \
  --tag dist.hosts.rfi:5000/${PROJECT_NAME}:builder \
  --cache-from dist.hosts.rfi:5000/${PROJECT_NAME}:builder || exit 1
docker push dist.hosts.rfi:5000/${PROJECT_NAME}:builder || exit 1


elif [[ $JOB_STAGE == "deploy-pre-production" ]]; then

tee $DOCKERFILE > /dev/null << ---
FROM dist.hosts.rfi:5000/dotnet/core/sdk:3.1-alpine
COPY / /src
RUN dotnet publish /src/${PROJECT_NAME} -c Release
RUN dotnet nuget push /src/${PROJECT_NAME}/bin/Release/*.nupkg --source http://dist.hosts.rfi:5555 --skip-duplicate \
 && dotnet nuget push /src/${PROJECT_NAME}/bin/Release/*.snupkg --source http://dist.hosts.rfi:5555 --skip-duplicate
---
docker pull dist.hosts.rfi:5000/${PROJECT_NAME}:builder || true
docker build /${PROJECT_DIR} --cache-from dist.hosts.rfi:5000/${PROJECT_NAME}:builder


elif [[ $JOB_STAGE == "deploy-production" ]]; then

tee $DOCKERFILE > /dev/null << ---
FROM dist.hosts.rfi:5000/dotnet/core/sdk:3.1-alpine
COPY / /src
RUN dotnet publish /src/${PROJECT_NAME} -c Release
RUN dotnet nuget push /src/${PROJECT_NAME}/bin/Release/*.nupkg --source http://dist.hosts.rfi:5555 --skip-duplicate \
 && dotnet nuget push /src/${PROJECT_NAME}/bin/Release/*.snupkg --source http://dist.hosts.rfi:5555 --skip-duplicate
---
docker pull dist.hosts.rfi:5000/${PROJECT_NAME}:builder || true
docker build /${PROJECT_DIR} --cache-from dist.hosts.rfi:5000/${PROJECT_NAME}:builder


fi

exit 0