USE [HotelDB]
GO
/****** Object:  UserDefinedTableType [dbo].[AmenityIDTableType]  ******/
CREATE TYPE [dbo].[AmenityIDTableType] AS TABLE(
	[AmenityID] [int] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[AmenityInsertType]    ******/
CREATE TYPE [dbo].[AmenityInsertType] AS TABLE(
	[Name] [nvarchar](100) NULL,
	[Description] [nvarchar](255) NULL,
	[CreatedBy] [nvarchar](100) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[AmenityStatusType]     ******/
CREATE TYPE [dbo].[AmenityStatusType] AS TABLE(
	[AmenityID] [int] NULL,
	[IsActive] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[AmenityUpdateType]  ******/
CREATE TYPE [dbo].[AmenityUpdateType] AS TABLE(
	[AmenityID] [int] NULL,
	[Name] [nvarchar](100) NULL,
	[Description] [nvarchar](255) NULL,
	[IsActive] [bit] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[GuestDetailsTableType]    ******/
CREATE TYPE [dbo].[GuestDetailsTableType] AS TABLE(
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[Email] [nvarchar](100) NULL,
	[Phone] [nvarchar](15) NULL,
	[AgeGroup] [nvarchar](50) NULL,
	[Address] [nvarchar](500) NULL,
	[CountryId] [int] NULL,
	[StateId] [int] NULL,
	[RoomID] [int] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[RoomIDTableType]    ******/
CREATE TYPE [dbo].[RoomIDTableType] AS TABLE(
	[RoomID] [int] NULL
)
GO
