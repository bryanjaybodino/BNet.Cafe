SELECT 
    COUNT(`DBId`) AS `DBTotal`,
    SUM(CASE WHEN `DBQuantityInStock` = 0 THEN 1 ELSE 0 END) AS `DBOutOfStock`,
    SUM(CASE WHEN `DBQuantityInStock` > 0 AND `DBQuantityInStock` <= `DBReorderLevel` THEN 1 ELSE 0 END) AS `DBLowStock`
FROM `inventory_items`
WHERE 1
  AND `DBIsDeleted` = {DBIsDeleted};