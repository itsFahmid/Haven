FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["haven/obhoy.csproj", "haven/"]
RUN dotnet restore "haven/obhoy.csproj"
COPY . .
WORKDIR "/src/haven"
RUN dotnet publish "obhoy.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=""
EXPOSE 8080
ENTRYPOINT ["dotnet", "Obhoy.dll"]
