# N.B.! Run this from the folder containing the .-sln file! otherwise dotnet restore will fail!!!
# docker build -t publicapi -f ./src/PublicApi/Dockerfile .
# docker tag publicapi shokieshoponweb.azurecr.io/publicapi:v<n>
# docker push shokieshoponweb.azurecr.io/publicapi:v<n>




# you dumbass. you first need to rebuild the image, THEN publish it. otherwise i'm jsut republishing the same image... or not?

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY . .
#COPY ["src/PublicApi/PublicApi.csproj", "./PublicApi/"]
#RUN dotnet restore "./PublicApi/PublicApi.csproj"
#COPY . .
WORKDIR "/app/src/PublicApi"
RUN dotnet restore

RUN dotnet build "./PublicApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./PublicApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PublicApi.dll"]