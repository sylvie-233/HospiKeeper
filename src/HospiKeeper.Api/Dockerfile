# 使用本机已有的 .NET 10 SDK / ASP.NET 镜像，构建 HospiKeeper.Api。
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore src/HospiKeeper.Api/HospiKeeper.Api.csproj
RUN dotnet publish src/HospiKeeper.Api/HospiKeeper.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HospiKeeper.Api.dll"]
