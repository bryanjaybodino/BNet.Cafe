SELECT * FROM chat_messages 
WHERE 1 
AND DBIsDeleted = '{DBIsDeleted}' 
-- AND DBUserId = '{DBUserId}' 
AND DBComputerId = '{DBComputerId}' 
AND DBMessage LIKE '%{DBMessage}%'
ORDER BY DBId DESC 
{LIMIT}