SELECT 
    p.DBUserId,
    IFNULL(u.DBEmail, 'Guest / Walk-in') AS DBEmail,
    SUM(CAST(p.DBPoints AS DECIMAL(18,2))) AS DBTotalPoints
FROM points p
LEFT JOIN users u ON p.DBUserId = u.DBId
WHERE 1=1
  AND p.DBIsDeleted = 'FALSE'
  AND p.DBUserId = '{DBUserId}'
GROUP BY p.DBUserId, u.DBEmail
ORDER BY DBTotalPoints DESC
{LIMIT}