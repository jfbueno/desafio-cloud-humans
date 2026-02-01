FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ClaudiaWebApi/ClaudiaWebApi.csproj ClaudiaWebApi/
RUN dotnet restore ClaudiaWebApi/ClaudiaWebApi.csproj

COPY ClaudiaWebApi/ ClaudiaWebApi/
WORKDIR /src/ClaudiaWebApi
RUN dotnet build ClaudiaWebApi.csproj -c Release -o /app/build

RUN dotnet publish ClaudiaWebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ClaudiaWebApi.dll"]
