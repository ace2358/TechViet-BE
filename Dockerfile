# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY techviet_be/techviet_be.csproj techviet_be/
RUN dotnet restore techviet_be/techviet_be.csproj

COPY techviet_be/ techviet_be/
RUN dotnet publish techviet_be/techviet_be.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "techviet_be.dll"]
