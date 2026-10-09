SELECT * FROM client_config
WHERE DBIsDeleted = '{DBIsDeleted}'
ORDER BY DBId DESC 
{LIMIT}