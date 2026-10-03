SELECT 
    COUNT(`DBId`) AS `DBTotal`
FROM `suppliers`
WHERE 1
  AND `DBIsDeleted` = {DBIsDeleted};