FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ./src ./

RUN dotnet restore Backend.Web/Backend.Web.csproj
RUN dotnet publish -c Release -o out Backend.Web/Backend.Web.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Backend.Web.dll"]
