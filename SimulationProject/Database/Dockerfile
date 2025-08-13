# Use the official Microsoft SQL Server 2022 Linux base image
FROM mcr.microsoft.com/mssql/server:2022-latest

# Switch to root to install tar
USER root

# Install tar 
RUN apt-get update && apt-get install -y tar && apt-get clean && rm -rf /var/lib/apt/lists/*

# Switch back to mssql user
USER mssql