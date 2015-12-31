using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class TileManager : MonoBehaviour {

	public GameObject tilePrefab;
	
	Dictionary<Vector2, GameObject> tiles = new Dictionary<Vector2, GameObject>();

	int tileHeight = 100/100;
	int tileWidth = 200/100;

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
