# https://hub.docker.com/_/microsoft-dotnet

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source
EXPOSE 80

# copy csproj and restore as distinct layers
COPY aspnetapp/*.csproj .
RUN dotnet restore --use-current-runtime  

# copy everything else and build app
COPY aspnetapp/. .
RUN dotnet publish -c Release -o /app --use-current-runtime --self-contained false --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "aspnetapp.dll"]