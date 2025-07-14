# Base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src


COPY ["LoginPage/LoginPage.csproj", "LoginPage/"]
RUN dotnet restore "LoginPage/LoginPage.csproj"
COPY . .
WORKDIR "/src/LoginPage"
RUN dotnet build "LoginPage.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish image
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LoginPage.csproj" -c $BUILD_CONFIGURATION -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LoginPage.dll"]
