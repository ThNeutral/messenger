#!/bin/bash

# A simple Docker Compose helper script for quick operations.
# Usage: ./docker-compose-helper.sh [command]
# Commands:
#   up            - Create and start containers in the background
#   stop          - Stop running containers
#   down          - Stop and remove containers, networks
#   down-v        - Stop and remove containers, networks, and volumes
#   build         - Build or rebuild services
#   build-nocache - Build or rebuild services without using cache

# Check if the script is run as root or with sudo
if [ "$EUID" -ne 0 ]; then
    echo "Please run as root or with sudo."
    exit 1
fi

COMPOSE_FILE="compose.yaml"

if [ ! -f "$COMPOSE_FILE" ]; then
    echo "Error: $COMPOSE_FILE not found in the current directory."
    exit 1
fi

case "$1" in
    up)
        echo "Starting containers..."
        docker compose up -d
        ;;

    up-fg)
        echo "Starting containers in foreground mode..."
        docker compose up
        ;;

    stop)
        echo "Stopping containers..."
        docker compose stop
        ;;

    down)
        echo "Stopping and removing containers, networks..."
        docker compose down
        ;;

    down-v)
        echo "Stopping and removing containers, networks, and volumes..."
        docker compose down --volumes
        ;;

    build)
        echo "Building services..."
        docker compose build
        ;;

    build-nocache)
        echo "Building services without cache..."
        docker compose build --no-cache
        ;;

    *)
        echo "Usage: $0 {up|stop|down|down-v|build|build-nocache}"
        exit 1
        ;;
esac