UPDATE `rentals` 
SET 
	`DBComputerId` = '{DBComputerId}',
	`DBUserId` = '{DBUserId}',
	`DBDuration` = '{DBDuration}',
	`DBAmount` = '{DBAmount}'
WHERE `DBId` = '{DBId}'