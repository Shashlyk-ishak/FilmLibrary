-- Create Database
CREATE DATABASE FilmLibrary;
GO

USE FilmLibrary;
GO

-- Create Users Table
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);

-- Create Roles Table
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);

-- Create UserRoles Table (Many-to-Many relationship)
CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

-- Create Movies Table
CREATE TABLE Movies (
    MovieId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    ReleaseYear INT,
    Rating DECIMAL(3,2),
    CoverImagePath NVARCHAR(255),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NOT NULL,
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);

-- Create Actors Table
CREATE TABLE Actors (
    ActorId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Biography NVARCHAR(MAX),
    PhotoPath NVARCHAR(255)
);

-- Create Directors Table
CREATE TABLE Directors (
    DirectorId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Biography NVARCHAR(MAX),
    PhotoPath NVARCHAR(255)
);

-- Create MovieActors Table (Many-to-Many relationship)
CREATE TABLE MovieActors (
    MovieId INT NOT NULL,
    ActorId INT NOT NULL,
    Role NVARCHAR(100),
    PRIMARY KEY (MovieId, ActorId),
    FOREIGN KEY (MovieId) REFERENCES Movies(MovieId),
    FOREIGN KEY (ActorId) REFERENCES Actors(ActorId)
);

-- Create MovieDirectors Table (Many-to-Many relationship)
CREATE TABLE MovieDirectors (
    MovieId INT NOT NULL,
    DirectorId INT NOT NULL,
    PRIMARY KEY (MovieId, DirectorId),
    FOREIGN KEY (MovieId) REFERENCES Movies(MovieId),
    FOREIGN KEY (DirectorId) REFERENCES Directors(DirectorId)
);

-- Create UserRatings Table
CREATE TABLE UserRatings (
    UserId INT NOT NULL,
    MovieId INT NOT NULL,
    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 10),
    Comment NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY (UserId, MovieId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (MovieId) REFERENCES Movies(MovieId)
);

-- Insert default admin role
INSERT INTO Roles (RoleName) VALUES ('Administrator');

-- Create indexes for better performance
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Movies_Title ON Movies(Title);
CREATE INDEX IX_Movies_Rating ON Movies(Rating);
CREATE INDEX IX_Actors_Name ON Actors(Name);
CREATE INDEX IX_Directors_Name ON Directors(Name);
CREATE INDEX IX_UserRatings_MovieId ON UserRatings(MovieId); 