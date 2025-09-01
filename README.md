# Hotel Booking API

A robust **ASP.NET Core 8.0 Web API** project for managing hotel reservations, room availability, users, and payments.  
This API is designed with modular architecture, clean code practices, and logging using **Serilog**.  

---

## Features

- **User Management** with authentication and role-based authorization  
- **Hotel Search & Filtering** by location, price, amenities, and more  
- **Reservation System** with support for bookings, cancellations, and modifications  
- **Payment Processing** with invoice and refund management  
- **Logging** via Serilog with file-based rolling logs  
- **Swagger/OpenAPI** integration for easy API documentation and testing  

---

## Modules Overview

### 1. User Management Module
- **Role-Based Access Control**: Differentiate access levels (Administrator, Manager, Guest).  
- **Authentication and Authorization**: Secure login, ensuring users only access authorized features.  
- **User Profile Management**: Update personal and contact information.  

### 2. Search and Filter Module
- **Search**: Find hotels by location, price, star rating, amenities, etc.  
- **Filter**: Refine search results with advanced filters.  

### 3. Hotel Management Module (Admin/Owners)
- Add, update, or remove hotel listings.  
- Manage room types, availability, prices, and facilities.  

### 4. Amenities Management Module
- **Amenities Setup and Mapping**: Define/manage amenities like spa, gym, pool, etc.  

### 5. Room Management Module
- **Room Setup**: Categorize rooms with features and availability.  
- **Room Availability Checking**: Real-time availability for reservations.  

### 6. Reservation System Module
- **Booking Engine**: Book rooms with dates and preferences.  
- **Reservation Modifications**: Handle cancellations/alterations.  
- **Group Bookings**: Book multiple rooms under one reservation.  

### 7. Guest Management Module
- **Guest Registration**: Manage guest details and preferences.  

### 8. Payment Module
- **Payment and Invoicing**: Process payments and generate invoices.  
- **Refund Management**: Handle refunds across different methods.  

---

## Tech Stack

- **Framework**: ASP.NET Core 8.0  
- **Database**: Microsoft SQL Server (via `Microsoft.Data.SqlClient`)  
- **Logging**: Serilog (File Sink, Config-based)  
- **API Docs**: Swagger / OpenAPI  

---

## Setup Instructions

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/HotelBookingAPI.git
   cd HotelBookingAPI


Update your connection string in appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "YourConnectionStringHere" }

Run the application:
dotnet run


Access Swagger UI at:
https://localhost:5001/swagger


Logging Configuration

Logging is handled via Serilog, with daily rolling logs.
Default path: Logs/MyAppLog-.txt

Example configuration in appsettings.json:

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


Author

Abhishek Jat
LinkedIn: https://www.linkedin.com/in/abhishek-jat-409250208/