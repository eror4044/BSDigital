# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# install EF tool
RUN dotnet tool install --global dotnet-ef --version 9.*
ENV PATH="$PATH:/root/.dotnet/tools"

COPY BSD_fullstack_API.csproj ./
RUN dotnet restore "./BSD_fullstack_API.csproj"

COPY . .
RUN dotnet build "BSD_fullstack_API.csproj" -c Release -o /app/build

# publish stage
FROM build AS publish
RUN dotnet publish "BSD_fullstack_API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BSD_fullstack_API.dll"]
