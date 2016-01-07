using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class TileManager : MonoBehaviour {

	public GameObject tilePrefab;
	
	Dictionary<Vector2, GameObject> tiles = new Dictionary<Vector2, GameObject>();

	int tileHeight = 100/100;
	int tileWidth = 200/100;

    public Dictionary<Vector2, GameObject> GetAllTiles()
    {
        return tiles;
    }

	public GameObject GetTile(int x, int y)
	{
		Vector2 key = new Vector2 (x, y);
		if (tiles.ContainsKey(key))
			return tiles[key];
		else
			return null;
	}
	
	public GameObject GetTile(Vector2 position)
	{
		Vector2 key = position;
		if (tiles.ContainsKey(key))
			return tiles[key];
		else
			return null;
	}
	
	public Vector3 GetTilePos(Vector2 position)
	{
		GameObject tile = GetTile(position);
		return tile.transform.position;
	}

    public List<GameObject> GetTilesInSquareRange(Vector2 mid, int reach, bool includeUnitPos)
    {
        List<GameObject> tilesInRange = new List<GameObject>();
        tilesInRange.Add(GetTile(mid));
        for (int i = 0; i < reach; i++)
        {
            tilesInRange = AddNearbyTiles(tilesInRange);
        }
        
        if (!includeUnitPos)
        {
            tilesInRange.Remove(tilesInRange[0]);
        }
        
        return tilesInRange;
    }
    
    public void ChangeTilesToSeletedColor(List<GameObject> tiles)
    {
        foreach(var tile in tiles)
        {
            tile.GetComponent<SpriteRenderer>().color -= new Color(0, 0.5f, 0.5f, 0);
            tile.GetComponent<Tile>().SetPreSelected(true);
        }
    }
    
    public void ChangeTilesFromSeletedColorToDefaultColor(List<GameObject> tiles)
    {
        foreach(var tile in tiles)
        {
            tile.GetComponent<SpriteRenderer>().color += new Color(0, 0.5f, 0.5f, 0);
            tile.GetComponent<Tile>().SetPreSelected(false);
        }
    }
    
    List<GameObject> AddNearbyTiles(List<GameObject> tileList)
	{
		List<GameObject> newTileList = new List<GameObject>();
		foreach (var tile in tileList)
		{
			Vector2 position = tile.GetComponent<Tile>().GetTilePos();
			
			if (!newTileList.Contains(tile))
			{
				newTileList.Add(tile);
			}

			GameObject nearbyUpTile = GetTile(position + Vector2.up);
			if (nearbyUpTile != null && !newTileList.Contains(nearbyUpTile))
			{
				newTileList.Add(nearbyUpTile);
			}
			GameObject nearbyDownTile = GetTile(position + Vector2.down);
			if (nearbyDownTile != null && !newTileList.Contains(nearbyDownTile))
			{
				newTileList.Add(nearbyDownTile);
			}
			GameObject nearbyLeftTile = GetTile(position + Vector2.left);
			if (nearbyLeftTile != null && !newTileList.Contains(nearbyLeftTile))
			{
				newTileList.Add(nearbyLeftTile);
			}
			GameObject nearbyRightTile = GetTile(position + Vector2.right);
			if (nearbyRightTile != null && !newTileList.Contains(nearbyRightTile))
			{
				newTileList.Add(nearbyRightTile);
			}
		}
        
        return newTileList;
    }

	void GenerateTiles (int x, int y)
	{
		for (int i = 0; i < x; i++)
		{
			for (int j = 0; j < y; j++)
			{
				GameObject tile = Instantiate(tilePrefab, new Vector3(tileWidth * (j+i) * 0.5f, tileHeight * (j-i) * 0.5f, (j-i) * 0.1f), Quaternion.identity) as GameObject;
				tile.GetComponent<Tile>().SetTilePos(i, j);
                if (Random.Range(0, 3) > 0)
                    tile.GetComponent<Tile>().SetTileType(TileType.flatland);
                else
                    tile.GetComponent<Tile>().SetTileType(TileType.hill);
				tiles.Add(new Vector2(i, j), tile);
			}
		}
		
		Debug.Log("Generate tiles complete");
	}

	// Use this for initialization
	void Start () {
		// FIXME : num of tiles is temp constant.
		GenerateTiles(10, 10);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
