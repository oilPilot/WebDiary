# Docker Setup — WebDiary

This document describes how to run **WebDiary Backend + Frontend** using Docker.

## Requirements

- Docker 24+
- Docker Compose v2+

## Services

| Service  | Port | Description |
|----------|------|-------------|
| Backend  | 5281 | ASP.NET Core API |
| Frontend | 5101 | ASP.NET Core frontend |
| Postgres | 5432 | Local dev database (profile `dev`) |

## How to run

1. Create .env file with secrets, example you can see in .env.example

2. Run
docker compose up --build

### Access at
Frontend: http://localhost:5101
Backend: http://localhost:5281

## Troubleshooting
* DB connection fails → verify DIARIES_CONNECTION_STRING
* Frontend cannot reach backend → ensure backend container is healthy
* Migrations → must be applied manually or on startup (if enabled)
