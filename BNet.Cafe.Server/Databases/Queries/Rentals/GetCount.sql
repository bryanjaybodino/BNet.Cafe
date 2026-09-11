SELECT 
    COUNT(r.DBId) AS DBTotalTransactions,
    
    -- Counts transactions where a valid User ID exists
    SUM(CASE WHEN r.DBUserId IS NOT NULL AND r.DBUserId != '' AND r.DBUserId != '0' THEN 1 ELSE 0 END) AS DBTotalUsers,
    
    -- Counts transactions where User ID is missing or 0 (Walk-ins/Guests)
    SUM(CASE WHEN r.DBUserId IS NULL OR r.DBUserId = '' OR r.DBUserId = '0' THEN 1 ELSE 0 END) AS DBTotalWalkIn,
    
    -- Financial Summary
    IFNULL(SUM(r.DBAmount), 0) AS DBTotalIncome,
    
    -- Total Combined Usage
    IFNULL(SUM(r.DBDuration), 0) AS DBTotalDuration,
    CASE 
        WHEN SUM(r.DBDuration) IS NULL OR SUM(r.DBDuration) = 0 THEN '0 mins'
        WHEN FLOOR(SUM(r.DBDuration) / 60) > 0 AND (SUM(r.DBDuration) % 60) > 0 THEN 
            CONCAT(FLOOR(SUM(r.DBDuration) / 60), IF(FLOOR(SUM(r.DBDuration) / 60) = 1, ' hr ', ' hrs '), (SUM(r.DBDuration) % 60), IF((SUM(r.DBDuration) % 60) = 1, ' min', ' mins'))
        WHEN FLOOR(SUM(r.DBDuration) / 60) > 0 THEN 
            CONCAT(FLOOR(SUM(r.DBDuration) / 60), IF(FLOOR(SUM(r.DBDuration) / 60) = 1, ' hr', ' hrs'))
        ELSE 
            CONCAT((SUM(r.DBDuration) % 60), IF((SUM(r.DBDuration) % 60) = 1, ' min', ' mins'))
    END AS DBTotalFormattedDuration

FROM rentals r
LEFT JOIN computers c ON r.DBComputerId = c.DBId
LEFT JOIN users u ON r.DBUserId = u.DBId
WHERE 1=1 
    AND r.DBIsDeleted = '{DBIsDeleted}'
    AND r.DBComputerId = '{DBComputerId}'  
    AND ( c.DBComputerName LIKE '%{DBSearch}%'  OR u.DBEmail LIKE '%{DBSearch}%');