#!/bin/bash
set -euo pipefail

: "${ACCEPT_EULA:?ACCEPT_EULA must be set to Y}"
: "${MSSQL_SA_PASSWORD:?MSSQL_SA_PASSWORD is required}"
: "${SQLCMD:=/opt/mssql-tools18/bin/sqlcmd}"

# Launch MSSQL in background
/opt/mssql/bin/sqlservr & 
pid=$!

# Wait for it to be available
echo "Waiting for MS SQL to be available"
count=0
is_up=1
while [ "$is_up" -ne 0 ] && [ "$count" -lt 30 ] ; do
    "$SQLCMD" -l 30 -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" >/dev/null 2>&1;
    is_up=$?
    if [ "$is_up" -ne 0 ]; then
        echo "SQL Server not ready."
        ((count++))
        sleep 5
    fi
done

if [ "$is_up" -ne 0 ] && [ "$count" -eq 30 ] ; then
    echo "SQL Server did not become ready."
    wait $pid
    exit 1
fi

echo "SQL Server is ready."

LOG_FILE=output.log

if [ ! -f "${SCRIPTS}/${LOG_FILE}" ]; then
    for script in "${SCRIPTS}/"*.sh; do
        echo "Executing: ${script}" >> "${SCRIPTS}/${LOG_FILE}"
        if [ -x "script" ]; then
            "${script}"
        else 
            echo "Ignoring : ${script}" >> "${SCRIPTS}/${LOG_FILE}"
        fi
    done

# trap SIGTERM and send same to sqlservr process for clean shutdown
trap "kill -15 $pid" SIGTERM
# Wait on the sqlserver process
wait $pid
