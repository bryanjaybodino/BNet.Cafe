SELECT 
    b.DBId,
    b.DBUserId,
    IFNULL(u.DBEmail, 'Guest / Walk-in') AS DBEmail,
    b.DBDuration,
    CASE 
        WHEN b.DBDuration IS NULL OR b.DBDuration = 0 THEN '0 min'
        ELSE CONCAT(
            IF(b.DBDuration < 0, '-', ''),
            CASE 
                WHEN FLOOR(ABS(b.DBDuration) / 60) > 0 AND (ABS(b.DBDuration) % 60) > 0 THEN 
                    CONCAT(
                        FLOOR(ABS(b.DBDuration) / 60), IF(FLOOR(ABS(b.DBDuration) / 60) = 1, ' hr ', ' hrs '), 
                        (ABS(b.DBDuration) % 60), IF((ABS(b.DBDuration) % 60) = 1, ' min', ' mins')
                    )
                WHEN FLOOR(ABS(b.DBDuration) / 60) > 0 THEN 
                    CONCAT(FLOOR(ABS(b.DBDuration) / 60), IF(FLOOR(ABS(b.DBDuration) / 60) = 1, ' hr', ' hrs'))
                ELSE 
                    CONCAT((ABS(b.DBDuration) % 60), IF((ABS(b.DBDuration) % 60) = 1, ' min', ' mins'))
            END
        )
    END AS DBFormattedDuration,
    FORMAT(b.DBAmount, 2) AS DBAmount,
    b.DBDescription,
    b.DBDateCreated,
    b.DBTimeCreated,
    b.DBIsDeleted
FROM balances b
LEFT JOIN users u ON b.DBUserId = u.DBId
WHERE 1 = 1 
  AND b.DBIsDeleted = '{DBIsDeleted}'
  AND b.DBUserId = '{DBUserId}'
  AND (u.DBEmail LIKE '%{DBSearch}%' OR b.DBDescription LIKE '%{DBSearch}%')
ORDER BY b.DBId DESC
{LIMIT}