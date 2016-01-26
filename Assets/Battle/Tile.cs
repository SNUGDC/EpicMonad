using UnityEngine;
using System.Collections;
using Enums;

public class Tile : MonoBehaviour {

    public TileForm form;
    public Element element;
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

    public void SetTileImage(TileForm form, Element element)
    {
        
    }

    public void SetTileForm(TileForm form)
    {
        string imagePath = "TileImage/" + form.ToString();
        GetComponent<SpriteRenderer>().sprite = Resources.Load(imagePath, typeof(Sprite)) as Sprite;
        this.form = form;
    }
    
    public void SetTileElement(Element element)
    {
        this.element = element;
    }

    public TileForm GetTileForm()
    {
        return form;
    }
    
    public Element GetTileElement()
    {
        return element;
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
    
    public string GetTileName()
    {
        if (form == TileForm.flatland)
            return "평지";
        else if (form == TileForm.hill)
            return "언덕";
        else
            return "";
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
        // GetComponent<SpriteRenderer>().color -= new Color(0.3f, 0.3f, 0.3f, 0);
        
        if (IsUnitOnTile())
        {
            FindObjectOfType<GameManager>().GetUnitViewerUI().SetActive(true);
            FindObjectOfType<UnitViewer>().UpdateUnitViewer(unitOnTile);
        }

        FindObjectOfType<GameManager>().GetTileViewerUI().SetActive(true);
        FindObjectOfType<TileViewer>().UpdateTileViewer(gameObject);
    }
    
    void OnMouseExit()
    {
        // GetComponent<SpriteRenderer>().color += new Color(0.3f, 0.3f, 0.3f, 0);
        
        FindObjectOfType<GameManager>().GetUnitViewerUI().SetActive(false);
        FindObjectOfType<GameManager>().GetTileViewerUI().SetActive(false);
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
