FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY *.sln ./
COPY NuGet.config ./
COPY Sample.Microservice.Device.Api/Sample.Microservice.Device.Api.csproj Sample.Microservice.Device.Api/
RUN dotnet restore Sample.Microservice.Device.Api/Sample.Microservice.Device.Api.csproj

COPY . .
WORKDIR /src/Sample.Microservice.Device.Api
RUN dotnet build Sample.Microservice.Device.Api.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish Sample.Microservice.Device.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sample.Microservice.Device.Api.dll"]
