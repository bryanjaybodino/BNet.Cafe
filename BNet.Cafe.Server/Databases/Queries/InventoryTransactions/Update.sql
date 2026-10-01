UPDATE `inventory_transactions` 
SET 
    `DBItemId` = '{DBItemId}',
    `DBUserId` = '{DBUserId}',
    `DBTransactionType` = '{DBTransactionType}',
    `DBQuantity` = '{DBQuantity}'
WHERE `DBId` = '{DBId}';