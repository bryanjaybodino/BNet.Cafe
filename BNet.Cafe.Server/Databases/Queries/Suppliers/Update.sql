UPDATE `suppliers` 
SET 
    `DBSupplierName` = '{DBSupplierName}',
    `DBContactPerson` = '{DBContactPerson}',
    `DBPhone` = '{DBPhone}',
    `DBEmail` = '{DBEmail}',
    `DBAddress` = '{DBAddress}'
WHERE `DBId` = '{DBId}';