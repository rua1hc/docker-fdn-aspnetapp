FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /source
EXPOSE 80

# copy csproj and restore as distinct layers
COPY ./*.csproj .
# RUN dotnet restore --use-current-runtime
RUN dotnet restore

# copy everything else and build app
COPY . .
RUN dotnet publish -c Release -o /app --self-contained false --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ServiceStub.dll"]
