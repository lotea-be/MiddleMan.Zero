FROM mcr.microsoft.com/devcontainers/dotnet:1-9.0-bookworm

# Remove the problematic Yarn repository with expired GPG key
RUN rm -f /etc/apt/sources.list.d/yarn.list && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*
