-- Update DBStatus = FALSE if the record already exists
UPDATE users
    SET DBIsDeleted = 'FALSE'
        WHERE DBEmail = '{DBEmail}';

-- Check if user exists
SELECT 
    (
        SELECT COUNT(*) 
        FROM users 
        WHERE DBEmail = '{DBEmail}'
    ) AS EmailExist;

-- Insert new record if no conflict exists
INSERT INTO users (
    DBEmail,
    DBPassword,
    DBName,
    DBRole,
    DBDateCreated,
    DBTimeCreated,
    DBIsDeleted
)
SELECT 
    '{DBEmail}',
    '{DBPassword}',
    '{DBName}',
    '{DBRole}',
    '{DBDateCreated}',
    '{DBTimeCreated}',
    '{DBIsDeleted}'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1
    FROM users
    WHERE DBEmail = '{DBEmail}'
);