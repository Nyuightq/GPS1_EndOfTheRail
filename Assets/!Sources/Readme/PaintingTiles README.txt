There are 2 tilemaps:
> EventTilemap (For Event Tiles, NonTraversableTile)
	> Tilemap (For Placing rails, Starting/Rest/Ending points)

When Drawing Event Tiles, NonTraversableTile, draw on EventTilemap.
	> If you draw NonTraversableTile on Tilemap, the player can overwrite the NonTraversableTile using the rails.

When Drawing rails, Starting/Rest/Ending points on Tilemap.