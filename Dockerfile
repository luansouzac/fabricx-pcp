FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

COPY src/FabricX.Domain/FabricX.Domain.csproj src/FabricX.Domain/
COPY src/FabricX.Application/FabricX.Application.csproj src/FabricX.Application/
COPY src/FabricX.Infrastructure/FabricX.Infrastructure.csproj src/FabricX.Infrastructure/
COPY src/FabricX.Api/FabricX.Api.csproj src/FabricX.Api/
RUN dotnet restore src/FabricX.Api/FabricX.Api.csproj

COPY src/ src/
COPY database/ database/
RUN dotnet publish src/FabricX.Api/FabricX.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Development \
    ConnectionStrings__Default="Data Source=/data/fabricx.db"

VOLUME /data
EXPOSE 8080
ENTRYPOINT ["dotnet", "FabricX.Api.dll"]
