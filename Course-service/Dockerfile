#docker build -f Course-service\Dockerfile -t course-api .

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Course-service/Course-service.csproj", "Course-service/"]
COPY ["SharedModels/SharedModels.csproj", "SharedModels/"]
RUN dotnet restore "Course-service/Course-service.csproj"
COPY . .
WORKDIR "/src/Course-service"
RUN dotnet build "Course-service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Course-service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT Development
ENTRYPOINT ["dotnet", "Course-service.dll"]