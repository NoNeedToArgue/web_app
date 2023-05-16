FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["web_app1.csproj", "."]
RUN dotnet restore "./web_app1.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "web_app1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "web_app1.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "web_app1.dll"]