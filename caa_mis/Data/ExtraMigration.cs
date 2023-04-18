using Microsoft.EntityFrameworkCore.Migrations;

namespace caa_mis.Data
{
    public static class ExtraMigration
    {
        public static void Steps(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
             @"
                    Create View StockSummaryByBranch as
                    SELECT s.ID, b.ID as BranchID, b.Name as BranchName, 
                            i.Name as ItemName, i.Cost as ItemCost, s.Quantity, s.MinLevel                        
                    FROM Items i
                    LEFT JOIN Stocks s on i.ID = s.ItemID
                    INNER JOIN Branches b on s.BranchID = b.ID
                ");

            migrationBuilder.Sql(
             @"
                    CREATE VIEW EventSummary AS
                    SELECT ei.ID, ei.EventID, e.Name AS EventName,
                           e.EmployeeID, emp.FirstName AS EmployeeName,
                           e.BranchID, b.Name AS BranchName,
                           e.TransactionStatusID, ts.Name AS TransactionStatusName,
                           e.Date AS EventDate, ei.ItemID, i.Name AS ItemName,
                           ei.Quantity AS EventQuantity
                    FROM EventItems ei
                    INNER JOIN Events e ON ei.EventID = e.ID
                    INNER JOIN Employees emp ON e.EmployeeID = emp.ID
                    INNER JOIN Branches b ON e.BranchID = b.ID
                    INNER JOIN TransactionStatuses ts ON e.TransactionStatusID = ts.ID
                    INNER JOIN Items i ON ei.ItemID = i.ID
                ");
            migrationBuilder.Sql(
            @"
                    CREATE VIEW TransactionItemSummary AS
                    SELECT ti.ID, ti.ItemID, i.Name AS ItemName,
                           ti.TransactionID, e.FirstName AS EmployeeName,                            
                           t.OriginID, b1.Name AS OriginName,                           
                           t.DestinationID, b2.Name AS DestinationName,
                           ts.Name AS TransactionStatusName,
                           ti.Quantity
                    FROM TransactionItems ti
                    INNER JOIN Items i ON ti.ItemID = i.ID
                    INNER JOIN Transactions t ON ti.TransactionID = t.ID
                    INNER JOIN Employees e ON t.EmployeeID = e.ID
                    INNER JOIN Branches b1 ON t.OriginID = b1.ID
                    INNER JOIN Branches b2 ON t.DestinationID = b2.ID
                    INNER JOIN TransactionStatuses ts ON t.TransactionStatusID = ts.ID
                ");
            migrationBuilder.Sql(
            @"
                    CREATE VIEW ProductList AS
                    SELECT s.ItemID as ID, s.BranchID, s.Quantity, i.Name, i.SKUNumber
                    FROM STOCKS s
                    INNER JOIN ITEMS i ON (s.ItemID = i.ID)
                    WHERE i.ItemStatusID = 1
                ");
        }
    }
}
