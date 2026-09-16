SELECT 
    IFNULL(c.DBComputerName, 'Unknown') AS DBComputerName,
    COUNT(r.DBId) AS DBCount
FROM rentals r
LEFT JOIN computers c ON r.DBComputerId = c.DBId
WHERE 1 = 1
  AND r.DBIsDeleted = '{DBIsDeleted}'
  AND r.DBDateCreated BETWEEN '{DBDateStart}' AND '{DBDateEnd}'
GROUP BY c.DBComputerName
ORDER BY DBCount DESC;