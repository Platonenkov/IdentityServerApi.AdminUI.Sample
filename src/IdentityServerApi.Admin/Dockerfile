FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/IdentityServerApi.Admin/IdentityServerApi.Admin.csproj", "src/IdentityServerApi.Admin/"]
COPY ["src/IdentityServerApi.Admin.EntityFramework.Shared/IdentityServerApi.Admin.EntityFramework.Shared.csproj", "src/IdentityServerApi.Admin.EntityFramework.Shared/"]
COPY ["src/IdentityServerApi.Admin.EntityFramework.SqlServer/IdentityServerApi.Admin.EntityFramework.SqlServer.csproj", "src/IdentityServerApi.Admin.EntityFramework.SqlServer/"]
COPY ["src/IdentityServerApi.Admin.EntityFramework.PostgreSQL/IdentityServerApi.Admin.EntityFramework.PostgreSQL.csproj", "src/IdentityServerApi.Admin.EntityFramework.PostgreSQL/"]
COPY ["src/IdentityServerApi.Shared/IdentityServerApi.Shared.csproj", "src/IdentityServerApi.Shared/"]
COPY ["src/IdentityServerApi.Admin.EntityFramework.MySql/IdentityServerApi.Admin.EntityFramework.MySql.csproj", "src/IdentityServerApi.Admin.EntityFramework.MySql/"]
RUN dotnet restore "src/IdentityServerApi.Admin/IdentityServerApi.Admin.csproj"
COPY . .
WORKDIR "/src/src/IdentityServerApi.Admin"
RUN dotnet build "IdentityServerApi.Admin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IdentityServerApi.Admin.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENTRYPOINT ["dotnet", "IdentityServerApi.Admin.dll"]