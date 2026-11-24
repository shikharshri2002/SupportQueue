CREATE OR ALTER PROCEDURE dbo.GetTicketDashboardStats
    @UserId uniqueidentifier = NULL,
    @EmployeeOnly bit = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) AS TotalTickets,
        COALESCE(SUM(CASE WHEN Status = 'Open' THEN 1 ELSE 0 END), 0) AS OpenTickets,
        COALESCE(SUM(CASE WHEN Status = 'InProgress' THEN 1 ELSE 0 END), 0) AS InProgressTickets,
        COALESCE(SUM(CASE WHEN Status = 'Resolved' THEN 1 ELSE 0 END), 0) AS ResolvedTickets,
        COALESCE(SUM(CASE WHEN Priority = 'Urgent' AND Status <> 'Resolved' THEN 1 ELSE 0 END), 0) AS UrgentTickets,
        CAST(CASE WHEN COUNT(*) = 0 THEN 0
            ELSE 100.0 * SUM(CASE WHEN Status = 'Resolved' THEN 1 ELSE 0 END) / COUNT(*)
        END AS float) AS ResolutionPercent
    FROM dbo.Tickets
    WHERE @EmployeeOnly = 0 OR CreatedById = @UserId;
END;
