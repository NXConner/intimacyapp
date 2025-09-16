# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY IntimacyAI.Client.csproj ./
RUN dotnet restore IntimacyAI.Client.csproj

COPY . ./
RUN dotnet publish IntimacyAI.Client.csproj -c Release -o /app/publish /p:GZipCompressBlazorFiles=true /p:UseAppHost=false

# Serve with nginx
FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
CMD ["nginx","-g","daemon off;"]
