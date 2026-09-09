UPDATE `rentals` 
SET 
	`DBComputerId` = '{DBComputerId}',
	`DBCustomerId` = '{DBCustomerId}',
	`DBDuration` = '{DBDuration}',
	`DBAmount` = '{DBAmount}'
WHERE `DBId` = '{DBId}'