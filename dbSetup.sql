CREATE DATABASE IF NOT EXISTS `DotNetTraining`;
USE `DotNetTraining`;

CREATE TABLE IF NOT EXISTS `Contacts` (
  `Id` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `FirstName` varchar(255) NOT NULL,
  `LastName` varchar(255),
  `Email` varchar(255) NOT NULL UNIQUE,
  `Phone` varchar(255)
) ENGINE=InnoDB;
-- INSERT INTO `Contacts` VALUES (1, 'Tuan', 'Tran', "tuan@info.vn", "0902");
-- INSERT INTO `Contacts` VALUES (2, 'user2 fn', 'user2 ln', "user2@info.vn", "1002");

