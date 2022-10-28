-- dotnet - ef migrations script --output autoMigrateDb4Docker.sql --context DotNetTraining --idempotent

-- docker exec -it <id> mysql -u root -p Admin123!

-- -- START MySQL
-- CREATE DATABASE IF NOT EXISTS `DotNetTraining`;
-- USE `DotNetTraining`;

-- CREATE TABLE IF NOT EXISTS `Contacts` (
--   `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
--   `FirstName` varchar(255) NOT NULL,
--   `LastName` varchar(255),
--   `Email` varchar(255) NOT NULL UNIQUE,
--   `Phone` varchar(255)
-- ) ENGINE=InnoDB;
-- INSERT INTO `Contacts` VALUES (1, 'Tuan', 'Tran', "tuan@info.vn", "0902");
-- INSERT INTO `Contacts` VALUES (2, 'user2 fn', 'user2 ln', "user2@info.vn", "1002");
-- -- END MySQL


-- /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Admin123!
-- select [name] from sys.databases where database_id > 4
-- SELECT name FROM sys.tables

CREATE DATABASE net_User;
GO
USE net_User;
GO

CREATE TABLE [dbo].[Users] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [UserName]  NVARCHAR (255) NOT NULL,
    [FirstName] NVARCHAR (255) NOT NULL,
    [LastName]  NVARCHAR (255) NULL,
    [Email]     NVARCHAR (255) NOT NULL,
    [Phone]     NVARCHAR (255) NULL,
    [Balance]   REAL           DEFAULT (CONVERT([real],(0))) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]([Email] ASC);
GO
SET IDENTITY_INSERT [dbo].[Users] ON
GO
INSERT INTO [dbo].[Users] ([Id], [UserName], [FirstName], [LastName], [Email], [Phone], [Balance])
VALUES(1, N'user1', N'user1 fn', N'user1 ln', N'tuan.anhtran@infodation.vn', N'090', 500);
INSERT INTO [dbo].[Users] ([Id], [UserName], [FirstName], [LastName], [Email], [Phone], [Balance])
VALUES(2, N'user2', N'user2 fn', N'user2 ln', N'elian24@ethereal.email', N'091', 1000);
GO
SET IDENTITY_INSERT [dbo].[Users] OFF
GO


CREATE DATABASE net_Report;
GO
USE net_Report;
GO

CREATE TABLE [dbo].[Reports] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [CourseName]   NVARCHAR (255) NOT NULL,
    [TotalPayment] REAL           NOT NULL,
    [Month]        INT            NOT NULL,
    [Year]         INT            NOT NULL,
    CONSTRAINT [PK_Reports] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO


CREATE DATABASE net_Course;
GO
USE net_Course;
GO

CREATE TABLE [dbo].[Courses] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Code]        NVARCHAR (255) NOT NULL,
    [Price]       REAL           NOT NULL,
    [Description] NVARCHAR (255) NULL,
    CONSTRAINT [PK_Courses] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO
SET IDENTITY_INSERT [dbo].[Courses] ON
GO
INSERT INTO [dbo].[Courses] ([Id], [Code], [Price], [Description])
VALUES (1, N'CourseA', 20, N'FrontEnd fundamental');
INSERT INTO [dbo].[Courses] ([Id], [Code], [Price], [Description])
VALUES (2, N'CourseB', 30, N'C# advanced');
INSERT INTO [dbo].[Courses] ([Id], [Code], [Price], [Description])
VALUES (3, N'CourseC', 50, N'.Net Full-Stack developer');
GO
SET IDENTITY_INSERT [dbo].[Courses] OFF
GO

CREATE TABLE [dbo].[Enrollments] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [UserId]       INT           NOT NULL,
    [EnrolledDate] DATETIME2 (7) NOT NULL,
    [CourseId]     INT           NOT NULL,
    CONSTRAINT [PK_Enrollments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Enrollments_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses] ([Id]) ON DELETE CASCADE
);
GO
CREATE NONCLUSTERED INDEX [IX_Enrollments_CourseId]
    ON [dbo].[Enrollments]([CourseId] ASC);
GO

