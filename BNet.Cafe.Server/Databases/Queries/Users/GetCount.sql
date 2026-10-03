SELECT 
    COUNT(`DBId`) AS `DBTotal`,
    SUM(CASE WHEN UPPER(`DBRole`) = 'VIP' THEN 1 ELSE 0 END) AS `DBTotalVip`,
    SUM(CASE WHEN UPPER(`DBRole`) = 'MEMBER' THEN 1 ELSE 0 END) AS `DBTotalMember`
FROM `users`
WHERE 1
  AND `DBIsDeleted` = {DBIsDeleted};