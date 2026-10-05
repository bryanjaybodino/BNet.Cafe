SELECT * FROM chat_messages 
WHERE 1 
AND DBIsDeleted = '{DBIsDeleted}' 
AND DBMessage LIKE '%{DBMessage}%'
ORDER BY DBId DESC 
{LIMIT}