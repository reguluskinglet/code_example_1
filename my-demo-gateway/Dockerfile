FROM v-ccng-stage-nexus.stage.demo.ru:8084/dotnet/core/sdk:3.1 AS build
WORKDIR /app

COPY . .
WORKDIR /app/demo.DemoGateway
RUN dotnet restore -s http://v-ccng-stage-nexus.stage.demo.ru:8081/repository/ccng-nuget/
RUN dotnet publish -c Release -o out

FROM v-ccng-stage-nexus.stage.demo.ru:8084/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/demo.DemoGateway/out ./

ENV ASPNETCORE_ENVIRONMENT=${NEXUS_DOCKER_REGISTRY_PULL}
ENV ASPNETCORE_VERSION=${VERSION}
ENTRYPOINT ["dotnet", "demo.DemoGateway.dll"]
