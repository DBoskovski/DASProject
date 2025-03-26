FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN mkdir -p /app/keys
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://*:7079
EXPOSE 7079
ENTRYPOINT ["dotnet", "196119.dll"]