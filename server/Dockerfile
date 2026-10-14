FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY GameSessionHub.Api.csproj ./
RUN dotnet restore GameSessionHub.Api.csproj

COPY . ./
RUN dotnet publish GameSessionHub.Api.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GameSessionHub.Api.dll"]
