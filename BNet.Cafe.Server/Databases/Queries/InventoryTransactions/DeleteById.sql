-- Step 1: Deduct the stock quantity from inventory_items
UPDATE `inventory_items` i
JOIN `inventory_transactions` t ON i.`DBId` = t.`DBItemId`
SET i.`DBQuantityInStock` = GREATEST(0, i.`DBQuantityInStock` - t.`DBQuantity`)
WHERE t.`DBId` = '{DBId}' 
  AND t.`DBIsDeleted` = 'FALSE';

-- Step 2: Mark the transaction record as deleted
UPDATE `inventory_transactions` 
SET `DBIsDeleted` = 'TRUE'
WHERE `DBId` = '{DBId}';