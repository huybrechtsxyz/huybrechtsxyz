#!/bin/bash

set -e

host="$1"
shift
cmd="$@"

# Wait until the SQLite database is ready
until sqlite3 /app/data/xyz-application.db -line "SELECT 1;" >/dev/null 2>&1; do
  >&2 echo "SQLite is unavailable - sleeping"
  sleep 1
done

>&2 echo "SQLite is up - executing command"
exec $cmd
