SELECT 
    r.DBId,
    r.DBComputerId,
    c.DBComputerName,
    r.DBUserId,
    IFNULL(u.DBEmail, 'Guest / Walk-in') AS DBEmail,
    r.DBDuration,
    CASE 
        WHEN r.DBDuration IS NULL OR r.DBDuration = 0 THEN '0 min'
        WHEN FLOOR(r.DBDuration / 60) > 0 AND (r.DBDuration % 60) > 0 THEN 
            CONCAT(FLOOR(r.DBDuration / 60), IF(FLOOR(r.DBDuration / 60) = 1, ' hr ', ' hrs '), (r.DBDuration % 60), IF((r.DBDuration % 60) = 1, ' min', ' mins'))
        WHEN FLOOR(r.DBDuration / 60) > 0 THEN 
            CONCAT(FLOOR(r.DBDuration / 60), IF(FLOOR(r.DBDuration / 60) = 1, ' hr', ' hrs'))
        ELSE 
            CONCAT((r.DBDuration % 60), IF((r.DBDuration % 60) = 1, ' min', ' mins'))
    END AS DBFormattedDuration,
    r.DBAmount,
    r.DBDateCreated,
    r.DBTimeCreated,
    r.DBIsDeleted
FROM rentals r
LEFT JOIN computers c ON r.DBComputerId = c.DBId
LEFT JOIN users u ON r.DBUserId = u.DBId
WHERE 1 = 1 
  AND r.DBIsDeleted = '{DBIsDeleted}'
  AND r.DBComputerId = '{DBComputerId}'
  AND (c.DBComputerName LIKE '%{DBSearch}%'  OR u.DBEmail LIKE '%{DBSearch}%' )
ORDER BY r.DBId DESC
{LIMIT}