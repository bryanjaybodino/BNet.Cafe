SELECT * FROM computers 
WHERE 1 
AND DBIsDeleted ='{DBIsDeleted}' 
AND DBComputerName LIKE '%{DBComputerName}%'
ORDER BY DBId DESC 
{LIMIT}