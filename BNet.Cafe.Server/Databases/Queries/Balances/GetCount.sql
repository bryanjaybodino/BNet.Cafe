SELECT 
    COUNT(b.DBId) AS DBTotalTransactions,
    COUNT(DISTINCT b.DBUserId) AS DBTotalUsers,
    FORMAT(IFNULL(SUM(b.DBAmount), 0), 2) AS DBTotalIncome,
    CASE 
        WHEN IFNULL(SUM(b.DBDuration), 0) = 0 THEN '0 min'
        WHEN FLOOR(SUM(b.DBDuration) / 60) > 0 AND (SUM(b.DBDuration) % 60) > 0 THEN 
            CONCAT(FLOOR(SUM(b.DBDuration) / 60), IF(FLOOR(SUM(b.DBDuration) / 60) = 1, ' hr ', ' hrs '), (SUM(b.DBDuration) % 60), IF((SUM(b.DBDuration) % 60) = 1, ' min', ' mins'))
        WHEN FLOOR(SUM(b.DBDuration) / 60) > 0 THEN 
            CONCAT(FLOOR(SUM(b.DBDuration) / 60), IF(FLOOR(SUM(b.DBDuration) / 60) = 1, ' hr', ' hrs'))
        ELSE 
            CONCAT((SUM(b.DBDuration) % 60), IF((SUM(b.DBDuration) % 60) = 1, ' min', ' mins'))
    END AS DBTotalFormattedDuration
FROM balances b
WHERE 1 = 1 
  AND b.DBIsDeleted = '{DBIsDeleted}'
  AND  b.DBUserId = '{DBUserId}'
  AND b.DBDateCreated BETWEEN '{DBDateStart}' AND '{DBDateEnd}'