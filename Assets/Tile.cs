using UnityEngine;
using System.Collections;
using Enums;

public class Tile : MonoBehaviour {

    public TileType type;
	Vector2 position;
	GameObject unitOnTile = null;
	
	public void SetTilePos(int x, int y)
	{
		position = new Vector2(x, y);
	}
    	
	public Vector2 GetTilePos()
	{
		return position;
	}

    public void SetTileType(TileType type)
    {
        string imagePath = "TileImage/" + type.ToString();
        GetComponent<SpriteRenderer>().sprite = Resources.Load(imagePath, typeof(Sprite)) as Sprite;
        this.type = type;
    }

    public TileType GetTileType()
    {
        return type;
    }
    
    public int GetRequireAPAtTile()
    {
        return GetRequireAPFromTileType(type);
    }

	public bool IsUnitOnTile ()
	{
		return !(unitOnTile == null);
	}
	
	public void SetUnitOnTile(GameObject unit)
	{
		unitOnTile = unit;
	}
	
	public GameObject GetUnitOnTile ()
	{
		if (unitOnTile == null)
		{
			Debug.Log("(" + position.x + ", " + position.y + ") is empty.");
		}
		return unitOnTile;
	}

    int GetRequireAPFromTileType(TileType type)
    {
        if (type == TileType.flatland)
        {
            return 3;
        }
        else if (type == TileType.hill)
        {
            return 5;
        }
        else
        {
            Debug.Log("Invaild tiletype : " + type.ToString());
            return 1;
        }
    }

    void OnMouseEnter()
    {
        GetComponent<SpriteRenderer>().color -= new Color(0.3f, 0.3f, 0.3f, 0);
    }
    
    void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().color += new Color(0.3f, 0.3f, 0.3f, 0);
    }

	void OnMouseDown()
	{
		GameManager gameManager = FindObjectOfType<GameManager>();
		if (gameManager != null)
		{
			gameManager.OnMouseDownHandlerFromTile(position);
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
