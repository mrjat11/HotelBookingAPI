# Hotel Management & Booking System API

A comprehensive **Hotel Management & Booking System API** built using **.NET 8** and **SQL Server**.  
This project provides RESTful APIs for managing hotels, rooms, reservations, users, payments, and more.  

---

## 🚀 Tech Stack
- **.NET 8** (ASP.NET Core Web API)  
- **SQL Server** (Database with tables, stored procedures, and user-defined table types)  
- **Microsoft.Data.SqlClient** (Database connection)  
- **Serilog** (Structured logging with rolling file logs)  
- **Swagger / Swashbuckle** (API documentation and testing)  

---

## 📂 Project Structure
HotelBookingAPI/
│-- Controllers/ # API Controllers
│-- Models/ # Entity Models
│-- Services/ # Business Logic
│-- DatabaseScripts/ # SQL Scripts (Tables, SPs, UDTs)
│-- Program.cs # Application entry point
│-- appsettings.json # Configuration (ConnectionStrings, Logging)

---

## 🗄️ Database
The project includes a `DatabaseScripts` folder with:  
- **Table creation scripts**  
- **Stored procedures**  
- **User-defined table types (UDTTs)**  

This allows anyone to set up the required SQL Server database schema before running the API.  

---

## 📋 Features / Modules

### User Management Module
- **Role-Based Access Control**: Differentiate access levels (Admin, Manager, Guest).  
- **Authentication & Authorization**: Secure login with role-based restrictions.  
- **User Profile Management**: Update personal and contact details.  

### Search & Filter Module
- **Search**: Find hotels by location, price, rating, amenities, etc.  
- **Filters**: Refine search results with multiple criteria.  

### Hotel Management Module (Admin / Hotel Owners)
- Add, update, or remove hotel listings.  
- Manage room types, availability, pricing, and facilities.  

### Amenities Management Module
- Define and manage hotel amenities (spa, gym, pool, etc.).  
- Map amenities to hotels.  

### Room Management Module
- Room setup with categories, features, and availability.  
- Real-time availability checks for reservations.  

### Reservation System Module
- Booking engine for room reservations.  
- Modify bookings (cancellation, updates).  
- Support for group bookings.  

### Guest Management Module
- Guest registration with personal preferences.  
- Manage guest profiles and history.  

### Payment Module
- Secure payment processing & invoicing.  
- Refund management with multiple payment method support.  

---

## ⚙️ Setup & Run Locally

### 1. Clone the Repository
```bash
git clone https://github.com/mrjat11/HotelBookingAPI.git
cd HotelBookingAPI

2. Set up Database

Open SQL Server Management Studio (SSMS).

Run scripts from the DatabaseScripts folder to create tables, stored procedures, and UDTs.

3. Configure Connection String

Update appsettings.json with your SQL Server connection string:

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=HotelBookingDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}

4. Run the Application
dotnet run

The API will be available at:
👉 https://localhost:5001/swagger (Swagger UI for testing endpoints)

📝 Logging

Logging is configured using Serilog.

Logs are stored in Logs/MyAppLog-.txt (rolling daily logs).

📖 API Documentation

Swagger is integrated for testing and exploring endpoints:
👉 Navigate to /swagger after running the project.

👨‍💻 Author

Abhishek Jat

GitHub: https://github.com/mrjat11

LinkedIn: https://www.linkedin.com/in/abhishek-jat-409250208/