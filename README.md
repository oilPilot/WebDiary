# 🖥 Personal Diary Web App
This is web application built with ASP.NET Core and Blazor that allows users to log daily experiences, categorize them and manage entries with timestamps.

### Live demo:
* You can see live demo hosted here: https://webdiary-frontend.onrender.com

### Screenshots:
![AdminPanelOfAppScreenshot](screenshots/AdminPanel.png)
![MainDiariesScreenshot](screenshots/DiariesPage.png)
![StatisticsPageScreenshot](screenshots/StatisticsPage.png)

## Features:
* User authentication and registration
* Categorized diary entries to named categories that you are creating
* Automatic timestamping
* Input validation
* Serilog-based logging
* Rich text formatting
* Export to PDF
* Light and Dark Mode
* quick emoji scale at each diary entry;
* Admin panel
* Statistics page
* Unit tests


## 🧗 Installation on local machine
### Prerequisites:
* .NET 9
* Postgres SQL
* Visual Studio / VS Code
### Installation
1. Clone the repository:
```
git clone https://github.com/oilPilot/WebDiary.git
cd WebDiary
dotnet run --project WebDiary
dotnet run --project WebDiary.Frontend
```
2. Before running the backend, configure the following secrets (using `dotnet user-secrets` or environment variables):
	- `Jwt:Key` – Secret key for Jwt signing.
	- `ConnectionStrings:DiariesConnection` – Database connection string.
3. Note: Database migrations will run automatically

## Architecture Overview
WebDiary is structured as a separated frontend and backend application.
1. The backend is an ASP.NET Core Web API. It is organized into:
* Controllers handle HTTP requests.
* Services contain business logic such.
* Data layer using EF Core and entities.
* DTOs defining the data contracts exposed.
2. The frontend is a Blazor application that communicates with the backend via HttpClient. It contains:
* Razor components responsible for UI and user interaction.
* API client classes that encapsulate HTTP communication and CRUD operations.
* Frontend models that match DTOs.
The request flow is: UI component => API client => backend controller => service => database => response flows back using DTO.

## 🛠️ Tech Stack
* ASP.NET Core
* Entity Framework Core
* Serilog
* Blazor Server
* Bootstrap UI
* Docker files (used for deployment)
* xUnit and Moq for unit tests
* Git for version control

## 📈 Planned Improvements
* Realize messaging between users
* Fix glitches
* You can look more in ROADMAP.md

## Where can be used
* Where this can be used:
* User portals with time-tracking
* Personal productivity apps
* CRM task logs
* Admin dashboards
* Company internal diaries/logs

## 🤝 Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
