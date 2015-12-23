using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	TileManager tileManager;
	UnitManager unitManager;
	
	bool isInputPhase = false;
	bool isWaiting = false;
	bool isSelectedTileByUser = false;
	bool isWaitingUserInput = false;

	GameObject selectedUnit; // NOT used now.
	Queue<GameObject> readiedUnits = new Queue<GameObject>();

	int[] requireActionPoint = {2, 5, 9, 14, 20, 27, 35, 44};

	// Use this for initialization
	void Start () {
		tileManager = FindObjectOfType<TileManager>();
		unitManager = FindObjectOfType<UnitManager>();
		selectedUnit = null;
	}
	
	// Update is called once per frame
	void Update () {
		if (!isInputPhase && !isWaiting)
		{
			readiedUnits = unitManager.readiedUnits;
			if (readiedUnits.Count == 0)
			{
				// StartCoroutine("EndTurn");
				StartCoroutine(EndTurn());
			}
			else
			{
				StartCoroutine("OperateEachReadiedUnit");
			}
		}
	}
	
	public void OnMouseDownHandlerFromTile(Vector2 position)
	{
		if (isWaitingUserInput)
		{
			isSelectedTileByUser = true;
			Debug.Log("Clicked " + position + " tile");
		}
	}
		
	List<GameObject> AddNearbyTiles(List<GameObject> tileList, GameObject unit)
	{
		List<GameObject> newTileList = new List<GameObject>();
		foreach (var tile in tileList)
		{
			Vector2 position = tile.GetComponent<Tile>().GetTilePos();
			
			if (!newTileList.Contains(tile))
			{
				newTileList.Add(tile);
			}

			GameObject nearbyUpTile = tileManager.GetTile(position + Vector2.up);
			if (nearbyUpTile != null && !newTileList.Contains(nearbyUpTile))
			{
				newTileList.Add(nearbyUpTile);
			}
			GameObject nearbyDownTile = tileManager.GetTile(position + Vector2.down);
			if (nearbyDownTile != null && !newTileList.Contains(nearbyDownTile))
			{
				newTileList.Add(nearbyDownTile);
			}
			GameObject nearbyLeftTile = tileManager.GetTile(position + Vector2.left);
			if (nearbyLeftTile != null && !newTileList.Contains(nearbyLeftTile))
			{
				newTileList.Add(nearbyLeftTile);
			}
			GameObject nearbyRightTile = tileManager.GetTile(position + Vector2.right);
			if (nearbyRightTile != null && !newTileList.Contains(nearbyRightTile))
			{
				newTileList.Add(nearbyRightTile);
			}
		}
		
		List<GameObject> filteredNewTileList = new List<GameObject>();
		foreach (var tile in newTileList)
		{
			if (!(tile.GetComponent<Tile>().IsUnitOnTile() && 
			tile.GetComponent<Tile>().GetTilePos() != unit.GetComponent<Unit>().GetPosition()))
			{
				filteredNewTileList.Add(tile);
			}
		}
		
		// Debug.Log("tileList : " + tileList.Count + ", " + "newTileList : " + newTileList.Count);
		
		return filteredNewTileList;
	}
	
	List<GameObject> CheckMovableTiles(GameObject unit)
	{
		Vector2 pos = unit.GetComponent<Unit>().GetPosition();
		List<GameObject> nearbyTiles = new List<GameObject>();
		nearbyTiles.Add(tileManager.GetTile(pos));
		
		int currentAP = unit.GetComponent<Unit>().GetActionPoint();
		int totalRequireAP = 0;
		for (int i = 0; i < requireActionPoint.Length; i++)
		{
			if (currentAP < totalRequireAP + requireActionPoint[i])
			{
				break;
			}
			nearbyTiles = AddNearbyTiles(nearbyTiles, unit);
			totalRequireAP += requireActionPoint[i];
		}
		
		return nearbyTiles;
	}
	
	void MoveToTile(GameObject unit, GameObject destTile)
	{
		GameObject currentTile = tileManager.GetTile(unit.GetComponent<Unit>().GetPosition());
		currentTile.GetComponent<Tile>().SetUnitOnTile(null);
		unit.transform.position = destTile.transform.position + new Vector3(0, 0, -0.01f);
		unit.GetComponent<Unit>().SetPosition(destTile.GetComponent<Tile>().GetTilePos());
		destTile.GetComponent<Tile>().SetUnitOnTile(unit);
	}
	
	IEnumerator SelectMovingPoint (GameObject unit)
	{
		List<GameObject> nearbyTiles = CheckMovableTiles(unit);
		foreach (var tile in nearbyTiles)
		{
			tile.GetComponent<SpriteRenderer>().color -= new Color(0.3f, 0.3f, 0.3f, 0);
		}
		yield return new WaitForSeconds(3);		
		
		isWaitingUserInput = true;
		while (!isSelectedTileByUser)
		{
			yield return null;
		}
		isWaitingUserInput = false;
		
		GameObject destTile = nearbyTiles[Random.Range(0, nearbyTiles.Count)];
		Vector2 destTilePos = destTile.GetComponent<Tile>().GetTilePos();
		Vector2 currentTilePos = unit.GetComponent<Unit>().GetPosition();
		Vector2 distanceVector = destTilePos - currentTilePos;
		int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
		int totalUseActionPoint = 0;
		for (int i = 0; i < distance; i++)
		{
			totalUseActionPoint += requireActionPoint[i];
		}
		
		MoveToTile(unit, destTile);
		unit.GetComponent<Unit>().UseActionPoint(totalUseActionPoint);
		
		yield return new WaitForSeconds(1);
		
		foreach (var tile in nearbyTiles)
		{
			tile.GetComponent<SpriteRenderer>().color += new Color(0.3f, 0.3f, 0.3f, 0);
		}
		
		// Over AP is changed to HP.
		if (unit.GetComponent<Unit>().GetActionPoint() >= unitManager.maxActionPoint)
		{
			unit.GetComponent<Unit>().UseActionPoint(unitManager.maxActionPoint/2);
			Debug.Log("Rest and recover HP");
		}
		
		yield return new WaitForSeconds(1);
	}
	
	IEnumerator OperateEachReadiedUnit()
	{
		isInputPhase = true;
		while (readiedUnits.Count > 0)
		{
			GameObject unit = readiedUnits.Dequeue();
			Camera.main.transform.position = new Vector3 (unit.transform.position.x, unit.transform.position.y, -10);
			
			isSelectedTileByUser = false;
			yield return StartCoroutine(SelectMovingPoint(unit));
			isSelectedTileByUser = false;
		}
		
		readiedUnits = new Queue<GameObject>();
		isInputPhase = false;
		
		// yield return null;
	}
	
	IEnumerator EndTurn()
	{
		isWaiting = true;
		Debug.Log("Turn End.");
		unitManager.EndTurn();
		yield return new WaitForSeconds(0.5f);
		isWaiting = false;
	}
}
