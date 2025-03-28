FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /src
COPY ["UnchainexWallet.Backend/UnchainexWallet.Backend.csproj", "UnchainexWallet.Backend/"]
COPY ["UnchainexWallet/UnchainexWallet.csproj", "UnchainexWallet/"]
RUN dotnet restore "UnchainexWallet.Backend/UnchainexWallet.Backend.csproj"
COPY . .
WORKDIR "/src/UnchainexWallet.Backend"
RUN dotnet build "UnchainexWallet.Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UnchainexWallet.Backend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UnchainexWallet.Backend.dll"]
