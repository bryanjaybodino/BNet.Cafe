SELECT 
    b.DBUserId,
    IFNULL(u.DBName, CONCAT('User #', b.DBUserId)) AS MemberName,
    COUNT(b.DBId) AS TopUpCount,
    SUM(CAST(b.DBAmount AS DECIMAL(10,2))) AS TotalTopUpAmount,
    SUM(b.DBDuration) AS TotalMinutesAdded
FROM balances b
LEFT JOIN users u ON b.DBUserId = u.DBId
WHERE b.DBIsDeleted = '{DBIsDeleted}'
  AND b.DBDescription = 'TOP-UP LOAD CREDIT'
  AND (b.DBDateCreated BETWEEN '{DBDateStart}' AND '{DBDateEnd}' OR '{DBDateStart}' = '')
GROUP BY b.DBUserId, MemberName
ORDER BY TotalTopUpAmount DESC
LIMIT 10;