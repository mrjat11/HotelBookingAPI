# 🏨 Hotel Booking API

A RESTful **ASP.NET Core 8 Web API** for managing hotels, rooms, and reservations.  
This project demonstrates clean layered architecture with **Controllers, DTOs, Repository Pattern, Custom Validators**, and **structured logging with Serilog**.  
Database connectivity is implemented using **Microsoft.Data.SqlClient** with SQL Server.  

---

## 🚀 Features
- 🔹 Hotel Management (CRUD operations for hotels)
- 🔹 Room and Room Type Management
- 🔹 Booking, Reservation, and Cancellation APIs
- 🔹 User management
- 🔹 SQL Server integration using **Microsoft.Data.SqlClient**
- 🔹 Repository Pattern for clean data access
- 🔹 DTOs for request/response handling
- 🔹 Attribute-based validation (dates, price ranges, etc.)
- 🔹 **Logging with Serilog** (daily rolling file logs)
- 🔹 API Documentation with Swagger/OpenAPI

---

## 🛠 Tech Stack
- **Backend:** ASP.NET Core 8 Web API  
- **Database Access:** Microsoft.Data.SqlClient (ADO.NET)  
- **Architecture:** Repository Pattern + DTOs + Attribute-based Validation  
- **Database:** SQL Server  
- **Logging:** Serilog (rolling file logs)  
- **Documentation:** Swagger / OpenAPI (Swashbuckle.AspNetCore)  

---

## 📂 Project Structure
HotelBookingAPI/
│── Connection/ # Database connection factory (SqlConnectionFactory)
│── Controllers/ # API endpoints
│── CustomValidator/ # Attribute-based validation
│── DTOs/ # Request/Response objects
│── Extensions/ # Helper extensions
│── Models/ # Common models (e.g., APIResponse)
│── Repository/ # Data access layer (SqlClient + Repository Pattern)
│── Properties/
│── Program.cs # Entry point
│── appsettings.json # Config (with safe placeholders)
│── HotelBookingAPI.csproj


---

## ⚡ Getting Started

### 🔹 Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local or remote)

### 🔹 Clone the Repository
```bash
git clone https://github.com/mrjat11/HotelBookingAPI.git
cd HotelBookingAPI

🔹 Setup Database

Update the connection string in appsettings.json:
"ConnectionStrings": {
  "DefaultConnection": "YourConnectionStringHere"}

🔹 Run the API
dotnet run

The API will be available at:

https://localhost:5001/swagger



📖 Logging with Serilog
This project uses Serilog for structured logging with daily rolling log files.

Sample Configuration (appsettings.json):

"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Error",
      "System": "Error"
    }
  },
  "WriteTo": [
    {
      "Name": "File",
      "Args": {
        "path": "Logs/MyAppLog-.txt",
        "rollingInterval": "Day",
        "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
      }
    }
  ]
}

Usage Example (Controller):
_logger.LogInformation("Creating reservation for UserId {UserId} on {Date}", 
    reservation.UserId, reservation.ReservationDate);

try
{
    // business logic...
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error occurred while creating reservation for UserId {UserId}", reservation.UserId);
}

Logs are written to:
/Logs/MyAppLog-2025-09-01.txt


📝 Future Improvements

✅ Authentication & Authorization (JWT)

✅ Unit & Integration Testing

✅ CI/CD with GitHub Actions

✅ Docker containerization


## Author

**Abhishek Jat**

[![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/mrjat11)  
[![LinkedIn](https://img.shields.io/badge/LinkedIn-0A66C2?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/abhishek-jat-409250208/)


⭐ If you like this project, give it a star on GitHub!