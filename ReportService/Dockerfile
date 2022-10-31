FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ReportService/ReportService.csproj", "ReportService/"]
COPY ["SharedModels/SharedModels.csproj", "SharedModels/"]
RUN dotnet restore "ReportService/ReportService.csproj"
COPY . .
WORKDIR "/src/ReportService"
RUN dotnet build "ReportService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReportService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT Development
ENTRYPOINT ["dotnet", "ReportService.dll"]