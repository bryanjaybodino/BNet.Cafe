SELECT * FROM suppliers 
WHERE 1 
AND DBIsDeleted = '{DBIsDeleted}' 
AND DBSupplierName LIKE '%{DBSupplierName}%'
ORDER BY DBId DESC 
{LIMIT}