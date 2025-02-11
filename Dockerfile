# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG TARGETARCH
WORKDIR /source

# Copy project file and restore as distinct layers
COPY --link *.csproj .
RUN dotnet restore -a $TARGETARCH

# Copy source code and publish app
COPY --link . .
RUN dotnet publish --no-restore -a $TARGETARCH -o /app


# Enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md
# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine

# Globalization
ENV \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8
RUN apk add --no-cache \
    icu-data-full \
    icu-libs \
    tzdata \
    openssl

EXPOSE 8000
WORKDIR /app
COPY --link --from=build /app .
USER $APP_UID
RUN mkdir -p /https && \
    chown -R $APP_UID:$APP_UID /https
ENTRYPOINT ["./Citlali"]