UPDATE `inventory_items` 
SET 
    `DBItemName` = '{DBItemName}',
    `DBCategory` = '{DBCategory}',
    `DBUnitPrice` = '{DBUnitPrice}',
    `DBQuantityInStock` = '{DBQuantityInStock}',
    `DBReorderLevel` = '{DBReorderLevel}'
WHERE `DBId` = '{DBId}';