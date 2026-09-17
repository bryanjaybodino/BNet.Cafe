SELECT * FROM pricing_rates 
WHERE 1 
AND DBIsDeleted = '{DBIsDeleted}' 
ORDER BY DBId DESC 
{LIMIT}