# WebDiary - Docker Deployment Guide

Complete guide for containerizing and deploying WebDiary using Docker and Docker Compose.

## Table of Contents

- [Docker Overview](#docker-overview)
- [Quick Start](#quick-start)
- [Building Images](#building-images)
- [Running Containers](#running-containers)
- [Docker Compose](#docker-compose)
- [Environment Configuration](#environment-configuration)
- [Database Management](#database-management)
- [Networking](#networking)
- [Troubleshooting](#troubleshooting)
- [Production Deployment](#production-deployment)
- [Health Checks](#health-checks)

## Docker Overview

WebDiary consists of three containerized services:

| Service | Image | Port | Purpose |
|---------|-------|------|---------|
| **backend** | webdiary:latest | 5281 | ASP.NET Core API |
| **frontend** | webdiary-frontend:latest | 5282 | Blazor Server UI |
| **db** | postgres:16 | 5432 | PostgreSQL Database |

### Architecture Benefits

- **Containerization**: Each service runs in isolated environment
- **Scalability**: Easy horizontal scaling with orchestration tools
- **Consistency**: Same environment across development and production
- **Deployment**: One-command deployment with Docker Compose

## Quick Start

### Prerequisites

- Docker 20.10+
- Docker Compose 2.0+
- 4GB RAM minimum
- 2GB free disk space

### 1. Quick Deploy (90 seconds)

```bash
# Clone repository
git clone https://github.com/yourusername/WebDiary.git
cd WebDiary

# Copy environment template
cp .env.example .env

# Start all services
docker-compose up -d

# Verify services
docker-compose ps
```

Services will be available at:
- **Frontend**: http://localhost:5282
- **Backend API**: http://localhost:5281
- **Database**: localhost:5432

### 2. Initial Setup

```bash
# Check logs
docker-compose logs -f

# Wait for database to initialize (30-40 seconds)
# Then access the application at http://localhost:5282
```

### 3. Stop Services

```bash
# Stop all services
docker-compose down

# Stop and remove volumes (⚠️ deletes database data)
docker-compose down -v
```

## Building Images

### Backend Build

The backend Dockerfile uses multi-stage building:

```bash
# Build backend image
docker build -f WebDiary/Dockerfile -t webdiary:latest .

# Build with custom tag
docker build -f WebDiary/Dockerfile -t webdiary:dev -t webdiary:latest .

# Build for specific platform
docker build \
  --platform linux/amd64 \
  -f WebDiary/Dockerfile \
  -t webdiary:latest .
```

**Build Stages**:
1. **SDK Stage**: Uses `mcr.microsoft.com/dotnet/sdk:9.0` for compilation
2. **Runtime Stage**: Uses `mcr.microsoft.com/dotnet/aspnet:9.0` for execution
3. **Size Reduction**: ~150MB runtime image (vs ~1GB with SDK)

### Frontend Build

```bash
# Build frontend image
docker build -f WebDiary.Frontend/Dockerfile -t webdiary-frontend:latest .

# Build with specific tag
docker build -f WebDiary.Frontend/Dockerfile -t webdiary-frontend:dev .
```

### Build with Compose

Docker Compose automatically builds images if they don't exist:

```bash
# Build all images
docker-compose build

# Build specific service
docker-compose build backend

# Rebuild without cache
docker-compose build --no-cache

# Force rebuild
docker-compose up -d --build
```

## Running Containers

### Backend Container

```bash
# Run backend alone (requires PostgreSQL running)
docker run \
  --name webdiary-api \
  -p 5281:5281 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e "ConnectionStrings__DiariesConnection=Host=db;Database=webdiary;Username=webdiary;Password=webdiary_password" \
  -e "Jwt__Key=YourSecretKeyHere" \
  webdiary:latest

# Access API
curl http://localhost:5281/health
```

### Frontend Container

```bash
# Run frontend alone (requires backend running)
docker run \
  --name webdiary-frontend \
  -p 5282:5282 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e "ApiConnection=http://backend:5281" \
  webdiary-frontend:latest

# Access frontend
curl http://localhost:5282
```

### Database Container

```bash
# Run PostgreSQL
docker run \
  --name webdiary-db \
  -p 5432:5432 \
  -e POSTGRES_USER=webdiary \
  -e POSTGRES_PASSWORD=webdiary_password \
  -e POSTGRES_DB=webdiary \
  -v postgres_data:/var/lib/postgresql/data \
  postgres:16-alpine

# Connect with psql
docker exec -it webdiary-db psql -U webdiary -d webdiary
```

## Docker Compose

### Starting Services

```bash
# Start in background
docker-compose up -d

# Start with output
docker-compose up

# Start specific services
docker-compose up -d backend frontend
```

### Service Management

```bash
# View service status
docker-compose ps

# View logs
docker-compose logs                    # All services
docker-compose logs -f backend         # Follow backend logs
docker-compose logs backend -n 50      # Last 50 lines

# Execute commands in container
docker-compose exec backend dotnet --version
docker-compose exec db psql -U webdiary -d webdiary

# Restart service
docker-compose restart backend

# Stop services
docker-compose stop                    # Stop all
docker-compose stop backend            # Stop specific

# Remove containers
docker-compose down                    # Remove stopped containers
docker-compose down -v                 # Remove with volumes
docker-compose down --rmi all          # Remove with images
```

### Scaling Services

```bash
# Scale backend to 3 instances (requires load balancer)
docker-compose up -d --scale backend=3

# Note: Only works for stateless services without port binding
```

## Environment Configuration

### .env File

Copy `.env.example` to `.env` and configure:

```bash
# Database Configuration
DIARIES_CONNECTION_STRING=Host=db;Database=webdiary;Username=webdiary;Password=webdiary_password
DB_PASSWORD=webdiary_password

# JWT Configuration
JWT_TOKEN_KEY=YourSuperSecretKeyChangeThis
JWT_ISSUER=DiaryIssuer
JWT_AUDIENCE=PeopleFromIntelligentWorld

# Email Configuration
MAILGUN_API_KEY=your-mailgun-api-key
EMAIL_FROM=noreply@webdiary.local

# Environment
ASPNETCORE_ENVIRONMENT=Development

# URLs
API_CONNECTION=http://backend:5281
FRONTEND_URL=http://localhost:5282
```

### Environment Variables in Docker

Variables can be passed multiple ways:

**1. Using .env file**
```bash
docker-compose config  # Verify variable expansion
```

**2. Using --env-file**
```bash
docker run --env-file .env webdiary:latest
```

**3. Using -e flag**
```bash
docker run -e JWT_TOKEN_KEY=mysecret webdiary:latest
```

**4. Using environment section in compose**
```yaml
services:
  backend:
    environment:
      JWT_TOKEN_KEY: ${JWT_TOKEN_KEY}
```

### Secrets Management

For production, use Docker Secrets instead of environment variables:

```yaml
services:
  backend:
    secrets:
      - jwt_key
      - db_password

secrets:
  jwt_key:
    external: true
  db_password:
    external: true
```

Create secrets:
```bash
echo "YourSecretKey" | docker secret create jwt_key -
echo "password" | docker secret create db_password -
```

## Database Management

### Initial Setup

Database initializes automatically on first start:

```bash
# Wait for database to be ready
docker-compose logs db | grep "database system is ready to accept connections"
```

### Database Migrations

Apply EF Core migrations in container:

```bash
# Using docker-compose
docker-compose exec backend dotnet ef database update

# Using docker directly
docker exec webdiary-api dotnet ef database update

# View pending migrations
docker-compose exec backend dotnet ef migrations list
```

### Database Backup

```bash
# Backup database
docker-compose exec db pg_dump -U webdiary webdiary > backup.sql

# Backup with compression
docker-compose exec db pg_dump -U webdiary -F c webdiary > backup.dump

# Backup volume
docker run --rm \
  -v webdiary_postgres_data:/data \
  -v $(pwd):/backup \
  ubuntu tar czf /backup/db-backup.tar.gz -C /data .
```

### Database Restore

```bash
# From SQL dump
docker-compose exec -T db psql -U webdiary < backup.sql

# From compressed dump
docker-compose exec -T db pg_restore -U webdiary -d webdiary backup.dump

# From volume backup
docker run --rm \
  -v webdiary_postgres_data:/data \
  -v $(pwd):/backup \
  ubuntu tar xzf /backup/db-backup.tar.gz -C /data
```

### Database Access

```bash
# Connect to database
docker-compose exec db psql -U webdiary -d webdiary

# Common commands
\dt           # List tables
\di           # List indexes
SELECT * FROM pg_stat_statements;  # View queries
```

## Networking

### Network Configuration

Services communicate via `webdiary_network` bridge:

```bash
# View network
docker network inspect webdiary_network

# Services can reach each other by name:
# - backend:5281 (from frontend)
# - db:5432 (from backend and frontend)
# - frontend:5282 (from load balancer)
```

### External Access

```bash
# Access from host machine
curl http://localhost:5281    # Backend
curl http://localhost:5282    # Frontend
psql -h localhost -U webdiary -d webdiary   # Database
```

### Custom Network

Create custom network for multi-compose setup:

```bash
docker network create custom-webdiary

docker-compose --network custom-webdiary up -d
```

## Troubleshooting

### Common Issues

#### 1. Database Connection Failed

```bash
# Check database logs
docker-compose logs db

# Verify database is ready
docker-compose exec db pg_isready -U webdiary

# Test connection
docker-compose exec backend dotnet run

# Solution
docker-compose down -v && docker-compose up -d
```

#### 2. Port Already in Use

```bash
# Find process using port
lsof -i :5281
netstat -ano | findstr :5281  # Windows

# Change port in docker-compose.yml
ports:
  - "5381:5281"  # Map to different port

# Or kill process
kill -9 <PID>
fuser -k 5281/tcp
```

#### 3. Out of Memory

```bash
# Check resource limits
docker stats

# Set memory limit in compose
services:
  backend:
    deploy:
      resources:
        limits:
          memory: 512M
        reservations:
          memory: 256M
```

#### 4. Image Build Fails

```bash
# Clear build cache
docker-compose build --no-cache

# Check build logs
docker-compose build --verbose

# Inspect layers
docker history webdiary:latest
```

#### 5. Frontend Can't Reach Backend

```bash
# Check service connectivity
docker-compose exec frontend curl http://backend:5281

# Verify network
docker network inspect webdiary_network

# Check environment variables
docker-compose exec frontend env | grep API
```

### Debugging

```bash
# View all container processes
docker-compose ps -a

# Execute shell in container
docker-compose exec backend /bin/sh
docker-compose exec db /bin/sh

# Monitor logs in real-time
docker-compose logs -f --tail=100

# Inspect container configuration
docker inspect webdiary-api

# View network isolation
docker exec webdiary-api ping webdiary-db
docker exec webdiary-api nslookup db
```

## Production Deployment

### Pre-Production Checklist

- [ ] Set strong JWT secret key
- [ ] Configure database backup strategy
- [ ] Set up log aggregation (ELK, DataDog, etc.)
- [ ] Configure HTTPS/TLS
- [ ] Set resource limits
- [ ] Enable health checks
- [ ] Set up monitoring and alerts
- [ ] Configure restart policies
- [ ] Plan disaster recovery

### Production Environment

```yaml
# docker-compose.prod.yml
version: '3.8'

services:
  db:
    image: postgres:16-alpine
    restart: always
    deploy:
      resources:
        limits:
          memory: 2G
    environment:
      POSTGRES_PASSWORD_FILE: /run/secrets/db_password
    secrets:
      - db_password

  backend:
    image: webdiary:latest
    restart: always
    deploy:
      resources:
        limits:
          memory: 1G
      replicas: 2
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      Jwt__Key_FILE: /run/secrets/jwt_key
    secrets:
      - jwt_key

  frontend:
    image: webdiary-frontend:latest
    restart: always

secrets:
  jwt_key:
    external: true
  db_password:
    external: true
```

Deploy:
```bash
docker-compose -f docker-compose.prod.yml up -d
```

### HTTPS Configuration

Use reverse proxy (Nginx, Traefik, Caddy):

```yaml
services:
  traefik:
    image: traefik:latest
    ports:
      - "443:443"
      - "80:80"
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
      - ./traefik.yml:/traefik.yml
      - ./letsencrypt:/letsencrypt
```

### Kubernetes Deployment

Convert to Kubernetes manifests:

```bash
# Option 1: Using Kompose
kompose convert -f docker-compose.yml -o kubernetes/

# Option 2: Manual creation
kubectl apply -f kubernetes/
```

## Health Checks

### Built-in Health Checks

Each service includes health checks:

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:5281/health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 40s
```

Check status:
```bash
docker-compose ps
# Shows: (healthy) or (unhealthy)

# View health details
docker inspect --format='{{json .State.Health}}' webdiary-api | jq
```

### Custom Health Endpoints

```bash
# Backend health
curl http://localhost:5281/health

# Frontend health (root page)
curl http://localhost:5282/

# Database health
docker-compose exec db pg_isready -U webdiary
```

## Best Practices

**Security**
- Use secrets for sensitive data
- Run containers as non-root user (already implemented)
- Keep base images updated
- Scan images for vulnerabilities

**Performance**
- Use multi-stage builds (already implemented)
- Enable build cache
- Optimize layer caching
- Use .dockerignore

**Reliability**
- Use restart policies
- Implement health checks
- Set resource limits
- Log aggregation

**Maintenance**
- Tag images with versions
- Document configuration
- Test before production
- Automate backups

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [PostgreSQL Docker](https://hub.docker.com/_/postgres)

---

**Version**: 1.0.0  
**Last Updated**: March 2026
