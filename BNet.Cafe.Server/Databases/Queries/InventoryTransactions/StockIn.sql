UPDATE inventory_items 
SET DBQuantityInStock = DBQuantityInStock + {DBQuantity} 
WHERE DBId = '{DBItemId}' AND DBIsDeleted = FALSE;

INSERT INTO inventory_transactions 
(
	DBItemId,
	DBUserId,
	DBTransactionType,
	DBQuantity,
	DBCost,
	DBDateCreated,
	DBTimeCreated,
	DBIsDeleted
) 
VALUES 
(
	'{DBItemId}',
	'{DBUserId}',
	'{DBTransactionType}', 
	'{DBQuantity}',
	'{DBCost}',
	'{DBDateCreated}',
	'{DBTimeCreated}', 
	'{DBIsDeleted}'
);