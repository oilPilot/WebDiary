# WebDiary - Full-Stack Diary Management Application

![WebDiary Banner](./screenshots/main.png)

A modern, feature-rich diary application built with **ASP.NET Core** and **Blazor Server**, designed for personal journaling, thematic diary organization, mood tracking, and secure data management.

## Overview

WebDiary is a comprehensive digital diary platform for personal journaling and mood tracking. Organize your diary entries by theme (sports, ideas, daily thoughts, etc.), track your mood patterns, access advanced analytics, and maintain a secure personal journal with powerful features.

### Implemented Features

**Authentication & Security**
- Secure JWT-based authentication with refresh tokens
- Password hashing using identity framework hashing
- Master password support for enhanced security
- PIN codes for secure group access

**Diary Management**
- Create and manage unlimited personal diary entries
- Rich text editing with HTML content support
- Organize entries by thematic groups (sports, ideas, etc.)
- Entries with timestamps and comprehensive metadata
- Full CRUD operations with permission-based access control

**Mood & Analytics**
- Mood tracking with visual mood entries
- Advanced statistics dashboard
- Mood trends visualization with interactive charts
- Personal activity metrics and frequency analysis
- Mood distribution analysis with custom date range filtering

**Data Export**
- Export diary entries as formatted PDF
- HTML sanitization for safe content display
- Export statistics and analytics reports

**Localization**
- Full support for English and German languages
- Localized error messages and resources
- Multi-language UI components

**Frontend Experience**
- Interactive Blazor Server-based application
- Responsive Bootstrap-based design
- Real-time updates with SignalR
- Local and session storage for client-side state
- Date range picker for flexible filtering
- Rich text editor for diary entries

**Admin Features**
- Admin panel for user and diary group management
- System statistics and usage analytics
- User activity monitoring

### Planned Features

**Enhanced Collaboration**
- Shared diary groups for collaboration with friends
- Role-based access control for group members
- Real-time chat and discussions within groups
- Group activity tracking and audit logs

**Intelligence & Automation**
- Automatic mood suggestion using AI emoji analysis
- Custom mood scoring system
- Statistics caching for performance optimization

**Email & Notifications**
- Email notifications for account events
- Email-based account recovery (in development)
- Login notifications

**Advanced Features**
- Mobile application (React Native)
- Advanced search with Elasticsearch
- Data encryption at rest
- Multi-factor authentication (MFA)
- Two-factor authentication (2FA)
- API versioning
- GraphQL API option
- Dark mode support

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 9.0
- **Language**: C#
- **Database**: PostgreSQL 16
- **ORM**: Entity Framework Core 9.0
- **Authentication**: JWT Bearer tokens
- **API Communication**: RESTful APIs + SignalR WebSockets
- **Logging**: Serilog with PostgreSQL sink
- **PDF Export**: PDFsharp
- **HTML Processing**: HtmlAgilityPack

### Frontend
- **Framework**: Blazor Server (Interactive Server Components)
- **.NET Target**: net9.0
- **UI Framework**: Bootstrap Blazor 9.4.2
- **Charts**: Plotly.Blazor for interactive data visualization
- **Rich Text Editor**: Blazored Text Editor
- **Storage**: Blazored Local Storage + Session Storage
- **Date Picker**: BlazorDateRangePicker
- **HTTP Client**: RestSharp
- **Authentication**: JWT with custom AuthenticationStateProvider
- **Logging**: Serilog

### Database
- **PostgreSQL 16** for data persistence
- **EF Core Migrations** for schema management
- **Connection Pooling** for performance optimization

### DevOps & Deployment
- **Containerization**: Docker with multi-stage builds
- **Orchestration**: Docker Compose
- **Development Environment**: VS Code / Visual Studio 2022
- **CI/CD Ready**: Current deployment on Render

## Project Structure

```
WebDiary/
├── WebDiary/                    # Backend API
│   ├── Controller/              # API controllers (Auth, Stats, Export, etc.)
│   ├── Endpoints/               # Minimal APIs for diary, groups, logs, users
│   ├── Services/                # Business logic (Auth, Export, Stats, Email)
│   ├── Data/                    # EF Core DbContext and extensions
│   ├── Entities/                # Domain models
│   ├── DTO/                     # Data transfer objects
│   ├── Mapping/                 # Entity to DTO mapping
│   ├── Helpers/                 # Utilities (PDF export, HTML sanitization, mood handling)
│   ├── Hubs/                    # SignalR hub for real-time chat
│   ├── Resources/               # Localized strings
│   ├── Model/                   # API request models
│   ├── Properties/              # Launch settings
│   ├── Dockerfile               # Backend container image
│   ├── Program.cs               # Application startup configuration
│   ├── appsettings.json         # Configuration
│   └── WebDiary.csproj          # Backend project file
│
├── WebDiary.Frontend/           # Blazor Server Frontend
│   ├── Components/              # Razor components and pages
│   ├── Clients/                 # HTTP clients for API communication
│   ├── Models/                  # Frontend view models
│   ├── Services/                # Frontend services
│   ├── Properties/              # Launch settings
│   ├── Dockerfile               # Frontend container image
│   ├── Program.cs               # Frontend startup configuration
│   ├── appsettings.json         # Configuration
│   └── WebDiary.Frontend.csproj # Frontend project file
│
├── WebDiary.Tests/              # Unit tests
│
├── docker-compose.yml           # Multi-service container orchestration
├── .env.example                 # Environment variables template
├── Dockerfile                   # (see service-specific Dockerfiles)
├── ROADMAP.md                   # Feature roadmap
└── README.md                    # This file
```

## Installation & Setup

### Prerequisites

- **.NET SDK 9.0** or later
- **PostgreSQL 16** or later
- **Docker & Docker Compose** (for containerized deployment)
- **Visual Studio 2022** or **VS Code** with C# extension

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/WebDiary.git
   cd WebDiary
   ```

2. **Configure environment variables**
   ```bash
   cp .env.example .env
   # Edit .env with your configuration:
   # - DIARIES_CONNECTION_STRING
   # - JWT_TOKEN_KEY (use a strong secret)
   # - MAILGUN_API_KEY (optional, for email features)
   ```

3. **Set up PostgreSQL database**
   ```bash
   # Ensure PostgreSQL is running and accessible
   # Update connection string in .env if needed
   ```

4. **Apply database migrations**
   ```bash
   cd WebDiary
   dotnet ef database update
   cd ..
   ```

5. **Run the backend**
   ```bash
   cd WebDiary
   dotnet run
   # Backend runs at http://localhost:5281
   ```

6. **Run the frontend** (in another terminal)
   ```bash
   cd WebDiary.Frontend
   dotnet run
   # Frontend runs at http://localhost:5282
   ```

### Docker Deployment

See [README.Docker.md](./README.Docker.md) for complete Docker setup instructions.

Quick start with Docker Compose:
```bash
# Build and run all services
docker-compose up -d

# Frontend: http://localhost:5282
# Backend API: http://localhost:5281
# Database: localhost:5432
```

## API Endpoints

### Authentication
- `POST /auth/jwttoken/login` - Login and receive JWT token
- `POST /auth/refresh` - Refresh JWT token
- `POST /auth/reset-password` - Reset password
- `POST /auth/verify-pin` - Verify group PIN

### Diary Management
- `GET /diaries` - Get user's diaries
- `POST /diaries` - Create new diary entry
- `GET /diaries/{id}` - Get specific diary
- `PUT /diaries/{id}` - Update diary
- `DELETE /diaries/{id}` - Delete diary

### Groups
- `GET /groups` - Get user's groups
- `POST /groups` - Create new group
- `PUT /groups/{id}` - Update group
- `DELETE /groups/{id}` - Delete group
- `POST /groups/{id}/members` - Add member to group

### Statistics
- `GET /stats/dashboard` - Get dashboard statistics
- `GET /stats/mood-trends` - Get mood trend data
- `GET /stats/activity` - Get activity metrics

### Export
- `POST /export/pdf` - Export diary as PDF

### Chat (SignalR)
- WebSocket connection: `/hubs/chat`

## Configuration

### Environment Variables

See `.env.example` for all available configuration options:

| Variable | Description | Default |
|----------|-------------|---------|
| `DIARIES_CONNECTION_STRING` | PostgreSQL connection string | |
| `JWT_TOKEN_KEY` | Secret key for JWT signing | |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | Development |
| `API_CONNECTION` | Backend API URL | http://localhost:5281 |

### Application Settings

Modify `appsettings.json` for additional configuration:
- Logging levels
- JWT issuer and audience
- Frontend URL
- CORS settings
- Database options

## Development

### Running Tests

```bash
dotnet test WebDiary.Tests/WebDiary.Tests.csproj
```

### Building for Release

#### Backend
```bash
cd WebDiary
dotnet publish -c Release -o ./publish
```

#### Frontend
```bash
cd WebDiary.Frontend
dotnet publish -c Release -o ./publish
```

### Code Organization & Best Practices

- **Separation of Concerns**: Controllers delegate to services, services contain business logic
- **DTOs**: Used for API communication to decouple domain models
- **Dependency Injection**: Extensive use via ASP.NET Core's DI container
- **Entity Framework**: Migrations for schema management
- **Logging**: Structured logging with Serilog throughout application
- **Localization**: Resource files for multi-language support
- **Security**: Password hashing, JWT validation, authorization policies

## Security Features

**Implemented**
- JWT-based authentication with expiration and refresh tokens
- Password hashing using ASP.NET Core Identity hashing
- Master password support for sensitive data access
- HTTPS enforcement in production
- CORS configuration
- Secure cookie handling
- User secrets management

**Planned**
- Multi-factor authentication (MFA)
- Two-factor authentication (2FA)
- Rate limiting
- Request signing

## Performance Optimizations

- **Database Connection Pooling**: DbContextPool for efficient connection management
- **Statistics Caching**: In-memory caching for frequently accessed stats
- **Entity Framework Optimization**: Select-only queries, lazy loading considerations
- **Serilog Buffering**: Efficient structured logging

## Development Status

For detailed feature roadmap, progress updates, and known issues, see [ROADMAP.md](./ROADMAP.md).

## Deployment

### Production Deployment

The application is currently deployed on **Render** cloud platform:
- **Backend**: https://webdiary-api.onrender.com
- **Frontend**: https://webdiary-frontend.onrender.com

### Deployment Guide

1. **Using Docker** (Recommended)
   - See [README.Docker.md](./README.Docker.md)

2. **Manual Deployment**
   - Publish projects: `dotnet publish -c Release`
   - Upload binaries to hosting platform
   - Configure environment variables
   - Run application

3. **CI/CD Pipeline**
   - GitHub Actions configuration available
   - Automatic builds on push
   - Docker image builds and pushes

## Contributing

Contributions are welcome! Please follow these guidelines:

1. Create a feature branch (`git checkout -b feature/amazing-feature`)
2. Commit changes (`git commit -m 'Add amazing feature'`)
3. Push to branch (`git push origin feature/amazing-feature`)
4. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Portfolio Highlights

This project demonstrates:

- Full-stack development with modern .NET technologies
- Interactive UI using Blazor Server with responsive design
- Production-grade security with JWT and password management
- Complex business logic including statistics, PDF generation, and integrations
- Database design with proper relationships and migrations
- DevOps practices with Docker containerization
- Internationalization supporting multiple languages
- Real-time features using SignalR
- Testing practices with unit test coverage
- Code quality with proper architecture and patterns

## Contact & Support

- **Issues**: GitHub Issues
- **Email**: matveyeresko1@gmail.com
- **Portfolio**: [\[Your Portfolio Link\]](https://oilpilot.github.io/Portfolio/)

---

**Version**: 1.0.0  
**Last Updated**: March 2026  
**Status**: Active Development
