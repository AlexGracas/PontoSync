#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["PontoSync/PontoSync.csproj", "PontoSync/"]
RUN dotnet restore "PontoSync/PontoSync.csproj"
COPY . .
WORKDIR "/src/PontoSync"
RUN dotnet build "PontoSync.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PontoSync.csproj" -c Release -o /app/publish

FROM base AS final
ENV TZ=America/Sao_Paulo
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
RUN dpkg-reconfigure -f noninteractive tzdata
ADD /usr/local/share/ca-certificates/tre-root-v3.crt /usr/local/share/ca-certificates/tre-root-v3.crt
ADD /usr/local/share/ca-certificates/ca-cert-03.crt /usr/local/share/ca-certificates/ca-cert-03.crt
RUN chmod 644 /usr/local/share/ca-certificates/ca-cert-03.crt && update-ca-certificates
RUN chmod 644 /usr/local/share/ca-certificates/v3.crt && update-ca-certificates
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PontoSync.dll"]