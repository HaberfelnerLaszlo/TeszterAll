#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TeszterTeams/TeszterTeams.csproj", "TeszterTeams/"]
COPY ["TeszterLibrary/TeszterLibrary.csproj", "TeszterLibrary/"]
RUN dotnet restore "TeszterTeams/TeszterTeams.csproj"
COPY . .
WORKDIR "/src/TeszterTeams"
RUN dotnet build "TeszterTeams.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TeszterTeams.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TeszterTeams.dll"]