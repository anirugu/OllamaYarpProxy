FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["OllamaYarpProject.csproj", "./"]
RUN dotnet restore "./OllamaYarpProject.csproj"

# Copy the rest and publish
COPY . .
RUN dotnet publish "OllamaYarpProject.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:11434
EXPOSE 11434
ENTRYPOINT ["dotnet", "OllamaYarpProject.dll"]
