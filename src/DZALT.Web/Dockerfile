FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim AS build
RUN apt-get update
RUN apt-get install -y sudo
RUN apt-get install -y ca-certificates curl gnupg
RUN mkdir -p /etc/apt/keyrings
RUN curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | sudo gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg
ENV NODE_MAJOR=20
RUN echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_$NODE_MAJOR.x nodistro main" | sudo tee /etc/apt/sources.list.d/nodesource.list
RUN apt-get update
RUN apt-get install nodejs -y
WORKDIR "/build"
COPY ["src/DZALT/DZALT.csproj", "src/DZALT/"]
COPY ["src/DZALT.Entities.Persistent.Postgres/DZALT.Entities.Persistent.Postgres.csproj", "src/DZALT.Entities.Persistent.Postgres/"]
COPY ["src/DZALT.Web/DZALT.Web.csproj", "src/DZALT.Web/"]
RUN dotnet restore "src/DZALT.Web/DZALT.Web.csproj"
COPY . .
WORKDIR "/build/src/DZALT.Web"
RUN dotnet publish "DZALT.Web.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS deploy
WORKDIR /app
COPY --from=build /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "DZALT.Web.dll"]