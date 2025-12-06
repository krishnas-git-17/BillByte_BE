# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY Billbyte_BE.csproj .
RUN dotnet restore "./Billbyte_BE.csproj"

# Copy everything else
COPY . .

# Build and publish the project
RUN dotnet publish "./Billbyte_BE.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Billbyte_BE.dll"]
