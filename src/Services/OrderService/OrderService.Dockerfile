# build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ./src/Services/OrderService/ ./OrderService/
WORKDIR /src/OrderService/OrderService.API
RUN dotnet restore

# Install EF tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet publish -c Release -o /app

# run

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8001
ENV ASPNETCORE_URLS=http://+:8001
ENTRYPOINT ["dotnet", "OrderService.API.dll"]
