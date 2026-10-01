SELECT * FROM inventory_items 
WHERE 1 
AND DBIsDeleted = '{DBIsDeleted}' 
AND DBItemName LIKE '%{DBItemName}%'
ORDER BY DBId DESC 
{LIMIT}