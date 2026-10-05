FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["QrProfile/QrProfile.csproj", "QrProfile/"]
RUN dotnet restore "QrProfile/QrProfile.csproj"
COPY . .
WORKDIR "/src/QrProfile"
RUN dotnet publish "QrProfile.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "QrProfile.dll"]
