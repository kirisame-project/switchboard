FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build

ADD . /app

WORKDIR /app

RUN dotnet restore

WORKDIR /app/Switchboard

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime

WORKDIR /app

COPY --from=build /app/Switchboard/out ./

ENTRYPOINT ["dotnet", "Switchboard.dll"]
