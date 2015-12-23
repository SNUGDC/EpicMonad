using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

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
