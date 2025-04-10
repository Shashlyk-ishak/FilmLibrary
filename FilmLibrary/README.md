# Film Library

A web application for managing and rating movies, built with ASP.NET Core MVC.

## Features

- User registration and authentication
- Movie browsing and details viewing
- Movie rating system
- Admin panel for movie management
- Role-based authorization
- Responsive design

## Prerequisites

- Visual Studio 2022
- SQL Server (LocalDB or full instance)
- .NET 7.0 SDK

## Setup Instructions

1. Clone or download the repository
2. Open the solution in Visual Studio 2022
3. Open SQL Server Management Studio (SSMS)
4. Execute the `Database/CreateDatabase.sql` script to create the database and tables
5. Update the connection string in `appsettings.json` if needed
6. Build and run the project

## Database Setup

1. Open SQL Server Management Studio
2. Connect to your SQL Server instance
3. Open and execute the `Database/CreateDatabase.sql` script
4. The script will:
   - Create the FilmLibrary database
   - Create all necessary tables
   - Set up relationships and constraints
   - Create indexes for better performance
   - Insert the default Administrator role

## Running the Application

1. In Visual Studio, press F5 or click the "Run" button
2. The application will open in your default browser
3. Register a new user account
4. To create an admin user:
   - Register a new user
   - Execute the following SQL query:
     ```sql
     USE FilmLibrary;
     GO
     INSERT INTO UserRoles (UserId, RoleId)
     SELECT u.UserId, r.RoleId
     FROM Users u, Roles r
     WHERE u.Email = 'your-email@example.com'
     AND r.RoleName = 'Administrator';
     ```

## Project Structure

```
FilmLibrary/
├── Controllers/         # MVC Controllers
├── Models/             # Data models
├── Views/              # Razor views
├── Services/           # Business logic
├── Data/              # Data access layer
├── wwwroot/           # Static files
│   ├── css/          # Stylesheets
│   ├── js/           # JavaScript files
│   └── lib/          # Third-party libraries
└── Database/          # Database scripts
```

## Security Features

- Password hashing using BCrypt
- Role-based authorization
- Secure cookie authentication
- Input validation
- XSS protection
- CSRF protection

## Performance Optimizations

- Database indexes for frequently queried fields
- Efficient entity relationships
- Caching for movie listings
- Optimized image loading
- Responsive design for all screen sizes

## Testing

1. Test user registration and login
2. Test movie creation and editing
3. Test rating system
4. Test admin features
5. Test responsive design on different devices

## Deployment Considerations

- Use HTTPS in production
- Set up proper error logging
- Configure proper session timeout
- Set up proper database backup
- Configure proper security headers 