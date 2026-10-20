SELECT 
    p.DBId,
    p.DBUserId,
    IFNULL(u.DBEmail, '--') AS DBEmail,
    IFNULL(u.DBName, 'Guest / Walk-in') AS DBName,
    IFNULL(p.DBDescription, 'Points Adjustment') AS DBDescription,
    FORMAT(p.DBPoints, 2) AS DBPoints,
    p.DBDateCreated,
    p.DBTimeCreated,
    p.DBIsDeleted
FROM points p
LEFT JOIN users u ON p.DBUserId = u.DBId
WHERE 1 = 1 
  AND p.DBIsDeleted = '{DBIsDeleted}'
  AND p.DBUserId = '{DBUserId}'
  AND p.DBDateCreated BETWEEN '{DBDateStart}' AND '{DBDateEnd}'
  AND (u.DBEmail LIKE '%{DBSearch}%' OR u.DBName LIKE '%{DBSearch}%' OR p.DBDescription LIKE '%{DBSearch}%')
ORDER BY p.DBId DESC
{LIMIT}