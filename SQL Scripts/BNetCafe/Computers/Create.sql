-- Update DBStatus = FALSE if the record already exists
UPDATE computers
    SET DBIsDeleted = 'FALSE'
        WHERE DBComputerName = 'PC 1';

-- Check if AssetType exists
SELECT 
    (
        SELECT COUNT(*) 
        FROM computers 
        WHERE DBComputerName = 'PC 1'
    ) AS ComputerNameExist;

-- Insert new record if no conflict exists
INSERT INTO computers (
    DBComputerName,
    DBDateCreated,
    DBTimeCreated,
    DBIsDeleted
)
SELECT 
    'PC 1',
    '2026-09-06',
    '05:46:32',
    'FALSE'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1
    FROM computers
    WHERE DBComputerName = 'PC 1'
);
