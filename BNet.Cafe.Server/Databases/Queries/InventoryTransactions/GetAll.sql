SELECT * FROM inventory_transactions 
WHERE 1 
AND DBIsDeleted = '{DBIsDeleted}' 
AND DBTransactionType LIKE '%{DBTransactionType}%'
ORDER BY DBId DESC 
{LIMIT}