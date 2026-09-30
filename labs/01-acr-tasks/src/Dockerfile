# syntax=docker/dockerfile:1

# --- Estagio de build ---
# Imagem base do SDK: e a "tag estavel de imagem base" da unidade 4.
# Um gatilho de base image update no ACR recompila esta imagem quando o
# provedor publica patch de seguranca nesta tag.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DocumentInferenceApi.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

# --- Estagio de runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Versao da imagem passada como argumento de build e promovida a variavel de
# ambiente. E assim que o endpoint /version sabe qual tag foi construida.
ARG IMAGE_VERSION=dev-local
ENV IMAGE_VERSION=${IMAGE_VERSION}
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "DocumentInferenceApi.dll"]
