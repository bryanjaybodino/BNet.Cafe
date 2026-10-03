SELECT 
    COUNT(it.`DBId`) AS DBTotalOrders,
    IFNULL(SUM(CAST(it.`DBQuantity` AS SIGNED)), 0) AS DBTotalItemsSold,
    FORMAT(IFNULL(SUM(CAST(it.`DBQuantity` AS DECIMAL(18,2)) * CAST(it.`DBCost` AS DECIMAL(18,2))), 0), 2) AS DBTotalRevenue,
    FORMAT(
        IFNULL(
            SUM(CAST(it.`DBQuantity` AS DECIMAL(18,2)) * CAST(it.`DBCost` AS DECIMAL(18,2))) / 
            NULLIF(COUNT(it.`DBId`), 0), 
            0
        ), 
        2
    ) AS DBAvgOrderValue
FROM `inventory_transactions` it
LEFT JOIN `inventory_items` ii ON it.`DBItemId` = ii.`DBId`
LEFT JOIN `users` u ON it.`DBUserId` = u.`DBId`
WHERE 1 = 1
  AND it.`DBIsDeleted` = '{DBIsDeleted}'
  AND it.`DBTransactionType` ='{DBTransactionType}'
  AND it.DBDateCreated BETWEEN '{DBDateStart}' AND '{DBDateEnd}'
  AND ( ii.`DBItemName` LIKE '%{DBSearch}%' OR u.`DBName` LIKE '%{DBSearch}%' OR it.`DBId` LIKE '%{DBSearch}%');