FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ThomasPool-api.csproj", "./"]
RUN dotnet restore

COPY . .
RUN dotnet publish "ThomasPool-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV PORT=8080
ENV ASPNETCORE_HTTP_PORTS=${PORT}

USER app

ENTRYPOINT ["dotnet", "ThomasPool-api.dll"]