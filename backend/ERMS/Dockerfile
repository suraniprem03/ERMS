# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["ERMS.sln", "./"]
COPY ["ERMS.API/ERMS.API.csproj", "ERMS.API/"]
COPY ["ERMS.Application/ERMS.Application.csproj", "ERMS.Application/"]
COPY ["ERMS.Domain/ERMS.Domain.csproj", "ERMS.Domain/"]
COPY ["ERMS.Infrastructure/ERMS.Infrastructure.csproj", "ERMS.Infrastructure/"]

# Restore dependencies
RUN dotnet restore

# Copy all the source code
COPY . .

# Build and Publish
WORKDIR "/src/ERMS.API"
RUN dotnet publish "ERMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (Render expects the app to bind to 0.0.0.0 on a specific port or default ASPNETCORE_HTTP_PORTS)
ENV ASPNETCORE_HTTP_PORTS=80
EXPOSE 80

ENTRYPOINT ["dotnet", "ERMS.API.dll"]
