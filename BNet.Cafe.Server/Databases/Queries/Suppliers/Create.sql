-- Update DBIsDeleted = FALSE if the supplier already exists
UPDATE suppliers
    SET DBIsDeleted = 'FALSE'
        WHERE DBSupplierName = '{DBSupplierName}';

-- Check if SupplierName exists
SELECT 
    (
        SELECT COUNT(*) 
        FROM suppliers 
        WHERE DBSupplierName = '{DBSupplierName}'
    ) AS SupplierNameExist;

-- Insert new record if no conflict exists
INSERT INTO suppliers (
    DBSupplierName,
    DBContactPerson,
    DBPhone,
    DBEmail,
    DBAddress,
    DBDateCreated,
    DBTimeCreated,
    DBIsDeleted
)
SELECT 
    '{DBSupplierName}',
    '{DBContactPerson}',
    '{DBPhone}',
    '{DBEmail}',
    '{DBAddress}',
    '{DBDateCreated}',
    '{DBTimeCreated}',
    '{DBIsDeleted}'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1
    FROM suppliers
    WHERE DBSupplierName = '{DBSupplierName}'
);