SELECT * FROM points_rates 
WHERE 1 
AND DBCustomerType = '{DBCustomerType}' 
AND DBIsDeleted = '{DBIsDeleted}' 
ORDER BY CAST(DBMinutes AS UNSIGNED) DESC 
{LIMIT}