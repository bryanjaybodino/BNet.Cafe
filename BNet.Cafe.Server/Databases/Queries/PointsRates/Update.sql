UPDATE `points_rates`
SET 
    `DBCustomerType` = '{DBCustomerType}',
    `DBMinutes` = '{DBMinutes}',
    `DBPoints` = '{DBPoints}'
WHERE `DBId` = '{DBId}';