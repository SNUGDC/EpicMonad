using UnityEngine;
using System.Collections;
using Enums;

public class Tile : MonoBehaviour {

    public TileForm form;
	Vector2 position;
	GameObject unitOnTile = null;

    bool isPreSeleted = false;
	
    public void SetPreSelected(bool input)
    {
        isPreSeleted = input;
    }
    
	public void SetTilePos(int x, int y)
	{
		position = new Vector2(x, y);
	}
    	
	public Vector2 GetTilePos()
	{
		return position;
	}

    public void SetTileImage(TileForm form, TileElement element)
    {
        
    }

    public void SetTileForm(TileForm form)
    {
        string imagePath = "TileImage/" + form.ToString();
        GetComponent<SpriteRenderer>().sprite = Resources.Load(imagePath, typeof(Sprite)) as Sprite;
        this.form = form;
    }

    public TileForm GetTileForm()
    {
        return form;
    }
    
    public int GetRequireAPAtTile()
    {
        return GetRequireAPFromTileType(form);
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
		// if (unitOnTile == null)
		// {
		// 	Debug.Log("(" + position.x + ", " + position.y + ") is empty.");
		// }
		return unitOnTile;
	}

    int GetRequireAPFromTileType(TileForm type)
    {
        if (type == TileForm.flatland)
        {
            return 3;
        }
        else if (type == TileForm.hill)
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
		if ((isPreSeleted) && (gameManager != null))
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
