# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY VisionaryAnalytics.sln ./
COPY VisionaryAnalytics.Domain/VisionaryAnalytics.Domain.csproj VisionaryAnalytics.Domain/
COPY VisionaryAnalytics.Application/VisionaryAnalytics.Application.csproj VisionaryAnalytics.Application/
COPY VisionaryAnalytics.Infrastructure/VisionaryAnalytics.Infrastructure.csproj VisionaryAnalytics.Infrastructure/
COPY VisionaryAnalytics.Api/VisionaryAnalytics.Api.csproj VisionaryAnalytics.Api/
COPY VisionaryAnalytics.FrameWorker/VisionaryAnalytics.FrameWorker.csproj VisionaryAnalytics.FrameWorker/
COPY VisionaryAnalytics.VideoWorker/VisionaryAnalytics.VideoWorker.csproj VisionaryAnalytics.VideoWorker/

#RUN dotnet restore

COPY . .

RUN dotnet publish VisionaryAnalytics.Api/VisionaryAnalytics.Api.csproj -c Release -o /app/publish/api
RUN dotnet publish VisionaryAnalytics.FrameWorker/VisionaryAnalytics.FrameWorker.csproj -c Release -o /app/publish/frameworker
RUN dotnet publish VisionaryAnalytics.VideoWorker/VisionaryAnalytics.VideoWorker.csproj -c Release -o /app/publish/videoworker

# API image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS api
WORKDIR /app

RUN apt-get update && \
    apt-get install -y --no-install-recommends ffmpeg libfontconfig1 && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish/api .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
VOLUME ["/app/uploads"]
ENTRYPOINT ["dotnet", "VisionaryAnalytics.Api.dll"]

# Frame worker image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS frameworker
WORKDIR /app

RUN apt-get update && \
    apt-get install -y --no-install-recommends ffmpeg libfontconfig1 && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish/frameworker .
VOLUME ["/app/uploads"]
ENTRYPOINT ["dotnet", "VisionaryAnalytics.FrameWorker.dll"]

# Video worker image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS videoworker
WORKDIR /app

RUN apt-get update && \
    apt-get install -y --no-install-recommends libfontconfig1 && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish/videoworker .
VOLUME ["/app/uploads"]
ENTRYPOINT ["dotnet", "VisionaryAnalytics.VideoWorker.dll"]
