-- Update DBIsDeleted = FALSE if the inventory item already exists
UPDATE inventory_items
    SET DBIsDeleted = 'FALSE'
        WHERE DBItemName = '{DBItemName}';

-- Check if ItemName exists
SELECT 
    (
        SELECT COUNT(*) 
        FROM inventory_items 
        WHERE DBItemName = '{DBItemName}'
    ) AS ItemNameExist;

-- Insert new record if no conflict exists
INSERT INTO inventory_items (
    DBItemName,
    DBCategory,
    DBUnitPrice,
    DBQuantityInStock,
    DBReorderLevel,
    DBDateCreated,
    DBTimeCreated,
    DBIsDeleted
)
SELECT 
    '{DBItemName}',
    '{DBCategory}',
    '{DBUnitPrice}',
    '{DBQuantityInStock}',
    '{DBReorderLevel}',
    '{DBDateCreated}',
    '{DBTimeCreated}',
    '{DBIsDeleted}'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1
    FROM inventory_items
    WHERE DBItemName = '{DBItemName}'
);