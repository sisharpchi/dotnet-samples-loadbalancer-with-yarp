FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["gateway/Yarp.LoadBalancer/Yarp.LoadBalancer.csproj", "gateway/Yarp.LoadBalancer/"]
RUN dotnet restore "./gateway/Yarp.LoadBalancer/Yarp.LoadBalancer.csproj"
COPY . .
WORKDIR "/src/gateway/Yarp.LoadBalancer"
RUN dotnet build "./Yarp.LoadBalancer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Yarp.LoadBalancer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yarp.LoadBalancer.dll"]
