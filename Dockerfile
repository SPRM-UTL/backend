# 1. Capa de ejecución (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# 2. Capa de compilación (SDK)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["backend/backend.csproj", "backend/"]
RUN dotnet restore "backend/backend.csproj"

# Copiar todo el resto del código fuente
COPY . .

# Nos movemos firmemente a la carpeta del proyecto para los siguientes pasos
WORKDIR "/src/backend"

# Compilar el proyecto
RUN dotnet build "backend.csproj" -c Release -o /app/build

# 3. Capa de publicación
FROM build AS publish
RUN dotnet publish "backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Configurar el contenedor final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "backend.dll"]
