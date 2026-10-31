#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8000
EXPOSE 8001

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TeszterWeb/TeszterWeb.csproj", "TeszterWeb/"]
COPY ["TeszterLibrary/TeszterLibrary.csproj", "TeszterLibrary/"]
COPY ["Directory.Packages.props", "."]
RUN dotnet restore "./TeszterWeb/TeszterWeb.csproj"
COPY . .
WORKDIR "/src/TeszterWeb"
RUN dotnet build "./TeszterWeb.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TeszterWeb.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TeszterWeb.dll"]