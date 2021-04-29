FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/IdentityServerApi.STS.Identity/IdentityServerApi.STS.Identity.csproj", "src/IdentityServerApi.STS.Identity/"]
COPY ["src/IdentityServerApi.Admin.EntityFramework.Shared/IdentityServerApi.Admin.EntityFramework.Shared.csproj", "src/IdentityServerApi.Admin.EntityFramework.Shared/"]
COPY ["src/IdentityServerApi.Shared/IdentityServerApi.Shared.csproj", "src/IdentityServerApi.Shared/"]
RUN dotnet restore "src/IdentityServerApi.STS.Identity/IdentityServerApi.STS.Identity.csproj"
COPY . .
WORKDIR "/src/src/IdentityServerApi.STS.Identity"
RUN dotnet build "IdentityServerApi.STS.Identity.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IdentityServerApi.STS.Identity.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENTRYPOINT ["dotnet", "IdentityServerApi.STS.Identity.dll"]