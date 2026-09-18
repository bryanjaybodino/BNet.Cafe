-- Restore record if the combination of Customer Type and Duration already exists
UPDATE pricing_rates
SET DBIsDeleted = 'FALSE'
WHERE DBCustomerType = '{DBCustomerType}'
  AND DBMinutes = '{DBMinutes}';

-- Check if the Pricing Rate record exists
SELECT 
    (
        SELECT COUNT(*) 
        FROM pricing_rates 
        WHERE DBCustomerType = '{DBCustomerType}'
          AND DBMinutes = '{DBMinutes}'
    ) AS PricingRateExist;

-- Insert new record if no conflict exists
INSERT INTO pricing_rates (
    DBCustomerType,
    DBMinutes,
    DBPrice,
    DBDateCreated,
    DBTimeCreated,
    DBIsDeleted
)
SELECT 
    '{DBCustomerType}',
    '{DBMinutes}',
    '{DBPrice}',
    '{DBDateCreated}',
    '{DBTimeCreated}',
    '{DBIsDeleted}'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1
    FROM pricing_rates
    WHERE DBCustomerType = '{DBCustomerType}'
      AND DBMinutes = '{DBMinutes}'
);