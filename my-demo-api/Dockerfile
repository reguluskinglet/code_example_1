FROM v-xxxx-nexus.demo.ru:8084/dotnet/core/sdk:3.1 AS build
WORKDIR /app

COPY . .
WORKDIR /app/demo.DemoApi.Service
RUN dotnet restore -s http://v-xxxx-nexus.demo.ru:8081/repository/xxxx-nuget/
RUN dotnet publish -c Release -o out

FROM v-xxxx-nexus.demo.ru:8084/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/demo.DemoApi.Service/out ./

ENV ASPNETCORE_ENVIRONMENT=${NEXUS_DOCKER_REGISTRY_PULL}
ENV ASPNETCORE_VERSION=${VERSION}
ENTRYPOINT ["dotnet", "demo.DemoApi.Service.dll"]
