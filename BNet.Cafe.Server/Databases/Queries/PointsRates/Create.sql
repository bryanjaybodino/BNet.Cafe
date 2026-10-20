-- Restore record if the combination of Customer Type and Duration already exists
UPDATE points_rates
SET DBIsDeleted = 'FALSE'
WHERE DBCustomerType = '{DBCustomerType}'
  AND DBMinutes = '{DBMinutes}';

-- Check if the Customer Points Rate record exists
SELECT 
    (
        SELECT COUNT(*) 
        FROM points_rates 
        WHERE DBCustomerType = '{DBCustomerType}'
          AND DBMinutes = '{DBMinutes}'
    ) AS CustomerPointsExist;

-- Insert new record if no conflict exists
INSERT INTO points_rates (
    DBCustomerType,
    DBMinutes,
    DBPoints,
    DBDateCreated,
    DBTimeCreated,
    DBIsDeleted
)
SELECT 
    '{DBCustomerType}',
    '{DBMinutes}',
    '{DBPoints}',
    '{DBDateCreated}',
    '{DBTimeCreated}',
    '{DBIsDeleted}'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1
    FROM points_rates
    WHERE DBCustomerType = '{DBCustomerType}'
      AND DBMinutes = '{DBMinutes}'
);