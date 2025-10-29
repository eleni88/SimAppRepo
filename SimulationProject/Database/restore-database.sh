#!/bin/bash

set -euo pipefail

: "${DB_NAME:?DB_NAME is required}"
: "${BAK_FILE:?BAK_FILE is required}"
: "${MSSQL_BACKUP_DIR:=/var/opt/mssql/backup}"
: "${MSSQL_DATA_DIR:=/var/opt/mssql/data}"
: "${MSSQL_SA_PASSWORD:?MSSQL_SA_PASSWORD is required}"
: "${SQLCMD:=/opt/mssql-tools18/bin/sqlcmd}"

LOG_FILE="${MSSQL_DATA_DIR}/${DB_NAME}_log.ldf"
DATA_FILE="${MSSQL_DATA_DIR}/${DB_NAME}.mdf"
BAK_PATH="${MSSQL_BACKUP_DIR}/${BAK_FILE}"

if [ ! -f "$BAK_PATH" ]; then
  echo "Backup file not found at $BAK_PATH"
  exit 1
fi

echo "Restoring database ${DB_NAME}"
"${SQLCMD}" \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -l 60 \
    -Q "RESTORE DATABASE ${DB_NAME} FROM DISK = '${BAK_PATH}' WITH MOVE '${DB_NAME}' TO '${DATA_FILE}', MOVE '${DB_NAME}_Log' TO '${LOG_FILE}'"

echo "Restore completed."