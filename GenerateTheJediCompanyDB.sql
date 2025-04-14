USE [master]
GO

CREATE DATABASE [TheCodeJediCompany]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'TheCodeJediCompany', FILENAME = N'F:\SQL Server\UserData\TheCodeJediCompany.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'TheCodeJediCompany_log', FILENAME = N'D:\SQL Server\UserLog\TheCodeJediCompany_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO

ALTER DATABASE [TheCodeJediCompany] SET COMPATIBILITY_LEVEL = 160
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [TheCodeJediCompany].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [TheCodeJediCompany] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET ARITHABORT OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [TheCodeJediCompany] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [TheCodeJediCompany] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET  DISABLE_BROKER 
GO

ALTER DATABASE [TheCodeJediCompany] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [TheCodeJediCompany] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET RECOVERY FULL 
GO

ALTER DATABASE [TheCodeJediCompany] SET  MULTI_USER 
GO

ALTER DATABASE [TheCodeJediCompany] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [TheCodeJediCompany] SET DB_CHAINING OFF 
GO

ALTER DATABASE [TheCodeJediCompany] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [TheCodeJediCompany] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO

ALTER DATABASE [TheCodeJediCompany] SET DELAYED_DURABILITY = DISABLED 
GO

ALTER DATABASE [TheCodeJediCompany] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO

EXEC sys.sp_db_vardecimal_storage_format N'TheCodeJediCompany', N'ON'
GO

ALTER DATABASE [TheCodeJediCompany] SET QUERY_STORE = ON
GO

ALTER DATABASE [TheCodeJediCompany] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO

USE [TheCodeJediCompany]
GO

CREATE SCHEMA [ods]
GO

CREATE SCHEMA [hash]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [ods].[Company](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](1024) NOT NULL,
	[Hash] [varbinary](64) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[ModifiedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_Company] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [ods].[Company] ADD  CONSTRAINT [DF_Company_Key]  DEFAULT (newid()) FOR [Key]
GO

CREATE FUNCTION [hash].[Company](
	--Key
	@Key [uniqueidentifier],
	--Data
	@Name [nvarchar](255),
	@Description [nvarchar](1024),
	@ModifiedAt [datetime2](7)
)
RETURNS [varbinary](64)
AS
BEGIN 
	DECLARE @result varbinary(64);

	SELECT @result = Hashbytes('SHA2_512', (SELECT 
		[Key] = @Key, [Name] = @Name, [Description] = @Description, [ModifiedAt] = @ModifiedAt
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
	)

	RETURN @result;
END
GO

CREATE TRIGGER [ods].[Company.Trigger.Insert]
ON [ods].[Company]
AFTER INSERT
AS
BEGIN
	UPDATE [ods].[Company]
	SET [CreatedAt] = SYSDATETIME()
	FROM [ods].[Company] AS c
	JOIN inserted AS i ON c.[Id] = i.[Id]
END
GO

CREATE TRIGGER [ods].[Company.Trigger.Update]
ON [ods].[Company]
FOR UPDATE
AS
BEGIN
	UPDATE [ods].[Company]
	SET [ModifiedAt] = SYSDATETIME(),
		[Hash] = [hash].[Company](c.[Key], c.[Name], c.[Description], SYSDATETIME())
	FROM [ods].[Company] AS c
	JOIN inserted AS i ON c.[Id] = i.[Id]
END
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [ods].[Department](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[Description] [nvarchar](1024) NULL,
	[CompanyId] [int] NULL,
	[Hash] [varbinary](64) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[ModifiedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_Department] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [ods].[Department] ADD  CONSTRAINT [DF_Department_Key]  DEFAULT (newid()) FOR [Key]
GO

CREATE FUNCTION [hash].[Department](
	--Key
	@Key [uniqueidentifier],
	--Data
	@Name [nvarchar](255),
	@Description [nvarchar](1024),
	@CompanyId [int],
	@ModifiedAt [datetime2](7)
)
RETURNS [varbinary](64)
AS
BEGIN 
	DECLARE @result varbinary(64);

	SELECT @result = Hashbytes('SHA2_512', (SELECT 
		[Key] = @Key, [Name] = @Name, [Description] = @Description, [CompanyId] = @CompanyId, [ModifiedAt] = @ModifiedAt
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
	)

	RETURN @result;
END
GO

CREATE TRIGGER [ods].[Department.Trigger.Insert]
ON [ods].[Department]
AFTER INSERT
AS
BEGIN
	UPDATE [ods].[Department]
	SET [CreatedAt] = SYSDATETIME()
	FROM [ods].[Department] AS d
	JOIN inserted AS i ON d.[Id] = i.[Id]
END
GO

CREATE TRIGGER [ods].[Department.Trigger.Update]
ON [ods].[Department]
FOR UPDATE
AS
BEGIN
	UPDATE [ods].[Department]
	SET [ModifiedAt] = SYSDATETIME(),
		[Hash] = [hash].[Department](d.[Key], d.[Name], d.[Description], d.[CompanyId], SYSDATETIME())
	FROM [ods].[Department] AS d
	JOIN inserted AS i ON d.[Id] = i.[Id]
END
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [ods].[Employee](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](1024) NULL,
	[DepartmentId] [int] NULL,
	[Function] [int] NULL,
	[Hash] [varbinary](64) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[ModifiedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_Employee] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [ods].[Employee] ADD  CONSTRAINT [DF_Employee_Key]  DEFAULT (newid()) FOR [Key]
GO


CREATE FUNCTION [hash].[Employee](
	--Key
	@Key [uniqueidentifier],
	--Data
	@Name [nvarchar](255),
	@Description [nvarchar](1024),
	@DepartmentId [int],
	@Function [int],
	@ModifiedAt [datetime2](7)
)
RETURNS [varbinary](64)
AS
BEGIN 
	DECLARE @result varbinary(64);

	SELECT @result = Hashbytes('SHA2_512', (SELECT 
		[Key] = @Key, [Name] = @Name, [Description] = @Description, [DepartmentId] = @DepartmentId, [Function] = @Function, [ModifiedAt] = @ModifiedAt
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
	)

	RETURN @result;
END
GO

CREATE TRIGGER [ods].[Employee.Trigger.Insert]
ON [ods].[Employee]
AFTER INSERT
AS
BEGIN
	UPDATE [ods].[Employee]
	SET [CreatedAt] = SYSDATETIME()
	FROM [ods].[Employee] AS e
	JOIN inserted AS i ON e.[Id] = i.[Id]
END
GO

CREATE TRIGGER [ods].[Employee.Trigger.Update]
ON [ods].[Employee]
FOR UPDATE
AS
BEGIN
	UPDATE [ods].[Employee]
	SET [ModifiedAt] = SYSDATETIME(),
		[Hash] = [hash].[Employee](e.[Key], e.[Name], e.[Description], e.[DepartmentId], e.[Function], SYSDATETIME())
	FROM [ods].[Employee] AS e
	JOIN inserted AS i ON e.[Id] = i.[Id]
END
GO


ALTER TABLE [ods].[Department]  WITH CHECK ADD  CONSTRAINT [FK_Department_Company] FOREIGN KEY([CompanyId])
REFERENCES [ods].[Company] ([Id])
ON DELETE SET NULL
GO

ALTER TABLE [ods].[Department] CHECK CONSTRAINT [FK_Department_Company]
GO

ALTER TABLE [ods].[Employee]  WITH CHECK ADD  CONSTRAINT [FK_Employee_Department] FOREIGN KEY([DepartmentId])
REFERENCES [ods].[Department] ([Id])
ON DELETE SET NULL
GO

ALTER TABLE [ods].[Employee] CHECK CONSTRAINT [FK_Employee_Department]
GO

USE [master]
GO

ALTER DATABASE [TheCodeJediCompany] SET  READ_WRITE 
GO

USE [TheCodeJediCompany]
GO

INSERT INTO [ods].[Company] ([Name], [Description])
VALUES 
('The Rebel Alliance','The light side of the force'),
('The Empire','The dark side of the force')
GO

INSERT INTO [ods].[Department] ([Name],[Description],[CompanyId])
VALUES
('X-wing','Fighter Squadron',(SELECT Id FROM [ods].[Company] WHERE [Name]='The Rebel Alliance')),
('Y-wing','Bomber Squadron',(SELECT Id FROM [ods].[Company] WHERE [Name]='The Rebel Alliance')),
('A-wing','Interception Squadron',(SELECT Id FROM [ods].[Company] WHERE [Name]='The Rebel Alliance')),
('U-wing','Support Squadron',(SELECT Id FROM [ods].[Company] WHERE [Name]='The Rebel Alliance')),
('B-wing','Bomber Squadron',(SELECT Id FROM [ods].[Company] WHERE [Name]='The Rebel Alliance')),
('TIE Fighter','Fighter Squandron',(SELECT Id FROM [ods].[Company] WHERE [Name]='The Empire')),
('TIE Bomber','Bomber Squadron',(SELECT Id FROM [ods].[Company] WHERE [Name]='The Empire')),
('TIE Interceptor','Interceptor Squadron',(SELECT Id FROM [ods].[Company] WHERE [Name]='The Empire')),
('TIE Reaper','Support Squadron',(SELECT Id FROM [ods].[Company] WHERE [Name]='The Empire'))
GO

INSERT INTO [ods].[Employee] ([Name], [Description], [DepartmentId], [Function])
VALUES
('Arvel Crynyd','Pilot',(SELECT Id FROM [ods].[Department] WHERE [Name] = 'Y-wing'),0),
('Barlon Hightower','Pilot',(SELECT Id FROM [ods].[Department] WHERE [Name] = 'X-wing'),0),
('Biggs Darklighter','Pilot',(SELECT Id FROM [ods].[Department] WHERE [Name] = 'X-wing'),0),
('Blue leader','Pilot',(SELECT Id FROM [ods].[Department] WHERE [Name] = 'U-wing'),0),
('Bodhi Rook','Pilot',(SELECT Id FROM [ods].[Department] WHERE [Name] = 'Y-wing'),0),	
('Broan Danurs','Pilot',(SELECT Id FROM [ods].[Department] WHERE [Name] = 'A-wing'),0),
('Captain Chedaki','Pilot',(SELECT Id FROM [ods].[Department] WHERE [Name] = 'B-wing'),0),
('Shade','Pilot',(SELECT Id FROM [ods].[Department] WHERE [Name] = 'TIE Fighter'),NULL),
('Break','Pilot',(SELECT Id FROM [ods].[Department] WHERE [Name] = 'TIE Fighter'),NULL),
('Drive','Pilot',(SELECT Id FROM [ods].[Department] WHERE [Name] = 'TIE Fighter'),NULL),
('Axel','Pilot',(SELECT Id FROM [ods].[Department] WHERE [Name] = 'TIE Bomber'),NULL),
('Battle','Pilot',(SELECT Id FROM [ods].[Department] WHERE [Name] = 'TIE Interceptor'),NULL),
('Cannon','Pilot',(SELECT Id FROM [ods].[Department] WHERE [Name] = 'TIE Interceptor'),NULL),
('Exit','Pilot',(SELECT Id FROM [ods].[Department] WHERE [Name] = 'TIE Reaper'),NULL)
GO