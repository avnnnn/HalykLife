GO
CREATE TABLE [dbo].[R_CURRENCY] (
    [ID]     INT             IDENTITY (1, 1) NOT NULL,
    [TITLE]  NVARCHAR (60)   COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [CODE]   VARCHAR (3)     NOT NULL,
    [VALUE]  NUMERIC (18, 2) NOT NULL,
    [A_DATE] DATE            NOT NULL
);
GO


CREATE PROCEDURE [dbo].[sp_GetRates]
	@A_DATE date,
	@CODE VARCHAR (3) = NULL
AS
if(@CODE is null)
BEGIN
	SELECT * 
	FROM dbo.R_CURRENCY
	WHERE A_DATE = @A_DATE
END

SELECT *
FROM dbo.R_CURRENCY
WHERE A_DATE = @A_DATE and CODE = @CODE

RETURN 0
