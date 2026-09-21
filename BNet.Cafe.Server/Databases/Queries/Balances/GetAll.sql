SELECT 
    b.DBId,
    b.DBUserId,
    IFNULL(u.DBEmail, '--') AS DBEmail,
    IFNULL(u.DBName, 'Guest / Walk-in') AS DBName,
    IFNULL(b.DBDescription, 'TopUp') AS DBDescription,
    b.DBDuration,
    CASE 
        WHEN b.DBDuration IS NULL OR b.DBDuration = 0 THEN '0 min'
        WHEN FLOOR(b.DBDuration / 60) > 0 AND (b.DBDuration % 60) > 0 THEN 
            CONCAT(FLOOR(b.DBDuration / 60), IF(FLOOR(b.DBDuration / 60) = 1, ' hr ', ' hrs '), (b.DBDuration % 60), IF((b.DBDuration % 60) = 1, ' min', ' mins'))
        WHEN FLOOR(b.DBDuration / 60) > 0 THEN 
            CONCAT(FLOOR(b.DBDuration / 60), IF(FLOOR(b.DBDuration / 60) = 1, ' hr', ' hrs'))
        ELSE 
            CONCAT((b.DBDuration % 60), IF((b.DBDuration % 60) = 1, ' min', ' mins'))
    END AS DBFormattedDuration,
    FORMAT(b.DBAmount, 2) AS DBAmount,
    b.DBDateCreated,
    b.DBTimeCreated,
    b.DBIsDeleted
FROM balances b
LEFT JOIN users u ON b.DBUserId = u.DBId
WHERE 1 = 1 
  AND b.DBIsDeleted = '{DBIsDeleted}'
  AND b.DBUserId = '{DBUserId}'
  AND b.DBDateCreated BETWEEN '{DBDateStart}' AND '{DBDateEnd}'
  AND (u.DBEmail LIKE '%{DBSearch}%' OR u.DBName LIKE '%{DBSearch}%' OR b.DBDescription LIKE '%{DBSearch}%')
ORDER BY b.DBId DESC
{LIMIT}