# Jogging Tracker API (Demo Project)
The Jogging Tracker API is designed to track the jogging times of users, providing features for user management, record tracking, and reporting. It follows RESTful principles and requires authentication for all API calls.

## User Management
-Users can create an account and log in.

-Authentication is required for all API calls.

## Role-based Access Control.
   The API implements three roles with different permission levels:
   
       Regular User:
         Can CRUD (Create, Read, Update, Delete) only their jogging records.
       User Manager:
          Can CRUD only users.
       Admin:
          Can CRUD all jogging records and users

## Jogging Records
-Each time entry includes a date, distance, time, and location.

## Reporting
-The API generates a report on the average speed and distance per week.

## Filtering and Pagination
-Endpoints supporting lists of elements provide filter capabilities.
-Pagination is supported for a better user experience.

## API Actions
-Users and admins can perform all necessary actions via the API, including authentication.

---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
# Getting Started

### Prerequisites

Before you begin, make sure you have the following installed on your machine:

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads/)

### Installation

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/mohamadAlhajja/JoggingTracker.git

2. **Open the Solution with Visual Studio**
3. **Build the project with dotnet build command**
4. **On Package Manager Console Create database with:**
   ```bash
   dotnet ef database update

### Running the Project and test

1. **With visual studio running is so easy you can run it by run Icon**
2. **Running tests can be also easily done with Test Explorer**





