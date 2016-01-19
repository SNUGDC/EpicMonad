using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Enums;

enum CurrentState
{
    None,
    FocusToUnit,
    SelectMovingPoint,
    CheckDestination,
    MoveToTile,
    SelectSkill,
    SelectSkillApplyPoint,
    CheckApplyOrChain,
    ApplySkill,
    ChainAndStandby,
    RestAndRecover,
    Standby
}

enum Command
{
    Waiting,
    Move,
    Attack,
    Rest,
    Standby,
    Cancel
}

enum SkillApplyCommand
{
    Waiting,
    Apply,
    Chain
}

public class GameManager : MonoBehaviour {

	TileManager tileManager;
	UnitManager unitManager;
    GameObject commandUI;
	GameObject skillUI;
    GameObject skillCheckUI;
    
    CurrentState currentState = CurrentState.None;
    
	bool isSelectedTileByUser = false;
    int indexOfSeletedSkillByUser = 0;
	bool isWaitingUserInput = false;
    
    bool rightClicked = false;
    
    Command command = Command.Waiting;
    SkillApplyCommand skillApplyCommand = SkillApplyCommand.Waiting;
    
    int moveCount;
    Vector2 selectedTilePosition;
	GameObject selectedUnit;
	Queue<GameObject> readiedUnits = new Queue<GameObject>();

    int currentPhase;

	int[] requireActionPoint = {4, 10, 18, 28, 40, 54, 70, 88};
	// int[] requireActionPoint = {2, 5, 9, 14, 20, 27, 35, 44};

	// Use this for initialization
	void Start () {
		tileManager = FindObjectOfType<TileManager>();
		unitManager = FindObjectOfType<UnitManager>();
		commandUI = GameObject.Find("CommandPanel");
        commandUI.SetActive(false);
        skillUI = GameObject.Find("SkillPanel");
        skillUI.SetActive(false);
        skillCheckUI = GameObject.Find("SkillCheckPanel");
        skillCheckUI.SetActive(false);
        selectedUnit = null;
        
        currentPhase = 1;
        
        InitCameraPosition(new Vector2(5, 5)); // temp init position;
        
        StartCoroutine(InstantiateTurnManager());
	}
    
    public int GetCurrentPhase()
    {
        return currentPhase;
    }
    
    void InitCameraPosition(Vector2 initTilePosition)
    {
        Vector2 tilePosition = tileManager.GetTilePos(initTilePosition);
        Camera.main.transform.position = new Vector3(tilePosition.x, tilePosition.y, -10);
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
            
            yield return StartCoroutine(EndPhase());
            readiedUnits = unitManager.readiedUnits;
        }
    }
    
    IEnumerator ActionAtTurn(GameObject unit)
    {
        Debug.Log(unit.GetComponent<Unit>().name + "'s turn");
        selectedUnit = unit;
        moveCount = 0;
        
        currentState = CurrentState.FocusToUnit;
        yield return StartCoroutine(FocusToUnit());
    }
    
    void CheckStandbyPossible()
    {
        bool isPossible = false;
        GameObject.Find("StandbyButton").GetComponent<Button>().interactable = isPossible;

        foreach (var unit in unitManager.GetAllUnits())
        {
            if ((unit != selectedUnit) && 
            (unit.GetComponent<Unit>().GetCurrentActivityPoint() >= selectedUnit.GetComponent<Unit>().GetCurrentActivityPoint()))
            {
                isPossible = true;
            }
        }
        
        GameObject.Find("StandbyButton").GetComponent<Button>().interactable = isPossible;
    }
    
    IEnumerator FocusToUnit()
    {
        while (currentState == CurrentState.FocusToUnit)
        {
            Camera.main.transform.position = new Vector3 (selectedUnit.transform.position.x, selectedUnit.transform.position.y, -10);

            commandUI.SetActive(true);
            GameObject.Find("NameText").GetComponent<Text>().text = selectedUnit.GetComponent<Unit>().name;
            CheckStandbyPossible();

            command = Command.Waiting;
            while (command == Command.Waiting)
            {
                yield return null;
            }
            
            if (command == Command.Move)
            {
                command = Command.Waiting;
                currentState = CurrentState.SelectMovingPoint;
                yield return StartCoroutine(SelectMovingPoint());
            }
            else if (command == Command.Attack)
            {
                command = Command.Waiting;
                currentState = CurrentState.SelectSkill;
                yield return StartCoroutine(SelectSkill());
            }
            else if (command == Command.Rest)
            {
                command = Command.Waiting;
                currentState = CurrentState.RestAndRecover;
                yield return StartCoroutine(RestAndRecover());
            }
            else if (command == Command.Standby)
            {
                command = Command.Waiting;
                currentState = CurrentState.Standby;
                yield return StartCoroutine(Standby());
            }
        }
        yield return null;
    }
    
    public void CallbackMoveCommand()
    {
        commandUI.SetActive(false);
        command = Command.Move;
    }
    
    public void CallbackAttackCommand()
    {
        commandUI.SetActive(false);
        command = Command.Attack;
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
        yield return new WaitForSeconds(1);
    }
    
    IEnumerator RestAndRecover ()
    {        
        int usingActivityPointToRest = (int)(selectedUnit.GetComponent<Unit>().GetCurrentActivityPoint() * 0.9f);
        int recoverHealthDuringRest = (int)(selectedUnit.GetComponent<Unit>().GetMaxHealth() * (usingActivityPointToRest/100f));
        selectedUnit.GetComponent<Unit>().UseActionPoint(usingActivityPointToRest);
        selectedUnit.GetComponent<Unit>().RecoverHealth(recoverHealthDuringRest);
        
        Debug.Log("Rest. Using " + usingActivityPointToRest + "AP and recover " + recoverHealthDuringRest + " HP");
        
        yield return new WaitForSeconds(1);
    }
    
    public void CallbackSkillIndex(int index)
    {
        indexOfSeletedSkillByUser = index;
        Debug.Log(index + "th skill is selected");
    }
    
    void CheckUsableSkill()
    {
        int[] requireAPOfSkills = selectedUnit.GetComponent<Unit>().requireAPOfSkills;
        for (int i = 0; i < requireAPOfSkills.Length; i++)
        {
            GameObject.Find((i+1).ToString() + "SkillButton").GetComponent<Button>().interactable = true;
            if (selectedUnit.GetComponent<Unit>().GetCurrentActivityPoint() < requireAPOfSkills[i])
            {
                GameObject.Find((i+1).ToString() + "SkillButton").GetComponent<Button>().interactable = false;
            }
        }
    }
    
    IEnumerator SelectSkill()
    {
        skillUI.SetActive(true);
        CheckUsableSkill();
        
        isWaitingUserInput = true;
        indexOfSeletedSkillByUser = 0;
        while (indexOfSeletedSkillByUser == 0)
        {
            yield return null;
        }
        isWaitingUserInput = false;
        
        skillUI.SetActive(false);
        
        yield return new WaitForSeconds(0.5f);
                
        yield return StartCoroutine(SelectSkillApplyPoint());
    }
    
    IEnumerator SelectSkillApplyPoint()
    {
        Vector2 selectedUnitPos = selectedUnit.GetComponent<Unit>().GetPosition();
        
        // temp values.
        List<GameObject> activeRange = new List<GameObject>();
        if (indexOfSeletedSkillByUser == 1)
            activeRange = tileManager.GetTilesInRange(RangeForm.square, selectedUnitPos, 4, false);
        else if (indexOfSeletedSkillByUser == 2)
            activeRange = tileManager.GetTilesInRange(RangeForm.square, selectedUnitPos, 4, true);
        else if (indexOfSeletedSkillByUser == 3)
            activeRange = tileManager.GetTilesInRange(RangeForm.square, selectedUnitPos, 2, false);    
        else if (indexOfSeletedSkillByUser == 4)
            activeRange = tileManager.GetTilesInRange(RangeForm.square, selectedUnitPos, 0, true);
        else
            activeRange = tileManager.GetTilesInRange(RangeForm.square, selectedUnitPos, 3, false);
        tileManager.ChangeTilesToSeletedColor(activeRange, TileColor.red);
        //
        
        isWaitingUserInput = true;
        isSelectedTileByUser = false;
		while (!isSelectedTileByUser)
		{
			yield return null;
		}
        isSelectedTileByUser = false;
		isWaitingUserInput = false; 
        
        // 타겟팅 스킬을 타겟이 없는 장소에 지정했을 경우 적용되지 않도록 예외처리 필요
        
        tileManager.ChangeTilesFromSeletedColorToDefaultColor(activeRange, TileColor.red);
        skillUI.SetActive(false);
        
        yield return StartCoroutine(CheckApplyOrChain(selectedTilePosition));
    }
    
    IEnumerator CheckApplyOrChain(Vector2 selectedTilePosition)
    {
        List<GameObject> selectedTiles = tileManager.GetTilesInRange(RangeForm.square, selectedTilePosition, 0, true);
        tileManager.ChangeTilesToSeletedColor(selectedTiles, TileColor.red);
        skillCheckUI.SetActive(true);
        
        skillApplyCommand = SkillApplyCommand.Waiting;
        while (skillApplyCommand == SkillApplyCommand.Waiting)
        {
            yield return null;
        }

        if (skillApplyCommand == SkillApplyCommand.Apply)
        {
            skillApplyCommand = SkillApplyCommand.Waiting;
            yield return StartCoroutine(ApplySkill(selectedTiles));
        }
        else if (skillApplyCommand == SkillApplyCommand.Chain)
        {
            skillApplyCommand = SkillApplyCommand.Waiting;
            yield return StartCoroutine(SelectMovingPoint());
        }
    }
    
    public void CallbackApplyCommand()
    {
        skillCheckUI.SetActive(false);
        skillApplyCommand = SkillApplyCommand.Apply;
    }
    
    public void CallbackChainCommand()
    {
        skillCheckUI.SetActive(false);
        skillApplyCommand = SkillApplyCommand.Chain;
    }
    
    IEnumerator ApplySkill(List<GameObject> selectedTiles)
    {
        foreach (var tile in selectedTiles)
        {
            GameObject target = tile.GetComponent<Tile>().GetUnitOnTile();
            if (target != null)
            {
                Debug.Log("Apply skill to " + target.GetComponent<Unit>().name);
            }
        }
        
        tileManager.ChangeTilesFromSeletedColorToDefaultColor(selectedTiles, TileColor.red);
        
        int requireAP = selectedUnit.GetComponent<Unit>().requireAPOfSkills[indexOfSeletedSkillByUser-1];
        selectedUnit.GetComponent<Unit>().UseActionPoint(requireAP);  
        indexOfSeletedSkillByUser = 0; // return to init value.
        
        yield return new WaitForSeconds(0.5f);
        
        yield return StartCoroutine(FocusToUnit());
    }
    
    IEnumerator ChainAndStandby()
    {
        yield return null;
    }
    
    public void CallbackRightClick()
    {
        rightClicked = true;
    }
    
    IEnumerator SelectMovingPoint ()
	{   
        List<GameObject> nearbyTiles = CheckMovableTiles(selectedUnit);
		tileManager.ChangeTilesToSeletedColor(nearbyTiles, TileColor.blue);
        
        rightClicked = false;
        
        isWaitingUserInput = true;
        isSelectedTileByUser = false;
		while (!isSelectedTileByUser)
		{
            //yield break 넣으면 코루틴 강제종료 
			if (rightClicked)
            {
                rightClicked = false;
                
                tileManager.ChangeTilesFromSeletedColorToDefaultColor(nearbyTiles, TileColor.blue);

                currentState = CurrentState.FocusToUnit;
                yield break;
            }
            yield return null;
		}
        isSelectedTileByUser = false;
		isWaitingUserInput = false;
		
       
        // FIXME : 어딘가로 옮겨야 할 텐데...        
		GameObject destTile = tileManager.GetTile(selectedTilePosition);
        Vector2 currentTilePos = selectedUnit.GetComponent<Unit>().GetPosition();
		Vector2 distanceVector = selectedTilePosition - currentTilePos;
		int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
		int totalUseActionPoint = 0;
		for (int i = 0; i < distance; i++)
		{
			totalUseActionPoint += requireActionPoint[i];
		}
		
        moveCount += distance;
		yield return StartCoroutine(MoveToTile(selectedUnit, destTile, totalUseActionPoint));
		
		yield return new WaitForSeconds(1);
		
        tileManager.ChangeTilesFromSeletedColorToDefaultColor(nearbyTiles, TileColor.blue);
		
		yield return new WaitForSeconds(0.5f);
        
        currentState = CurrentState.FocusToUnit;
        yield return StartCoroutine(FocusToUnit());
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(1))
        {
            CallbackRightClick();
        }
	}
	
	public void OnMouseDownHandlerFromTile(Vector2 position)
	{
		if (isWaitingUserInput)
		{
			isSelectedTileByUser = true;
            selectedTilePosition = position;
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
		
		int currentActivityPoint = unit.GetComponent<Unit>().GetCurrentActivityPoint();
		int totalRequireActivityPoint = 0;
		for (int i = 0; i < requireActionPoint.Length; i++)
		{
			if (currentActivityPoint < totalRequireActivityPoint + requireActionPoint[i])
			{
				break;
			}
			nearbyTiles = AddNearbyTiles(nearbyTiles, unit);
			totalRequireActivityPoint += requireActionPoint[i];
		}
		
		return nearbyTiles;
	}
	
	IEnumerator MoveToTile(GameObject unit, GameObject destTile, int totalUseActionPoint)
	{
		GameObject currentTile = tileManager.GetTile(unit.GetComponent<Unit>().GetPosition());
		currentTile.GetComponent<Tile>().SetUnitOnTile(null);
		unit.transform.position = destTile.transform.position + new Vector3(0, 0, -0.01f);
		unit.GetComponent<Unit>().SetPosition(destTile.GetComponent<Tile>().GetTilePos());
		destTile.GetComponent<Tile>().SetUnitOnTile(unit);
        
        unit.GetComponent<Unit>().UseActionPoint(totalUseActionPoint);
        
        yield return null;
	}
		
	IEnumerator EndPhase()
	{
		Debug.Log("Phase End.");
		
        currentPhase ++;
        
        unitManager.EndPhase();
		yield return new WaitForSeconds(0.5f);
	}
}
