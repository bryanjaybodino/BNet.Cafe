-- Update DBStatus = FALSE if the record already exists
UPDATE computers
    SET DBIsDeleted = 'FALSE'
        WHERE DBComputerName = '{DBComputerName}';

-- Check if AssetType exists
SELECT 
    (
        SELECT COUNT(*) 
        FROM computers 
        WHERE DBComputerName = '{DBComputerName}'
    ) AS ComputerNameExist;

-- Insert new record if no conflict exists
INSERT INTO computers (
    DBComputerName,
    DBDateCreated,
    DBTimeCreated,
    DBIsDeleted
)
SELECT 
    '{DBComputerName}',
    '{DBDateCreated}',
    '{DBTimeCreated}',
    '{DBIsDeleted}'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1
    FROM computers
    WHERE DBComputerName = '{DBComputerName}'
);
