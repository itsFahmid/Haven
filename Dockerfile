FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["haven/haven.csproj", "haven/"]
RUN dotnet restore "haven/haven.csproj"
COPY . .
WORKDIR "/src/haven"
RUN dotnet publish "haven.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Haven.dll"]
