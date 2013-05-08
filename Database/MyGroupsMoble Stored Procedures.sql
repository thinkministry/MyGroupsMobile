USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[api_MGM_GetContactNotes]    Script Date: 05/08/2013 17:17:27 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_MGM_GetContactNotes]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[api_MGM_GetContactNotes]
GO

/****** Object:  StoredProcedure [dbo].[api_MGM_GetEventGroupAttendees]    Script Date: 05/08/2013 17:17:27 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_MGM_GetEventGroupAttendees]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[api_MGM_GetEventGroupAttendees]
GO

/****** Object:  StoredProcedure [dbo].[api_MGM_GetGroupMembersAndMeetings]    Script Date: 05/08/2013 17:17:27 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_MGM_GetGroupMembersAndMeetings]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[api_MGM_GetGroupMembersAndMeetings]
GO

/****** Object:  StoredProcedure [dbo].[api_MGM_GetMyGroups]    Script Date: 05/08/2013 17:17:27 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_MGM_GetMyGroups]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[api_MGM_GetMyGroups]
GO

USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[api_MGM_GetContactNotes]    Script Date: 05/08/2013 17:17:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[api_MGM_GetContactNotes]
	@DomainID int,
	@OwnerUserID int,
	@TargetContactID int
	
AS
BEGIN

	SELECT TOP 500
		Contact_Log_ID
		,Contact_Date
		,Notes

	FROM Contact_Log
	WHERE Domain_ID = @DomainID
		AND Made_By = @OwnerUserID
		AND Contact_ID = @TargetContactID

	ORDER BY Contact_Date DESC
	
END
GO

/****** Object:  StoredProcedure [dbo].[api_MGM_GetEventGroupAttendees]    Script Date: 05/08/2013 17:17:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[api_MGM_GetEventGroupAttendees]
	@DomainID int,
	@EventID int,
	@GroupID int,
	@CheckedInStatus int

AS
BEGIN

	SELECT DISTINCT TOP 1000
		GP.Group_Participant_ID
		,EP.Event_Participant_ID
		,C.Contact_ID
		,GP.Participant_ID
		,C.Display_Name 
		,GR.Role_Title + ' (' + GRT.Group_Role_Type + ')' AS Role_Title

	FROM Group_Participants GP
		INNER JOIN Participants P ON P.Participant_ID = GP.Participant_ID
		INNER JOIN Contacts C ON C.Contact_ID = P.Contact_ID
		INNER JOIN Group_Roles GR ON GR.Group_Role_ID = GP.Group_Role_ID
		INNER JOIN Group_Role_Types GRT ON GRT.Group_Role_Type_ID = GR.Group_Role_Type_ID
		LEFT OUTER JOIN Event_Participants EP ON EP.Group_Participant_ID = GP.Group_Participant_ID
			AND EP.Event_ID = @EventID
			AND EP.Participation_Status_ID = @CheckedInStatus
	WHERE 
		GP.Domain_ID = @DomainID
		AND GP.Group_ID = @GroupID
		AND gp.[Start_Date] <= getdate() 
		AND (gp.End_Date IS NULL OR gp.End_Date > getdate())
	ORDER BY C.Display_Name
	
END
GO

/****** Object:  StoredProcedure [dbo].[api_MGM_GetGroupMembersAndMeetings]    Script Date: 05/08/2013 17:17:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[api_MGM_GetGroupMembersAndMeetings]
	@DomainID int,
	@GroupID int


AS
BEGIN

	DECLARE @ContactsPageID int
	SELECT TOP 1 @ContactsPageID = Page_ID FROM dp_Pages WHERE Table_Name = 'Contacts'

	SELECT TOP 1000
		GP.Group_Participant_ID
		,C.Contact_ID
		,GP.Participant_ID
		,C.Display_Name 
		,CASE WHEN C.Email_Unlisted = 1 AND C.Email_Address IS NOT NULL THEN 'Unlisted' ELSE C.Email_Address END AS Email_Address
		,CASE WHEN C.Mobile_Phone_Unlisted = 1 AND C.Mobile_Phone IS NOT NULL THEN 'Unlisted' ELSE C.Mobile_Phone END AS Mobile_Phone
		,C.__Age AS Age
		,C.Date_of_Birth 
		,G.Primary_Contact
		,CASE WHEN Home_Phone_Unlisted = 1 AND H.Home_Phone IS NOT NULL THEN 'Unlisted' ELSE H.Home_Phone END AS Home_Phone
		,GR.Role_Title + ' (' + GRT.Group_Role_Type + ')' AS Role_Title
		,dp_Files.[File_Name]
		,dp_Files.Unique_Name
		,dp_Files.Extension
		,A.Address_Line_1
		,A.Address_Line_2
		,A.City
		,A.[State/Region] AS [State]
		,A.Postal_Code
		
	FROM Group_Participants GP
		INNER JOIN Groups G ON G.Group_ID = GP.Group_ID
		INNER JOIN Participants P ON P.Participant_ID = GP.Participant_ID
		INNER JOIN Group_Roles GR ON GR.Group_Role_ID = GP.Group_Role_ID
		INNER JOIN Group_Role_Types GRT ON GRT.Group_Role_Type_ID = GR.Group_Role_Type_ID
		INNER JOIN Contacts C ON C.Contact_ID = P.Contact_ID
		LEFT OUTER JOIN Households H ON H.Household_ID = C.Household_ID
		LEFT OUTER JOIN Addresses A ON A.Address_ID = H.Address_ID
		LEFT OUTER JOIN dp_Files ON dp_Files.Page_ID = @ContactsPageID
			AND dp_Files.Record_ID = P.Contact_ID
			AND dp_Files.Default_Image = 1
	WHERE 
		GP.Domain_ID = @DomainID
		AND GP.Group_ID = @GroupID
		AND gp.[Start_Date] <= getdate() 
		AND (gp.End_Date IS NULL OR gp.End_Date > getdate())
	ORDER BY C.Display_Name
		
		
	SELECT TOP 10
		E.Event_ID
		,E.Event_Title
		,E.Event_Start_Date
		,E.Event_End_Date
		
	FROM Event_Groups EG
	INNER JOIN [Events] E ON E.Event_ID = EG.Event_ID
	WHERE EG.Domain_ID = @DomainID
		AND EG.Group_ID = @GroupID
		AND E.Event_Start_Date >= getdate()-1
		
END


GO

/****** Object:  StoredProcedure [dbo].[api_MGM_GetMyGroups]    Script Date: 05/08/2013 17:17:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE PROCEDURE [dbo].[api_MGM_GetMyGroups]
	@DomainID int
	,@ContactID int

AS
BEGIN

	SELECT 
		g.Group_ID
		,g.Group_Name
		,CASE WHEN @ContactID = g.Primary_Contact THEN 'Group Primary Contact'
		      WHEN @ContactID = PG.Primary_Contact THEN 'Parent Group Contact' 
		      ELSE LTRIM(MIN(CASE gr.Group_Role_Type_ID WHEN 1 THEN ' ' + gr.Role_Title ELSE gr.Role_Title END)) 
		      END AS Role_Title
		,m.Ministry_Name
		,m.Ministry_ID
		,g.Congregation_ID
		
	FROM Groups g
		INNER JOIN Ministries m ON m.Ministry_ID = g.Ministry_ID	
		LEFT OUTER JOIN Group_Participants gp ON gp.Group_ID = g.Group_ID
		LEFT OUTER JOIN Participants p on p.Participant_ID = gp.Participant_ID
		LEFT OUTER JOIN Group_Roles gr ON gr.Group_Role_ID = gp.Group_Role_ID
		LEFT OUTER JOIN Group_Role_Types grt ON grt.Group_Role_Type_ID = gr.Group_Role_Type_ID
		LEFT OUTER JOIN Groups PG ON PG.Group_ID = G.Parent_Group  
	WHERE 
		g.Domain_ID = @DomainID
		AND g.Available_Online = 1
		AND (p.Contact_ID = @ContactID OR g.Primary_Contact = @ContactID OR PG.Primary_Contact = @ContactID)
		AND (gp.[Start_Date] <= getdate() 
		AND (gp.End_Date IS NULL OR gp.End_Date > getdate()) OR g.Primary_Contact = @ContactID)

	GROUP BY g.Group_ID
		,g.Group_Name
		,g.Primary_Contact
		,PG.Primary_Contact
		,m.Ministry_Name
		,m.Ministry_ID
		,g.Congregation_ID
		
	  ORDER BY g.Group_Name
END






GO


