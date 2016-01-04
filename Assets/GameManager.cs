using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

enum Command
{
    Waiting,
    Move,
    Attack,
    Rest,
    Standby,
    Cancel
}

public class GameManager : MonoBehaviour {

	TileManager tileManager;
	UnitManager unitManager;
    GameObject commandUI;
	
	bool isSelectedTileByUser = false;
	bool isWaitingUserInput = false;
    
    Command command = Command.Waiting;
    
    int moveCount;
    Vector2 destTilePosition;
	GameObject selectedUnit;
	Queue<GameObject> readiedUnits = new Queue<GameObject>();

	int[] requireActionPoint = {2, 5, 9, 14, 20, 27, 35, 44};

	// Use this for initialization
	void Start () {
		tileManager = FindObjectOfType<TileManager>();
		unitManager = FindObjectOfType<UnitManager>();
		commandUI = GameObject.Find("CommandPanel");
        commandUI.SetActive(false);
        selectedUnit = null;
        
        StartCoroutine(InstantiateTurnManager());
	}
    
    IEnumerator InstantiateTurnManager()
    {
        while (true)
        {
            foreach (var readiedUnit in readiedUnits)
            {
                yield return StartCoroutine(ActionAtTurn(readiedUnit));
            }
            readiedUnits.Clear();
            yield return null;
            
            yield return StartCoroutine(EndTurn());
            readiedUnits = unitManager.readiedUnits;
        }
    }
    
    IEnumerator ActionAtTurn(GameObject unit)
    {
        Debug.Log(unit.GetComponent<Unit>().name + "'s turn");
        selectedUnit = unit;
        moveCount = 0;
        yield return StartCoroutine(FocusToUnit());
    }
    
    void CheckStandbyPossible()
    {
        bool isPossible = false;
        GameObject.Find("StandbyButton").GetComponent<Button>().interactable = isPossible;

        foreach (var unit in unitManager.GetAllUnits())
        {
            if ((unit != selectedUnit) && 
            (unit.GetComponent<Unit>().GetCurrentActionPoint() >= selectedUnit.GetComponent<Unit>().GetCurrentActionPoint()))
            {
                isPossible = true;
            }
        }
        
        GameObject.Find("StandbyButton").GetComponent<Button>().interactable = isPossible;
    }
    
    IEnumerator FocusToUnit()
    {
        Camera.main.transform.position = new Vector3 (selectedUnit.transform.position.x, selectedUnit.transform.position.y, -10);

        commandUI.SetActive(true);
        
        CheckStandbyPossible();

        command = Command.Waiting;
        while (command == Command.Waiting)
        {
            yield return null;
        }
        
        if (command == Command.Move)
        {
            command = Command.Waiting;
            yield return StartCoroutine(SelectMovingPoint());
        }
        else if (command == Command.Rest)
        {
            command = Command.Waiting;
            yield return StartCoroutine(RestAndRecover());
        }
        else if (command == Command.Standby)
        {
            command = Command.Waiting;
            yield return StartCoroutine(Standby());
        }
    }
    
    public void CallbackMoveCommand()
    {
        commandUI.SetActive(false);
        command = Command.Move;
    }
    
    public void CallbackRestCommand()
    {
        commandUI.SetActive(false);
        command = Command.Rest;
    }
    
    public void CallbackStandbyCommand()
    {
        commandUI.SetActive(false);
        command = Command.Standby;
    }
    
    IEnumerator Standby()
    {
        if (selectedUnit.GetComponent<Unit>().GetCurrentActionPoint() >= unitManager.maxActionPoint)
        {
            yield return StartCoroutine(RestAndRecover());
            Debug.Log("Auto rest");
        }
        else
        {
            yield return new WaitForSeconds(1);
        }
    }
    
    IEnumerator RestAndRecover ()
    {        
        int usingAPToRest = (int)(selectedUnit.GetComponent<Unit>().GetCurrentActionPoint() * 0.9f);
        int recoverHPDuringRest = (int)(selectedUnit.GetComponent<Unit>().GetMaxHp() * (usingAPToRest/100f));
        selectedUnit.GetComponent<Unit>().UseActionPoint(usingAPToRest);
        selectedUnit.GetComponent<Unit>().RecoverHp(recoverHPDuringRest);
        
        Debug.Log("Rest. Using " + usingAPToRest + "AP and recover " + recoverHPDuringRest + " HP");
        
        yield return new WaitForSeconds(1);
    }
    
    IEnumerator SelectMovingPoint ()
	{   
        List<GameObject> nearbyTiles = CheckMovableTiles(selectedUnit);
		foreach (var tile in nearbyTiles)
		{
            tile.GetComponent<Tile>().SetPreSelected(true);
			tile.GetComponent<SpriteRenderer>().color -= new Color(0.3f, 0.3f, 0.3f, 0);
		}
        
        isWaitingUserInput = true;
        isSelectedTileByUser = false;
		while (!isSelectedTileByUser)
		{
			yield return null;
		}
        isSelectedTileByUser = false;
		isWaitingUserInput = false;
		
		// GameObject destTile = nearbyTiles[Random.Range(0, nearbyTiles.Count)];
		// Vector2 destTilePos = destTile.GetComponent<Tile>().GetTilePos();
        GameObject destTile = tileManager.GetTile(destTilePosition);
        Vector2 currentTilePos = selectedUnit.GetComponent<Unit>().GetPosition();
		Vector2 distanceVector = destTilePosition - currentTilePos;
		int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
		int totalUseActionPoint = 0;
		for (int i = 0; i < distance; i++)
		{
			totalUseActionPoint += requireActionPoint[i];
		}
		
        moveCount += distance;
		MoveToTile(selectedUnit, destTile);
		selectedUnit.GetComponent<Unit>().UseActionPoint(totalUseActionPoint);
		
		yield return new WaitForSeconds(1);
		
		foreach (var tile in nearbyTiles)
		{
            tile.GetComponent<Tile>().SetPreSelected(false);
			tile.GetComponent<SpriteRenderer>().color += new Color(0.3f, 0.3f, 0.3f, 0);
		}
		
		yield return new WaitForSeconds(1);
        
        yield return StartCoroutine(FocusToUnit());
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	public void OnMouseDownHandlerFromTile(Vector2 position)
	{
		if (isWaitingUserInput)
		{
			isSelectedTileByUser = true;
            destTilePosition = position;
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
		
		return filteredNewTileList;
	}
	
	List<GameObject> CheckMovableTiles(GameObject unit)
	{
		Vector2 pos = unit.GetComponent<Unit>().GetPosition();
		List<GameObject> nearbyTiles = new List<GameObject>();
		nearbyTiles.Add(tileManager.GetTile(pos));
		
		int currentAP = unit.GetComponent<Unit>().GetCurrentActionPoint();
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
		
	IEnumerator EndTurn()
	{
		// isWaiting = true;
		Debug.Log("Turn End.");
		unitManager.EndTurn();
		yield return new WaitForSeconds(0.5f);
		// isWaiting = false;
	}
}
