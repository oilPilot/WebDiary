# 🖥 Personal Diary Web App
This is web application built with ASP.NET Core and Blazor that allows users to log daily experiences, categorize them and manage entries with timestamps.
### Features:
* User authentication and registration
* Categorized diary entries to named categories that you are creating
* Automatic timestamping
* Input validation
* Serilog-based logging
* Unit tests

## Deployment url: https://webdiary-frontend.onrender.com

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
```
2. Before running the backend, configure the following secrets (using `dotnet user-secrets` or environment variables):
	- `Jwt:Key` – Secret key for Jwt signing.
	- `ConnectionStrings:DiariesConnection` – Database connection string.
	- `AppPaswordForEmailAuth` – gmail app password for email authentication (you should also probably change 'EmailFromSend' in appsettings.json).
3. Database migrations will run automatically
4. Run the backend at ../Webdiary:
`dotnet run`
5. And then run frontend with same command at ../Webdiary.Frontend
Also you can run tests with this command: `dotnet test`

## 🛠️ Tech Stack
* ASP.NET Core
* Entity Framework Core
* Blazor
* Serilog
* Bootstrap
* xUnit and Moq for unit test
* Git for version control

## 📈 Planned Improvements
* Export/backup functionality
* Dark mode

## 🤝 Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
