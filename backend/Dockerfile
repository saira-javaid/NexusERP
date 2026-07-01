FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/NexusERP.API/NexusERP.API.csproj", "src/NexusERP.API/"]
COPY ["src/NexusERP.Infrastructure/NexusERP.Infrastructure.csproj", "src/NexusERP.Infrastructure/"]
COPY ["src/NexusERP.Application/NexusERP.Application.csproj", "src/NexusERP.Application/"]
COPY ["src/NexusERP.Domain/NexusERP.Domain.csproj", "src/NexusERP.Domain/"]
RUN dotnet restore "src/NexusERP.API/NexusERP.API.csproj"
COPY . .
WORKDIR "/src/src/NexusERP.API"
RUN dotnet build "NexusERP.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NexusERP.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NexusERP.API.dll"]
