# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
#COPY ../engine/examples ./
RUN dotnet publish -c Release -o out

#EXPOSE 80
#ENTRYPOINT ["dotnet", "run", "--server.urls", "http://0.0.0.0:80"]

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "aspnetapp.dll"]
EXPOSE 80
