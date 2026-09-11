SELECT * FROM users 
WHERE 1 
AND DBIsDeleted ='{DBIsDeleted}' 
AND (DBName LIKE '%{DBSearch}%' OR DBEmail LIKE '%{DBSearch}%')
ORDER BY DBId DESC 
{LIMIT}