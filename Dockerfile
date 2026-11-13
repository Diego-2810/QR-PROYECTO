FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["TarjeTaxi/TarjeTaxi.csproj", "TarjeTaxi/"]
RUN dotnet restore "TarjeTaxi/TarjeTaxi.csproj"
COPY . .
WORKDIR "/src/TarjeTaxi"
RUN dotnet publish "TarjeTaxi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TarjeTaxi.dll"]
