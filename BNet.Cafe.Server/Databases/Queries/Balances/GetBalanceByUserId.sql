SELECT 
    b.DBUserId,
    IFNULL(u.DBEmail, 'Guest / Walk-in') AS DBEmail,
    SUM(b.DBDuration) AS DBTotalDuration,
    CASE 
        WHEN SUM(b.DBDuration) IS NULL OR SUM(b.DBDuration) = 0 THEN '0 min'
        ELSE CONCAT(
            IF(SUM(b.DBDuration) < 0, '-', ''),
            CASE 
                WHEN FLOOR(ABS(SUM(b.DBDuration)) / 60) > 0 AND (ABS(SUM(b.DBDuration)) % 60) > 0 THEN 
                    CONCAT(
                        FLOOR(ABS(SUM(b.DBDuration)) / 60), IF(FLOOR(ABS(SUM(b.DBDuration)) / 60) = 1, ' hr ', ' hrs '), 
                        (ABS(SUM(b.DBDuration)) % 60), IF((ABS(SUM(b.DBDuration)) % 60) = 1, ' min', ' mins')
                    )
                WHEN FLOOR(ABS(SUM(b.DBDuration)) / 60) > 0 THEN 
                    CONCAT(FLOOR(ABS(SUM(b.DBDuration)) / 60), IF(FLOOR(ABS(SUM(b.DBDuration)) / 60) = 1, ' hr', ' hrs'))
                ELSE 
                    CONCAT((ABS(SUM(b.DBDuration)) % 60), IF((ABS(SUM(b.DBDuration)) % 60) = 1, ' min', ' mins'))
            END
        )
    END AS DBFormattedTotalDuration
FROM balances b
LEFT JOIN users u ON b.DBUserId = u.DBId
WHERE 1=1
  AND b.DBIsDeleted = 'FALSE'
  AND b.DBUserId = '{DBUserId}'
GROUP BY b.DBUserId, u.DBEmail
ORDER BY DBTotalDuration DESC
{LIMIT}