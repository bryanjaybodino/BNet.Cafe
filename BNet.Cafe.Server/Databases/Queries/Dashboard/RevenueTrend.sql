SELECT 
    r.DBDateCreated AS DBDate,
    FORMAT(IFNULL(SUM(r.DBAmount), 0), 2) AS DBTotalAmount
FROM rentals r
WHERE 1 = 1
  AND r.DBIsDeleted = '{DBIsDeleted}'
  AND r.DBDateCreated BETWEEN '{DBDateStart}' AND '{DBDateEnd}'
GROUP BY r.DBDateCreated
ORDER BY r.DBDateCreated ASC;