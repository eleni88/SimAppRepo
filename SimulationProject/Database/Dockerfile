ARG SCRIPTS_PATH=/var/opt/mssql/scripts

# Use the official Microsoft SQL Server 2022 Linux base image
FROM mcr.microsoft.com/mssql/server:2022-latest

ARG SCRIPTS_PATH

ENV MSSQL_BACKUP_DIR=/var/opt/mssql/backup \
    MSSQL_DATA_DIR=/var/opt/mssql/data \
    PATH="$PATH:/opt/mssql-tools18/bin:/opt/mssql-tools/bin"

# Switch to root to create db_restore folder, copy the db_restore script and copy the entrypoint script
USER root

# copy the entrypoint script
COPY --chown=mssql:mssql mssql-entrypoint.sh /bin
RUN chmod +x /bin/mssql-entrypoint.sh

# create db_restore folder and copy the db_restore script
RUN mkdir -p ${SCRIPTS_PATH}
COPY --chown=mssql:mssql restore-database.sh ${SCRIPTS_PATH}
RUN chmod +x ${SCRIPTS_PATH}/restore-database.sh

ENV SCRIPTS_PATH=${SCRIPTS_PATH}

# Switch back to mssql user
USER mssql

EXPOSE 1433
ENTRYPOINT ["/bin/mssql-entrypoint.sh"]