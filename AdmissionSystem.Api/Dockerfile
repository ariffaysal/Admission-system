# Multi-stage build for .NET 8 API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore
COPY ["AdmissionSystem.Api/AdmissionSystem.Api.csproj", "AdmissionSystem.Api/"]
RUN dotnet restore "AdmissionSystem.Api/AdmissionSystem.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/AdmissionSystem.Api"
RUN dotnet build "AdmissionSystem.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "AdmissionSystem.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port 8080
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AdmissionSystem.Api.dll"]
