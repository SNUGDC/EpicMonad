using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Enums;
using LitJson;
using System.IO;
using System;

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

enum ActionCommand
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
    GameObject destCheckUI;
    GameObject unitViewerUI;
    GameObject tileViewerUI;
    
    CurrentState currentState = CurrentState.None;
    
	bool isSelectedTileByUser = false;
    int indexOfSeletedSkillByUser = 0;
	bool isWaitingUserInput = false;
    
    bool rightClicked = false;
    
    ActionCommand command = ActionCommand.Waiting;
    SkillApplyCommand skillApplyCommand = SkillApplyCommand.Waiting;
    
    int moveCount;
    Vector2 selectedTilePosition;
	GameObject selectedUnit;
	List<GameObject> readiedUnits = new List<GameObject>();
    
    List<ChainInfo> chainList = new List<ChainInfo>();

    int currentPhase;

	int[] requireActionPoint = {4, 10, 18, 28, 40, 54, 70, 88};

    // Load from json.
    int partyLevel;

    int GetLevelInfoFromJson()
    {
        string jsonString;
        JsonData jsonData;
        
        jsonString = File.ReadAllText(Application.dataPath + "/Data/PartyInfo.json");
        jsonData = JsonMapper.ToObject(jsonString);
        
        return Int32.Parse(jsonData["level"].ToString());
    }

	// Use this for initialization
	void Start () {        
        partyLevel = GetLevelInfoFromJson();
        
		tileManager = FindObjectOfType<TileManager>();
		unitManager = FindObjectOfType<UnitManager>();
		commandUI = GameObject.Find("CommandPanel");
        commandUI.SetActive(false);
        skillUI = GameObject.Find("SkillPanel");
        skillUI.SetActive(false);
        skillCheckUI = GameObject.Find("SkillCheckPanel");
        skillCheckUI.SetActive(false);
        destCheckUI = GameObject.Find("DestCheckPanel");
        destCheckUI.SetActive(false);
        unitViewerUI = GameObject.Find("UnitViewerPanel");
        unitViewerUI.SetActive(false);
        tileViewerUI = GameObject.Find("TileViewerPanel");
        tileViewerUI.SetActive(false);
        selectedUnit = null;
        
        currentPhase = 1;
        
        InitCameraPosition(new Vector2(5, 5)); // temp init position;
        
        StartCoroutine(InstantiateTurnManager());
	}
    
    public GameObject GetUnitViewerUI()
    {
        return unitViewerUI;
    }
    
    public GameObject GetTileViewerUI()
    {
        return tileViewerUI;
    }
    
    public int GetCurrentPhase()
    {
        return currentPhase;
    }
    
    public GameObject GetSelectedUnit()
    {
        return selectedUnit;
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
                selectedUnit = null;
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
        moveCount = 0; // 누적 이동 수        
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
            commandUI.transform.Find("NameText").GetComponent<Text>().text = selectedUnit.GetComponent<Unit>().name;
            CheckStandbyPossible();

            command = ActionCommand.Waiting;
            while (command == ActionCommand.Waiting)
            {
                yield return null;
            }
            
            if (command == ActionCommand.Move)
            {
                command = ActionCommand.Waiting;
                currentState = CurrentState.SelectMovingPoint;
                yield return StartCoroutine(SelectMovingPoint());
            }
            else if (command == ActionCommand.Attack)
            {
                command = ActionCommand.Waiting;
                currentState = CurrentState.SelectSkill;
                yield return StartCoroutine(SelectSkill());
            }
            else if (command == ActionCommand.Rest)
            {
                command = ActionCommand.Waiting;
                currentState = CurrentState.RestAndRecover;
                yield return StartCoroutine(RestAndRecover());
            }
            else if (command == ActionCommand.Standby)
            {
                command = ActionCommand.Waiting;
                currentState = CurrentState.Standby;
                yield return StartCoroutine(Standby());
            }
        }
        yield return null;
    }
    
    public void CallbackMoveCommand()
    {
        commandUI.SetActive(false);
        command = ActionCommand.Move;
    }
    
    public void CallbackAttackCommand()
    {
        commandUI.SetActive(false);
        command = ActionCommand.Attack;
    }
    
    public void CallbackRestCommand()
    {
        commandUI.SetActive(false);
        command = ActionCommand.Rest;
    }
    
    public void CallbackStandbyCommand()
    {
        commandUI.SetActive(false);
        command = ActionCommand.Standby;
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
    
    void UpdateSkillInfo()
    {
        List<Skill> skillList = selectedUnit.GetComponent<Unit>().GetSkillList();
        for (int i = 0; i < skillList.Count; i++)
        {
            GameObject skillButton = GameObject.Find((i+1).ToString() + "SkillButton"); //?? skillUI.transform.Find(i + "SkillButton")
            skillButton.transform.Find("NameText").GetComponent<Text>().text = skillList[i].GetName();
            skillButton.transform.Find("APText").GetComponent<Text>().text = skillList[i].GetRequireAP().ToString() + " AP";
            skillButton.transform.Find("CooldownText").GetComponent<Text>().text = "";
        }
    }
    
    void CheckUsableSkill()
    {
        List<Skill> skillList = selectedUnit.GetComponent<Unit>().GetSkillList();
        for (int i = 0; i < skillList.Count; i++)
        {
            GameObject.Find((i+1).ToString() + "SkillButton").GetComponent<Button>().interactable = true;
            if (selectedUnit.GetComponent<Unit>().GetCurrentActivityPoint() < skillList[i].GetRequireAP())
            {
                GameObject.Find((i+1).ToString() + "SkillButton").GetComponent<Button>().interactable = false;
            }
        }
    }
    
    IEnumerator SelectSkill()
    {
        while (currentState == CurrentState.SelectSkill)
        {
            skillUI.SetActive(true);
            UpdateSkillInfo();
            CheckUsableSkill();
            
            rightClicked = false;
            
            isWaitingUserInput = true;
            indexOfSeletedSkillByUser = 0;
            while (indexOfSeletedSkillByUser == 0)
            {
                if (rightClicked)
                {
                    rightClicked = false;
                
                    skillUI.SetActive(false);
                    currentState = CurrentState.FocusToUnit;
                    yield break;
                }
                yield return null;
            }
            isWaitingUserInput = false;
            
            skillUI.SetActive(false);
            
            yield return new WaitForSeconds(0.5f);
                    
            currentState = CurrentState.SelectSkillApplyPoint;        
            yield return StartCoroutine(SelectSkillApplyPoint());
        }
    }
    
    IEnumerator SelectSkillApplyPoint()
    {
        while (currentState == CurrentState.SelectSkillApplyPoint)
        {
            Vector2 selectedUnitPos = selectedUnit.GetComponent<Unit>().GetPosition();
            
            List<GameObject> activeRange = new List<GameObject>();
            Skill selectedSkill = selectedUnit.GetComponent<Unit>().GetSkillList()[indexOfSeletedSkillByUser-1];
            activeRange = tileManager.GetTilesInRange(selectedSkill.GetFirstRangeForm(),
                                                      selectedUnitPos,
                                                      selectedSkill.GetFirstMinReach(),
                                                      selectedSkill.GetFirstMaxReach(),
                                                      selectedSkill.GetIncludeMyself());
            tileManager.ChangeTilesToSeletedColor(activeRange, TileColor.red);
            
            rightClicked = false;
            
            isWaitingUserInput = true;
            isSelectedTileByUser = false;
            while (!isSelectedTileByUser)
            {
                if (rightClicked)
                {
                    rightClicked = false;
                    
                    tileManager.ChangeTilesFromSeletedColorToDefaultColor(activeRange);
                    currentState = CurrentState.SelectSkill;
                    yield break;
                }
                yield return null;
            }
            isSelectedTileByUser = false;
            isWaitingUserInput = false; 
            
            // 타겟팅 스킬을 타겟이 없는 장소에 지정했을 경우 적용되지 않도록 예외처리 필요
            
            tileManager.ChangeTilesFromSeletedColorToDefaultColor(activeRange);
            skillUI.SetActive(false);
            
            currentState = CurrentState.CheckApplyOrChain;
            yield return StartCoroutine(CheckApplyOrChain(selectedTilePosition));
        }
    }
    
    IEnumerator CheckApplyOrChain(Vector2 selectedTilePosition)
    {
        while (currentState == CurrentState.CheckApplyOrChain)
        {
            GameObject selectedTile = tileManager.GetTile(selectedTilePosition);
            Camera.main.transform.position = new Vector3(selectedTile.transform.position.x, selectedTile.transform.position.y, -10);
                    
            List<GameObject> selectedTiles = tileManager.GetTilesInRange(RangeForm.square, selectedTilePosition, 0, 0, true);
            tileManager.ChangeTilesToSeletedColor(selectedTiles, TileColor.red);
            skillCheckUI.SetActive(true);
         
            int requireAP = selectedUnit.GetComponent<Unit>().GetSkillList()[indexOfSeletedSkillByUser-1].GetRequireAP();
            string newAPText = "소모 AP : " + requireAP + "\n" +
                               "잔여 AP : " + (selectedUnit.GetComponent<Unit>().GetCurrentActivityPoint() - requireAP);
            skillCheckUI.transform.Find("APText").GetComponent<Text>().text = newAPText;
            
            rightClicked = false;
            
            skillApplyCommand = SkillApplyCommand.Waiting;
            while (skillApplyCommand == SkillApplyCommand.Waiting)
            {
                if (rightClicked)
                {
                    rightClicked = false;
                    
                    Camera.main.transform.position = new Vector3(selectedUnit.transform.position.x, selectedUnit.transform.position.y, -10);
                    skillCheckUI.SetActive(false);
                    currentState = CurrentState.SelectSkillApplyPoint;
                    yield break;
                }
                yield return null;
            }

            if (skillApplyCommand == SkillApplyCommand.Apply)
            {
                skillApplyCommand = SkillApplyCommand.Waiting;
                currentState = CurrentState.ApplySkill;
                yield return StartCoroutine(ApplySkill(selectedTiles));
            }
            else if (skillApplyCommand == SkillApplyCommand.Chain)
            {
                skillApplyCommand = SkillApplyCommand.Waiting;
                currentState = CurrentState.ChainAndStandby;
                yield return StartCoroutine(ChainAndStandby()); 
            }
            else
                yield return null;
        }
        yield return null;
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
        
        tileManager.ChangeTilesFromSeletedColorToDefaultColor(selectedTiles);
        
        int requireAP = selectedUnit.GetComponent<Unit>().GetSkillList()[indexOfSeletedSkillByUser-1].GetRequireAP();
        selectedUnit.GetComponent<Unit>().UseActionPoint(requireAP);  
        indexOfSeletedSkillByUser = 0; // return to init value.
        
        yield return new WaitForSeconds(0.5f);
        
        Camera.main.transform.position = new Vector3(selectedUnit.transform.position.x, selectedUnit.transform.position.y, -10);
        currentState = CurrentState.FocusToUnit;
        yield return StartCoroutine(FocusToUnit());
    }
    
    
    IEnumerator ChainAndStandby()
    {
        // 스킬 시전에 필요한 ap만큼 선 차감. 
        int requireAP = selectedUnit.GetComponent<Unit>().GetSkillList()[indexOfSeletedSkillByUser-1].GetRequireAP();
        selectedUnit.GetComponent<Unit>().UseActionPoint(requireAP);  
        indexOfSeletedSkillByUser = 0; // return to init value.
        
        // FIXME : 체인 목록에 추가. 
        
        yield return new WaitForSeconds(0.5f);
        
        Camera.main.transform.position = new Vector3(selectedUnit.transform.position.x, selectedUnit.transform.position.y, -10);
        currentState = CurrentState.Standby;         
        yield return StartCoroutine(Standby()); // 이후 대기.
    }
    
    public void CallbackRightClick()
    {
        rightClicked = true;
    }
    
    IEnumerator SelectMovingPoint ()
	{   
        while (currentState == CurrentState.SelectMovingPoint)
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
                    
                    tileManager.ChangeTilesFromSeletedColorToDefaultColor(nearbyTiles);

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
                totalUseActionPoint += requireActionPoint[i + moveCount];
            }
            
            moveCount += distance;
            
            tileManager.ChangeTilesFromSeletedColorToDefaultColor(nearbyTiles);
            currentState = CurrentState.CheckDestination;
            yield return StartCoroutine(CheckDestination(nearbyTiles, destTile, totalUseActionPoint, distance));
            // yield return StartCoroutine(MoveToTile(destTile, totalUseActionPoint));
            
            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
	}
    
    IEnumerator CheckDestination(List<GameObject> nearbyTiles, GameObject destTile, int totalUseActionPoint, int distance)
    {
        while (currentState == CurrentState.CheckDestination)
        {
            // 목표지점만 푸른색으로 표시
            List<GameObject> destTileList = new List<GameObject>();
            destTileList.Add(destTile);
            tileManager.ChangeTilesToSeletedColor(destTileList, TileColor.blue);
            // UI를 띄우고
            destCheckUI.SetActive(true);
            string newAPText = "소모 AP : " + totalUseActionPoint + "\n" +
                               "잔여 AP : " + (selectedUnit.GetComponent<Unit>().GetCurrentActivityPoint() - totalUseActionPoint);
            destCheckUI.transform.Find("APText").GetComponent<Text>().text = newAPText;
            
            // 카메라를 옮기고
            Camera.main.transform.position = new Vector3(destTile.transform.position.x, destTile.transform.position.y, -10);
            // 클릭 대기
            rightClicked = false;
            
            isWaitingUserInput = true;
            isSelectedTileByUser = false;
            while (!isSelectedTileByUser)
            {
                // 클릭 중 취소하면 돌아감
                // moveCount 되돌리기 
                // 카메라 유닛 위치로 원상복구
                // 이동가능 위치 다시 표시해주고 
                // UI 숨기고
                if (rightClicked)
                {
                    rightClicked = false;
                    moveCount -= distance;
                    Camera.main.transform.position = new Vector3(selectedUnit.transform.position.x, selectedUnit.transform.position.y, -10);
                    tileManager.ChangeTilesToSeletedColor(nearbyTiles, TileColor.blue);
                    destCheckUI.SetActive(false);
                    currentState = CurrentState.SelectMovingPoint;
                    yield break;
                }
                yield return null;
            }
            isSelectedTileByUser = false;
            isWaitingUserInput = false;
            
            // '일치하는 위치를' 클릭하면 그 자리로 이동. MoveToTile 호출 
            if (tileManager.GetTile(selectedTilePosition) == destTile)
            {
                tileManager.ChangeTilesFromSeletedColorToDefaultColor(destTileList);
                currentState = CurrentState.MoveToTile;
                destCheckUI.SetActive(false);
                yield return StartCoroutine(MoveToTile(destTile, totalUseActionPoint));
            }
            // 아니면 아무 반응 없음.
            else
                yield return null;
        }
        yield return null;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(1))
        {
            CallbackRightClick(); // 우클릭 취소를 받기 위한 핸들러. 
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
            // 누적 이동 수 계산을 위해 i 대신 i + moveCount 대입.
			if (currentActivityPoint < totalRequireActivityPoint + requireActionPoint[i + moveCount])
			{
				break;
			}
			nearbyTiles = AddNearbyTiles(nearbyTiles, unit);
			totalRequireActivityPoint += requireActionPoint[i + moveCount];
		}
		
		return nearbyTiles;
	}
	
	IEnumerator MoveToTile(GameObject destTile, int totalUseActionPoint)
	{
		GameObject currentTile = tileManager.GetTile(selectedUnit.GetComponent<Unit>().GetPosition());
		currentTile.GetComponent<Tile>().SetUnitOnTile(null);
		selectedUnit.transform.position = destTile.transform.position + new Vector3(0, 0, -0.01f);
		selectedUnit.GetComponent<Unit>().SetPosition(destTile.GetComponent<Tile>().GetTilePos());
		destTile.GetComponent<Tile>().SetUnitOnTile(selectedUnit);
        
        selectedUnit.GetComponent<Unit>().UseActionPoint(totalUseActionPoint);
            
        currentState = CurrentState.FocusToUnit;
        yield return StartCoroutine(FocusToUnit());
        
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
