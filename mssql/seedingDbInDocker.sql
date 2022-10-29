CREATE DATABASE net_User;
GO
USE net_User;
GO

CREATE TABLE Users (
	Id INT NOT NULL IDENTITY,
	UserName NVARCHAR (255) NOT NULL,
	FirstName NVARCHAR (255) NOT NULL,
	LastName NVARCHAR (255) NOT NULL,
	Email NVARCHAR (255) NOT NULL,
	Phone NVARCHAR (255) NOT NULL,
    Balance REAL DEFAULT (CONVERT([real],(0))) NOT NULL,
	PRIMARY KEY (Id)
);
GO

INSERT INTO Users (UserName, FirstName, LastName, Email, Phone, Balance)
VALUES 
('user1', 'user1 fn', 'user1 ln', 'tuan.anhtran@infodation.vn', '090', 500),
('user2', 'user2 fn', 'user2 ln', 'elian24@ethereal.email', '091', 1000);
GO
