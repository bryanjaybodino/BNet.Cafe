SELECT 
    COUNT(p.DBId) AS DBTotalTransactions,
    COUNT(DISTINCT p.DBUserId) AS DBTotalUsers,
    FORMAT(IFNULL(SUM(CAST(p.DBPoints AS DECIMAL(18,2))), 0), 2) AS DBTotalPointsEarned
FROM points p
WHERE 1 = 1 
  AND p.DBIsDeleted = '{DBIsDeleted}'
  AND p.DBUserId = '{DBUserId}'
  AND p.DBDateCreated BETWEEN '{DBDateStart}' AND '{DBDateEnd}'