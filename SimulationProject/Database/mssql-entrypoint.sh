#!/bin/bash

: "${ACCEPT_EULA:?ACCEPT_EULA must be set to Y}"
: "${MSSQL_SA_PASSWORD:?MSSQL_SA_PASSWORD is required}"
: "${SQLCMD:=/opt/mssql-tools18/bin/sqlcmd}"

# Launch MSSQL in background
/opt/mssql/bin/sqlservr & 
pid="$!"

# Wait for it to be available
echo "Waiting for MS SQL to be available"
count=0 
is_up=1
while [[ "$is_up" -ne 0 && "$count" -lt 40 ]]; do
    if  "$SQLCMD" -l 30 -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" >/dev/null 2>&1 ; then
        is_up=0
        break
    else
        is_up=$?
        echo "SQL Server not ready."
        ((count++))
        sleep 10
    fi
done

if [[ "$is_up" -ne 0 && "$count" -eq 40 ]]; then
    echo "SQL Server did not become ready."
    kill -15 "$pid"
    wait "$pid"
    exit 1
fi

echo "SQL Server is ready."

LOG_FILE=output.log

if [[ ! -f "${SCRIPTS}/${LOG_FILE}" ]]; then
    for script in "${SCRIPTS}/"*.sh; do
        echo "Executing: ${script}" >> "${SCRIPTS}/${LOG_FILE}" 2>&1
        if [[ -x "$script" ]]; then
            if ! "$script" >> "${SCRIPTS}/${LOG_FILE}" 2>&1; then
                echo "Script failed: ${script}" >> "${SCRIPTS}/${LOG_FILE}"
            fi
        else 
            echo "Ignoring : ${script}" >> "${SCRIPTS}/${LOG_FILE}"
        fi
    done
fi

# trap SIGTERM and send same to sqlservr process for clean shutdown
trap 'kill -15 "$pid"' SIGTERM
# Wait on the sqlserver process
wait "$pid"
