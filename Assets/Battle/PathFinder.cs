using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileWithPath
{
    public GameObject tile; // 도착지점 
    public List<GameObject> path; // '이전'까지의 경로
    public int requireActivityPoint; // '도착지점'까지 소모되는 ap
    
    public TileWithPath(GameObject startTile)
    {
        this.tile = startTile;
        this.path = new List<GameObject>();
        this.requireActivityPoint = 0;
    }
    
    public TileWithPath(GameObject destTile, TileWithPath prevTileWithPath)
    {
        this.tile = destTile; 
        this.path = new List<GameObject>();
        List<GameObject> prevPath = prevTileWithPath.path;
        GameObject lastPrevTile = prevTileWithPath.tile;
        foreach (var prevTile in prevPath)
            this.path.Add(prevTile);
        this.path.Add(lastPrevTile);

        this.requireActivityPoint = prevTileWithPath.requireActivityPoint + (tile.GetComponent<Tile>().GetRequireAPAtTile() + prevPath.Count);
    }
}

public class PathFinder {
    
    class TileTuple
    {
        public Vector2 tilePosition;
        public TileWithPath tileWithPath; 
        
        public TileTuple (Vector2 tilePosition, TileWithPath tileWithPath)
        {
            this.tilePosition = tilePosition;
            this.tileWithPath = tileWithPath;
        }
    }
    
    public static Dictionary<Vector2, TileWithPath> CalculatePath(GameObject unit)
    {
        Dictionary<Vector2, GameObject> tiles = GameObject.FindObjectOfType<TileManager>().GetAllTiles();
        Vector2 unitPosition = unit.GetComponent<Unit>().GetPosition();
        int remainAP = unit.GetComponent<Unit>().GetCurrentActivityPoint();

        Queue<TileTuple> tileQueue = new Queue<TileTuple>();
        
        Dictionary<Vector2, TileWithPath> tilesWithPath = new Dictionary<Vector2, TileWithPath>();
        TileWithPath startPoint = new TileWithPath(tiles[unitPosition]);
        tilesWithPath.Add(unitPosition, startPoint);
        // Queue에 넣음
        tileQueue.Enqueue(new TileTuple(unitPosition, startPoint));
        
        //// while loop
        while (tileQueue.Count > 0)
        {
            // Queue에 있는 모든 원소에 대해
            TileTuple newTileTuple = tileQueue.Dequeue();
            Vector2 newPosition = newTileTuple.tilePosition;
            TileWithPath newTileWithPath = newTileTuple.tileWithPath;
            // 전후좌우에 있는 타일을 탐색.
            SearchNearbyTile(tiles, tilesWithPath, tileQueue, remainAP, newPosition, newPosition + Vector2.up);
            SearchNearbyTile(tiles, tilesWithPath, tileQueue, remainAP, newPosition, newPosition + Vector2.down);
            SearchNearbyTile(tiles, tilesWithPath, tileQueue, remainAP, newPosition, newPosition + Vector2.left);
            SearchNearbyTile(tiles, tilesWithPath, tileQueue, remainAP, newPosition, newPosition + Vector2.right);
        }        
        //// queue가 비었으면 loop를 탈출.        
        
        return tilesWithPath;
    }
    
    static void SearchNearbyTile(Dictionary<Vector2, GameObject> tiles, Dictionary<Vector2, TileWithPath> tilesWithPath,
                                 Queue<TileTuple> tileQueue, int remainAP, Vector2 tilePosition, Vector2 nearbyTilePosition)
    {
        // if, 타일이 존재하지 않거나, 타일 위에 다른 유닛이 있거나, 타일까지 드는 ap가 remain ap보다 큰 경우 고려하지 않음.
        if (!tiles.ContainsKey(nearbyTilePosition)) return;
        
        GameObject nearbyTileObject = tiles[nearbyTilePosition];
        Tile nearbyTile = nearbyTileObject.GetComponent<Tile>();
        if (nearbyTile.IsUnitOnTile()) return;
        
        TileWithPath prevTileWithPath = tilesWithPath[tilePosition];
        TileWithPath nearbyTileWithPath = new TileWithPath(nearbyTileObject, prevTileWithPath);
        if (nearbyTileWithPath.requireActivityPoint > remainAP) return;
        
        // else, 
        //     if, 새로운 타일이거나, 기존보다 ap가 더 적게 드는 경로일 경우 업데이트하고 해당 타일을 queue에 넣음.
        if (!tilesWithPath.ContainsKey(nearbyTilePosition))
        {
            tilesWithPath.Add(nearbyTilePosition, nearbyTileWithPath);
            tileQueue.Enqueue(new TileTuple(nearbyTilePosition, nearbyTileWithPath));
            return;
        }
        
        TileWithPath existingNearbyTileWithPath = tilesWithPath[nearbyTilePosition];
        if (existingNearbyTileWithPath.requireActivityPoint > nearbyTileWithPath.requireActivityPoint)
        {
            tilesWithPath.Remove(nearbyTilePosition);
            tilesWithPath.Add(nearbyTilePosition, nearbyTileWithPath);
            tileQueue.Enqueue(new TileTuple(nearbyTilePosition, nearbyTileWithPath));
            return;
        }
    }
}
