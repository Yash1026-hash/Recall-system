CREATE OR ALTER PROCEDURE dbo.usp_CountUsersByDepartment
    @DepartmentName NVARCHAR(100),
    @UserCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @UserCount = COUNT(u.UserId)
    FROM dbo.KS_RecallUsers AS u
    INNER JOIN dbo.KS_Departments AS d ON u.DepartmentId = d.DepartmentId
    WHERE d.Name = @DepartmentName;
END;
