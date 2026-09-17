UPDATE `pricing_rates`
SET 
    `DBCustomerType` = '{DBCustomerType}',
    `DBMinutes` = '{DBMinutes}',
    `DBPrice` = '{DBPrice}'
WHERE `DBId` = '{DBId}';