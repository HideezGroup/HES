# Use base image Alpine Linux + Git
FROM alpine/git as base
WORKDIR /app
# Cloning Project
RUN git clone https://github.com/HideezGroup/HES /app/HES
# Use dotnet code sdk 5.0 image
FROM mcr.microsoft.com/dotnet/sdk:5.0-focal as build
WORKDIR /app
COPY --from=base /app .
WORKDIR /app/HES/HES.Web/
# Create directory and compiling application Hes
RUN mkdir -p  /opt/HES/ \
      && dotnet publish -c release -v d -o /opt/HES --runtime linux-x64 HES.Web.csproj \
      && cp Crypto_linux.dll /opt/HES/Crypto.dll
# Set work directory when container starting
WORKDIR /opt/HES/
			
