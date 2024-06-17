# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the csproj file and restore dependencies
COPY ["TipRecipe/TipRecipe.csproj", "TipRecipe/"]
RUN dotnet restore "TipRecipe/TipRecipe.csproj"

# Copy the remaining source code and build the application
COPY . .
WORKDIR "/src/TipRecipe"
RUN dotnet build "TipRecipe.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "TipRecipe.csproj" -c Release -o /app/publish

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TipRecipe.dll"]