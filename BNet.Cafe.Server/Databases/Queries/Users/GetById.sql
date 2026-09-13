SELECT 
    u.DBId,
    u.DBName,
    u.DBEmail,
    u.DBRole,
    u.DBPassword,
    u.DBDateCreated,
    u.DBTimeCreated,
    u.DBIsDeleted,
    IFNULL(b.DBTotalDuration, 0) AS DBTotalDuration,
    CASE 
        WHEN b.DBTotalDuration IS NULL OR b.DBTotalDuration <= 0 THEN '0 min'
        WHEN FLOOR(b.DBTotalDuration / 60) > 0 AND (b.DBTotalDuration % 60) > 0 THEN 
            CONCAT(FLOOR(b.DBTotalDuration / 60), IF(FLOOR(b.DBTotalDuration / 60) = 1, ' hr ', ' hrs '), (b.DBTotalDuration % 60), IF((b.DBTotalDuration % 60) = 1, ' min', ' mins'))
        WHEN FLOOR(b.DBTotalDuration / 60) > 0 THEN 
            CONCAT(FLOOR(b.DBTotalDuration / 60), IF(FLOOR(b.DBTotalDuration / 60) = 1, ' hr', ' hrs'))
        ELSE 
            CONCAT((b.DBTotalDuration % 60), IF((b.DBTotalDuration % 60) = 1, ' min', ' mins'))
    END AS DBFormattedTotalDuration
FROM users u
LEFT JOIN (
    SELECT 
        DBUserId, 
        SUM(DBDuration) AS DBTotalDuration
    FROM balances
    WHERE DBIsDeleted = 'FALSE'
    GROUP BY DBUserId
) b ON u.DBId = b.DBUserId
WHERE 1 = 1
  AND u.DBIsDeleted = '{DBIsDeleted}'
  AND u.DBId = '{DBId}'