FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /App

RUN apt-get update 

COPY --from=build-env /App/out .

ENV ASPNETCORE_URLS=http://*:3000

ARG DOCKERFILE_PROJECT

ENV RUNPATH=$DOCKERFILE_PROJECT.dll

ENTRYPOINT dotnet $RUNPATH