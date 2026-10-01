UPDATE inventory_items 
SET DBQuantityInStock = DBQuantityInStock + {DBQuantity} 
WHERE DBId = '{DBItemId}' AND DBIsDeleted = FALSE;

INSERT INTO inventory_transactions 
(DBItemId, DBUserId, DBTransactionType, DBQuantity, DBDateCreated, DBTimeCreated, DBIsDeleted) 
VALUES 
('{DBItemId}', '{DBUserId}', 'STOCK_IN', '{DBQuantity}', '{DBDateCreated}', '{DBTimeCreated}', FALSE);