SELECT 
    it.`DBId`,
    it.`DBItemId`,
    ii.`DBItemName`,
    it.`DBUserId`,
    u.`DBName` AS DBUserName,
    it.`DBTransactionType`,
    it.`DBQuantity`,
    it.`DBCost`,
    (it.`DBQuantity` * it.`DBCost`) AS DBTotalCost,
    it.`DBDateCreated`,
    it.`DBTimeCreated`,
    it.`DBIsDeleted`
FROM `inventory_transactions` it
LEFT JOIN `inventory_items` ii ON it.`DBItemId` = ii.`DBId`
LEFT JOIN `users` u ON it.`DBUserId` = u.`DBId`
WHERE 1 = 1
  AND it.`DBIsDeleted` = '{DBIsDeleted}'
  AND it.`DBTransactionType` LIKE '%{DBTransactionType}%'
ORDER BY it.`DBId` DESC
{LIMIT}