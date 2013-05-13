USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[api_SDK_GetUserRoles]    Script Date: 05/09/2013 09:13:55 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_SDK_GetUserRoles]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[api_SDK_GetUserRoles]
GO

/****** Object:  StoredProcedure [dbo].[api_SDK_GetDataRecord]    Script Date: 05/09/2013 09:13:55 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_SDK_GetDataRecord]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[api_SDK_GetDataRecord]
GO

/****** Object:  StoredProcedure [dbo].[api_SDK_GetMainPageSelection]    Script Date: 05/09/2013 09:13:55 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_SDK_GetMainPageSelection]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[api_SDK_GetMainPageSelection]
GO

USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[api_SDK_GetUserRoles]    Script Date: 05/09/2013 09:13:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Wareham, Stephen
-- Create date: 13 April 2013
-- Description:	Returns all security Roles for given User
-- =============================================
CREATE PROCEDURE [dbo].[api_SDK_GetUserRoles]

	@DomainID int
	,@UserID int
AS
BEGIN

	SELECT Role_ID FROM dp_User_Roles
	WHERE [User_ID] = @UserID AND Domain_ID = @DomainID

END


GO

/****** Object:  StoredProcedure [dbo].[api_SDK_GetDataRecord]    Script Date: 05/09/2013 09:13:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Wareham, Stephen
-- Create date: 13 April 2013
-- Description:	Returns schema and value(if RecordId > 0) for given Table and Record
-- =============================================
CREATE PROCEDURE [dbo].[api_SDK_GetDataRecord]

	@DomainID int, 
	@TableName varchar(75),
	@RecordId int = 0

AS
BEGIN

	DECLARE @PageID int
	DECLARE @PrimaryKeyName varchar(50)
	DECLARE @RecordSQL varchar(500)

	SELECT @PageID = Page_ID FROM dp_Pages WHERE Table_Name = @TableName AND Filter_Clause IS NULL

	SELECT @PrimaryKeyName = C.name FROM sys.objects T 
		INNER JOIN sys.index_columns I on I.object_id = T.object_id AND I.index_id = 1
		INNER JOIN sys.columns C ON C.object_id = T.object_id AND C.column_id = I.column_id
		WHERE T.type = 'U' AND T.name = @TableName
	
	SELECT
		C.column_id AS Field_Order	
		,C.name AS Field_Name
		,CASE WHEN FK.name IS NOT NULL THEN
			CASE WHEN FKFields.name <> C.name THEN REPLACE(C.name, FKFields.name, '')  
			     WHEN T.name = FK.name THEN 'Parent ' + P.Singular_Name ELSE P.Singular_Name END 
			ELSE REPLACE(C.name, '_', ' ') END AS Field_Label 
		,D.value AS Field_Description
		,CASE sys.index_columns.index_id WHEN 1 THEN 'true' Else 'false' END AS Is_Primary_Key
		,Y.name AS Data_Type
		,C.max_length AS Data_Size
		,CASE C.is_nullable WHEN 0 THEN 'true' Else 'false' END AS Is_Required
		,C.is_computed AS Is_Computed
		
		,ISNULL(FK.name, '') AS Foreign_Table_Name		
		,ISNULL(P.Selected_Record_Expression, '') AS Foreign_Expression
		,ISNULL(FKFields.name, '') AS Foreign_Field_Name
		
	FROM sys.columns C
		INNER JOIN sys.objects T ON t.object_id = C.object_id
		INNER JOIN sys.types Y ON Y.user_type_id = C.system_type_id
			--AND Y.user_type_id = C.user_type_id
		LEFT OUTER JOIN sys.index_columns on sys.index_columns.object_id = T.object_id
			AND sys.index_columns.column_id = C.column_id
		LEFT OUTER JOIN sys.foreign_key_columns ON sys.foreign_key_columns.parent_object_id = C.object_id 
			AND sys.foreign_key_columns.parent_column_id = C.column_id
		LEFT OUTER JOIN sys.objects FK on FK.object_id = sys.foreign_key_columns.referenced_object_id
		LEFT OUTER JOIN sys.columns FKFields on FKFields.object_id = FK.object_id 
			AND FKFields.column_id = sys.foreign_key_columns.referenced_column_id

		LEFT OUTER JOIN dp_Pages P ON P.Table_Name = FK.[name] AND P.Filter_Clause IS NULL
		LEFT OUTER JOIN sys.extended_properties D ON D.major_id = T.object_id 
			AND D.minor_id = C.column_id
			
	WHERE T.type = 'U' 
		AND T.name = @TableName
		
	ORDER BY T.name, C.column_id

	SET @RecordSQL = 'SELECT * FROM ' + @TableName + ' WHERE ' + @PrimaryKeyName + ' = ' + CONVERT(varchar(15), @RecordId)
	--PRINT(@RecordSQL)
	EXEC(@RecordSQL)
	

END


GO

/****** Object:  StoredProcedure [dbo].[api_SDK_GetMainPageSelection]    Script Date: 05/09/2013 09:13:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Wareham, Stephen
-- Create date: 13 April 2013
-- Description:	Returns all rows for given Selection ID
-- =============================================
CREATE PROCEDURE [dbo].[api_SDK_GetMainPageSelection]

	@DomainID int,
	@UserID int,
	@SelectionID int
	
AS
BEGIN
	DECLARE @sql varchar(8000)
	
	DECLARE @TableName varchar(50)
	DECLARE @PrimaryKeyName varchar(50)
	DECLARE @PageID int
	
	SELECT 
		@TableName = dp_Pages.Table_Name
		,@PrimaryKeyName = dp_Pages.Primary_Key 
		,@PageID = dp_Pages.Page_ID
	FROM dp_Pages 
	INNER JOIN dp_Selections ON dp_Selections.Page_ID = dp_Pages.Page_ID WHERE dp_Selections.Selection_ID = @SelectionID
	

	SET @sql = 'SELECT * FROM ' + @TableName + ' 
		INNER JOIN dp_Selected_Records SR ON SR.Record_ID = ' + @PrimaryKeyName + '
		INNER JOIN dp_Selections S ON S.Selection_ID = SR.Selection_ID


		WHERE S.[User_ID] = ' + CONVERT(varchar(10), @UserID) + ' AND S.Page_ID = ' + CONVERT(varchar(10), @PageID) + '
		AND (
			(S.Selection_ID = ' + CONVERT(varchar(10), @SelectionID) + ' AND ' + CONVERT(varchar(10), @SelectionID) + ' > 0) 
			OR 
			(S.Selection_Name = ''dp_DEFAULT'' AND ' + CONVERT(varchar(10), @SelectionID) + ' < 1 AND Sub_Page_ID IS NULL)
			)'
	  
	  EXEC(@sql)
	  
END


GO


