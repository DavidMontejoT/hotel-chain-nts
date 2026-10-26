# syntax=docker/dockerfile:1

# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first so this layer is cached while only source files change.
COPY HotelChain.sln ./
COPY src/HotelChain.Domain/HotelChain.Domain.csproj src/HotelChain.Domain/
COPY src/HotelChain.Application/HotelChain.Application.csproj src/HotelChain.Application/
COPY src/HotelChain.Infrastructure/HotelChain.Infrastructure.csproj src/HotelChain.Infrastructure/
COPY src/HotelChain.Migrations.Sqlite/HotelChain.Migrations.Sqlite.csproj src/HotelChain.Migrations.Sqlite/
COPY src/HotelChain.Migrations.SqlServer/HotelChain.Migrations.SqlServer.csproj src/HotelChain.Migrations.SqlServer/
COPY src/HotelChain.Api/HotelChain.Api.csproj src/HotelChain.Api/
RUN dotnet restore src/HotelChain.Api/HotelChain.Api.csproj

COPY src/ src/
RUN dotnet publish src/HotelChain.Api/HotelChain.Api.csproj -c Release -o /app --no-restore

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "HotelChain.Api.dll"]
