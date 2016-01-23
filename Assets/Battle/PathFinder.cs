using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class TileWithPath
{
    public GameObject tile;
    public List<GameObject> path;
    public int requireActivityPoint;
    
    public TileWithPath(GameObject destTile, TileWithPath prevTileWithPath)
    {
        this.tile = destTile;

        List<GameObject> prevPath = prevTileWithPath.path;
        GameObject lastPrevTile = prevTileWithPath.tile;
        foreach (var prevTile in prevPath)
            this.path.Add(prevTile);
        this.path.Add(lastPrevTile);

        this.requireActivityPoint += (tile.GetComponent<Tile>().GetRequireAPAtTile() + prevPath.Count);
    }
}

public class PathFinder {

    Dictionary<Vector2, GameObject> tiles;
    Vector2 unitPosition;
    int remainAP;
    
    Dictionary<Vector2, TileWithPath> tilesWithPath;
    
    public PathFinder(GameObject unit)
    {
        this.unitPosition = unit.GetComponent<Unit>().GetPosition();
        this.remainAP = unit.GetComponent<Unit>().GetCurrentActivityPoint();
        
        this.tiles = GameObject.FindObjectOfType<TileManager>().GetAllTiles();
    }
    
    void CalculatePath()
    {
        
    }
}
