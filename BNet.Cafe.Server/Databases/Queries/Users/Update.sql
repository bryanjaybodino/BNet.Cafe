UPDATE `users` 
SET `DBEmail` = '{DBEmail}',
    `DBPassword` = '{DBPassword}',
    `DBName` = '{DBName}',
    `DBRole` = '{DBRole}'
WHERE `DBId` = '{DBId}';