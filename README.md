# 🖥 Personal Diary Web App
This is web application built with ASP.NET Core and Blazor that allows users to log daily experiences, categorize them and manage entries with timestamps.
### Features:
* User authentication and registration
* Categorized diary entries to where you want
* Automatic timestamping
* Input validation
* Serilog-based logging
* Unit tests

## 🧗 Getting Started
### Prerequisites:
* .NET 9
* SQL Server
* Visual Studio / VS Code
###Installation
1. Clone the repository:
```
git clone https://github.com/oilPilot/WebDiary.git
cd your-repo-name
```
2. Set up the database connection string in appsettings.json.
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
