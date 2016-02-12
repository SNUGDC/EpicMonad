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

public class GameManager : MonoBehaviour
{

    TileManager tileManager;
    UnitManager unitManager;

    GameObject commandUI;
    GameObject skillUI;
    GameObject skillCheckUI;
    GameObject destCheckUI;
    GameObject unitViewerUI;
    GameObject selectedUnitViewerUI;
    GameObject tileViewerUI;
    GameObject selectDirectionUI;

    CurrentState currentState = CurrentState.None;

    bool isSelectedTileByUser = false;
    bool isSelectedDirectionByUser = false;
    int indexOfSeletedSkillByUser = 0;
    bool isWaitingUserInput = false;

    bool rightClicked = false; // 우클릭 : 취소 
    bool leftClicked = false; // 좌클릭 : 유닛뷰어 고정

    ActionCommand command = ActionCommand.Waiting;
    SkillApplyCommand skillApplyCommand = SkillApplyCommand.Waiting;

    int moveCount;
    bool alreadyMoved;
    Vector2 selectedTilePosition;
    Direction selectedDirection;
    GameObject selectedUnitObject;
    List<GameObject> readiedUnits = new List<GameObject>();

    List<ChainInfo> chainList = new List<ChainInfo>();

    int currentPhase;

    // temp values.
    int chainDamageFactor = 1;
    int[] requireActionPoint = { 4, 10, 18, 28, 40, 54, 70, 88 };

    // Load from json.
    int partyLevel;

    class LevelData {
        public int level;
    }
    int GetLevelInfoFromJson()
    {
        string jsonString = File.ReadAllText(Application.dataPath + "/Data/PartyInfo.json");
        LevelData levelData = JsonMapper.ToObject<LevelData>(jsonString);
        
        return levelData.level;
    }
    
    public int GetPartyLevel()
    {
        return partyLevel;
    }
    
    public List<ChainInfo> GetChainList()
    {
        return chainList;
    }
    
    void Awake ()
    {
        tileManager = FindObjectOfType<TileManager>();
        unitManager = FindObjectOfType<UnitManager>();

        commandUI = GameObject.Find("CommandPanel");
        skillUI = GameObject.Find("SkillPanel");
        skillCheckUI = GameObject.Find("SkillCheckPanel");
        destCheckUI = GameObject.Find("DestCheckPanel");
        unitViewerUI = GameObject.Find("UnitViewerPanel");
        selectedUnitViewerUI = GameObject.Find("SelectedUnitViewerPanel");
        tileViewerUI = GameObject.Find("TileViewerPanel");
        selectDirectionUI = GameObject.Find("SelectDirectionUI");
    }

    // Use this for initialization
    void Start()
    {   
        partyLevel = GetLevelInfoFromJson();
        unitManager.SetStandardActionPoint(partyLevel);

        commandUI.SetActive(false);
        skillUI.SetActive(false);
        skillCheckUI.SetActive(false);
        destCheckUI.SetActive(false);
        unitViewerUI.SetActive(false);
        selectedUnitViewerUI.SetActive(false);
        tileViewerUI.SetActive(false);
        selectDirectionUI.SetActive(false);
       
        selectedUnitObject = null;

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
        return selectedUnitObject;
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
            readiedUnits = unitManager.GetUpdatedReadiedUnits();
            
            while (readiedUnits.Count != 0)
            {
                FindObjectOfType<APDisplayCurrentViewer>().UpdateAPDisplay(unitManager.GetAllUnits());
                FindObjectOfType<APDisplayNextViewer>().UpdateAPDisplay(unitManager.GetAllUnits());
                
                yield return StartCoroutine(ActionAtTurn(readiedUnits[0]));
                selectedUnitObject = null;

                readiedUnits = unitManager.GetUpdatedReadiedUnits();
                yield return null;
            }

            yield return StartCoroutine(EndPhaseOnGameManager());
            // readiedUnits = unitManager.readiedUnits;
        }
    }

    IEnumerator ActionAtTurn(GameObject unit)
    {
        FindObjectOfType<APDisplayCurrentViewer>().UpdateAPDisplay(unitManager.GetAllUnits());
        FindObjectOfType<APDisplayNextViewer>().UpdateAPDisplay(unitManager.GetAllUnits());
        
        Debug.Log(unit.GetComponent<Unit>().GetName() + "'s turn");
        selectedUnitObject = unit;
        moveCount = 0; // 누적 이동 수 
        alreadyMoved = false; // 연속 이동 불가를 위한 변수.  
        ChainList.RemoveChainsFromUnit(selectedUnitObject); // 턴이 돌아오면 자신이 건 체인 삭제.     
        currentState = CurrentState.FocusToUnit;
        
        selectedUnitViewerUI.SetActive(true);
        selectedUnitViewerUI.GetComponent<SelectedUnitViewer>().UpdateUnitViewer(selectedUnitObject);
        selectedUnitObject.GetComponent<Unit>().SetActive();
        
        yield return StartCoroutine(FocusToUnit());

        selectedUnitViewerUI.SetActive(false);
        selectedUnitObject.GetComponent<Unit>().SetInactive();
    }

    void CheckStandbyPossible()
    {
        bool isPossible = false;

        foreach (var unit in unitManager.GetAllUnits())
        {
            if ((unit != selectedUnitObject) &&
            (unit.GetComponent<Unit>().GetCurrentActivityPoint() > selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint()))
            {
                isPossible = true;
            }
        }

        GameObject.Find("StandbyButton").GetComponent<Button>().interactable = isPossible;
    }
    
    void CheckSkillPossible()
    {
        bool isPossible = false;

        isPossible = !(selectedUnitObject.GetComponent<Unit>().IsSilenced() ||
                     selectedUnitObject.GetComponent<Unit>().IsFainted());

        GameObject.Find("SkillButton").GetComponent<Button>().interactable = isPossible;   
    }
    
    void CheckMovePossible()
    {
        bool isPossible = false;

        isPossible = !(selectedUnitObject.GetComponent<Unit>().IsBound() ||
                     selectedUnitObject.GetComponent<Unit>().IsFainted() ||
                     alreadyMoved);

        GameObject.Find("MoveButton").GetComponent<Button>().interactable = isPossible;        
    }

    IEnumerator FocusToUnit()
    {
        while (currentState == CurrentState.FocusToUnit)
        {
            Camera.main.transform.position = new Vector3(selectedUnitObject.transform.position.x, selectedUnitObject.transform.position.y, -10);

            commandUI.SetActive(true);
            commandUI.transform.Find("NameText").GetComponent<Text>().text = selectedUnitObject.GetComponent<Unit>().GetName();
            CheckStandbyPossible();
            CheckMovePossible();
            CheckSkillPossible();

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

    IEnumerator RestAndRecover()
    {
        int usingActivityPointToRest = (int)(selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() * 0.9f);
        int recoverHealthDuringRest = (int)(selectedUnitObject.GetComponent<Unit>().GetMaxHealth() * (usingActivityPointToRest / 100f));
        selectedUnitObject.GetComponent<Unit>().UseActionPoint(usingActivityPointToRest);
        selectedUnitObject.GetComponent<Unit>().RecoverHealth(recoverHealthDuringRest);

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
        List<Skill> skillList = selectedUnitObject.GetComponent<Unit>().GetSkillList();
        for (int i = 0; i < skillList.Count; i++)
        {
            GameObject skillButton = GameObject.Find((i + 1).ToString() + "SkillButton"); //?? skillUI.transform.Find(i + "SkillButton")
            skillButton.transform.Find("NameText").GetComponent<Text>().text = skillList[i].GetName();
            skillButton.transform.Find("APText").GetComponent<Text>().text = skillList[i].GetRequireAP().ToString() + " AP";
            skillButton.transform.Find("CooldownText").GetComponent<Text>().text = "";
        }
    }

    void CheckUsableSkill()
    {
        List<Skill> skillList = selectedUnitObject.GetComponent<Unit>().GetSkillList();
        for (int i = 0; i < skillList.Count; i++)
        {
            GameObject.Find((i + 1).ToString() + "SkillButton").GetComponent<Button>().interactable = true;
            if (selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() < skillList[i].GetRequireAP())
            {
                GameObject.Find((i + 1).ToString() + "SkillButton").GetComponent<Button>().interactable = false;
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
                    isWaitingUserInput = false;
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
            Vector2 selectedUnitPos = selectedUnitObject.GetComponent<Unit>().GetPosition();

            List<GameObject> activeRange = new List<GameObject>();
            Skill selectedSkill = selectedUnitObject.GetComponent<Unit>().GetSkillList()[indexOfSeletedSkillByUser - 1];
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
                    isWaitingUserInput = false;
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

    void CheckChainPossible()
    {
        bool isPossible = false;
        
        // ap 조건으로 체크.
        int requireAP = selectedUnitObject.GetComponent<Unit>().GetSkillList()[indexOfSeletedSkillByUser - 1].GetRequireAP();        
        int remainAPAfterChain = selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() - requireAP;

        foreach (var unit in unitManager.GetAllUnits())
        {
            if ((unit != selectedUnitObject) &&
            (unit.GetComponent<Unit>().GetCurrentActivityPoint() > remainAPAfterChain))
            {
                isPossible = true;
            }
        }
        
        // 스킬 타입으로 체크. 공격스킬만 체인을 걸 수 있음.
        if (selectedUnitObject.GetComponent<Unit>().GetSkillList()[indexOfSeletedSkillByUser - 1].GetSkillApplyType() 
            != SkillApplyType.Damage)
        {
            isPossible = false;
        }

        GameObject.Find("ChainButton").GetComponent<Button>().interactable = isPossible;
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
            CheckChainPossible();

            Skill selectedSkill = selectedUnitObject.GetComponent<Unit>().GetSkillList()[indexOfSeletedSkillByUser - 1];
            int requireAP = selectedSkill.GetRequireAP();
            string newAPText = "소모 AP : " + requireAP + "\n" +
                               "잔여 AP : " + (selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() - requireAP);
            skillCheckUI.transform.Find("APText").GetComponent<Text>().text = newAPText;

            rightClicked = false;

            skillApplyCommand = SkillApplyCommand.Waiting;
            while (skillApplyCommand == SkillApplyCommand.Waiting)
            {
                if (rightClicked)
                {
                    rightClicked = false;

                    Camera.main.transform.position = new Vector3(selectedUnitObject.transform.position.x, selectedUnitObject.transform.position.y, -10);
                    skillCheckUI.SetActive(false);
                    currentState = CurrentState.SelectSkillApplyPoint;
                    yield break;
                }
                yield return null;
            }

            if (skillApplyCommand == SkillApplyCommand.Apply)
            {
                skillApplyCommand = SkillApplyCommand.Waiting;
                // 체인이 가능한 스킬일 경우. 체인 발동. 
                if (selectedSkill.GetSkillApplyType() == SkillApplyType.Damage)
                {
                    // 자기 자신을 체인 리스트에 추가.
                    ChainList.AddChains(selectedUnitObject, selectedTiles, indexOfSeletedSkillByUser);
                    // 체인 체크, 순서대로 공격.
                    List<ChainInfo> allVaildChainInfo = ChainList.GetAllChainInfoToTargetArea(selectedTiles);
                    int chainCombo = allVaildChainInfo.Count;
                    currentState = CurrentState.ApplySkill;
                    
                    foreach (var chainInfo in allVaildChainInfo)
                    {
                        GameObject focusedTile = chainInfo.GetTargetArea()[0];
                        Camera.main.transform.position = new Vector3(focusedTile.transform.position.x, focusedTile.transform.position.y, -10);
                        yield return StartCoroutine(ApplySkill(chainInfo.GetUnit(), chainCombo)); 
                    }
                    
                    Camera.main.transform.position = new Vector3(selectedUnitObject.transform.position.x, selectedUnitObject.transform.position.y, -10);
                    currentState = CurrentState.FocusToUnit;
                    yield return StartCoroutine(FocusToUnit());
                }
                // 체인이 불가능한 스킬일 경우, 그냥 발동.
                else
                {
                    currentState = CurrentState.ApplySkill;
                    yield return StartCoroutine(ApplySkill(selectedTiles));
                }
            }
            else if (skillApplyCommand == SkillApplyCommand.Chain)
            {
                skillApplyCommand = SkillApplyCommand.Waiting;
                currentState = CurrentState.ChainAndStandby;
                yield return StartCoroutine(ChainAndStandby(selectedTiles));
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

    // 체인 가능 스킬일 경우의 스킬 시전 코루틴. 공격 유닛을 받고, 유닛으로 체인 정보를 받아오는 방식으로 수정.
    IEnumerator ApplySkill(GameObject unitObject, int chainCombo)
    {
        // FIXME : 이펙트는 따로 들어가야 할 듯.

        ChainInfo chainInfoOfUnit = chainList.Find(k => k.GetUnit() == unitObject);
        Unit unitInChainInfo = chainInfoOfUnit.GetUnit().GetComponent<Unit>();
        Skill appliedSkill = unitInChainInfo.GetSkillList()[chainInfoOfUnit.GetSkillIndex() - 1];
        List<GameObject> selectedTiles = chainInfoOfUnit.GetTargetArea();

        foreach (var tile in selectedTiles)
        {
            GameObject target = tile.GetComponent<Tile>().GetUnitOnTile();
            if (target != null)
            {
                if (appliedSkill.GetSkillApplyType() == SkillApplyType.Damage)
                {
                    var damageAmount = (int)((chainCombo * chainDamageFactor) * unitInChainInfo.GetActualPower() * appliedSkill.GetPowerFactor());
                    var damageCoroutine = target.GetComponent<Unit>().Damaged(unitInChainInfo.GetUnitClass(), damageAmount);
                    yield return StartCoroutine(damageCoroutine);
                    Debug.Log("Apply " + damageAmount + " damage to " + target.GetComponent<Unit>().GetName() + "\n" + 
                              "ChainCombo : " + chainCombo);
                }
                else if (appliedSkill.GetSkillApplyType() == SkillApplyType.Heal)
                {
                    var recoverAmount = (int)(unitInChainInfo.GetActualPower() * appliedSkill.GetPowerFactor());
                    var recoverHealthCoroutine = target.GetComponent<Unit>().RecoverHealth(recoverAmount); 
                    yield return StartCoroutine(recoverHealthCoroutine);
                    Debug.Log("Apply " + recoverAmount + " heal to " + target.GetComponent<Unit>().GetName());
                }
                else
                {
                    Debug.Log("Apply additional effect to " + target.GetComponent<Unit>().name);
                }

                // FIXME : 버프, 디버프는 아직 미구현. 데미지/힐과 별개일 때도 있고 같이 들어갈 때도 있으므로 별도의 if문으로 구현할 것. 
            }
        }

        // 자신의 체인 정보 삭제.
        ChainList.RemoveChainsFromUnit(unitObject);
        tileManager.ChangeTilesFromSeletedColorToDefaultColor(selectedTiles);

        int requireAP = appliedSkill.GetRequireAP();
        unitInChainInfo.UseActionPoint(requireAP);
        indexOfSeletedSkillByUser = 0; // return to init value.

        yield return new WaitForSeconds(1f);

        alreadyMoved = false;
    }
    
    // 체인 불가능 스킬일 경우의 스킬 시전 코루틴. 스킬 적용 범위만 받는다.
    IEnumerator ApplySkill(List<GameObject> selectedTiles)
    {
        // FIXME : 이펙트는 따로 들어가야 할 듯.

        Unit selectedUnit = selectedUnitObject.GetComponent<Unit>();
        Skill appliedSkill = selectedUnit.GetSkillList()[indexOfSeletedSkillByUser - 1];
        
        foreach (var tile in selectedTiles)
        {
            GameObject target = tile.GetComponent<Tile>().GetUnitOnTile();
            if (target != null)
            {
                if (appliedSkill.GetSkillApplyType() == SkillApplyType.Damage)
                {
                    var damageAmount = (int)(selectedUnit.GetActualPower() * appliedSkill.GetPowerFactor());
                    var damageCoroutine = target.GetComponent<Unit>().Damaged(selectedUnit.GetUnitClass(), damageAmount);
                    yield return StartCoroutine(damageCoroutine);
                    Debug.Log("Apply " + damageAmount + " damage to " + target.GetComponent<Unit>().GetName());
                }
                else if (appliedSkill.GetSkillApplyType() == SkillApplyType.Heal)
                {
                    var recoverAmount = (int)(selectedUnit.GetActualPower() * appliedSkill.GetPowerFactor());
                    var recoverHealthCoroutine = target.GetComponent<Unit>().RecoverHealth(recoverAmount); 
                    yield return StartCoroutine(recoverHealthCoroutine);
                    Debug.Log("Apply " + recoverAmount + " heal to " + target.GetComponent<Unit>().GetName());
                }
                else
                {
                    Debug.Log("Apply additional effect to " + target.GetComponent<Unit>().GetName());
                }

                // FIXME : 버프, 디버프는 아직 미구현. 데미지/힐과 별개일 때도 있고 같이 들어갈 때도 있으므로 별도의 if문으로 구현할 것. 
            }
        }

        tileManager.ChangeTilesFromSeletedColorToDefaultColor(selectedTiles);

        int requireAP = appliedSkill.GetRequireAP();
        selectedUnit.UseActionPoint(requireAP);
        indexOfSeletedSkillByUser = 0; // return to init value.

        yield return new WaitForSeconds(1f);

        Camera.main.transform.position = new Vector3(selectedUnitObject.transform.position.x, selectedUnitObject.transform.position.y, -10);
        currentState = CurrentState.FocusToUnit;
        alreadyMoved = false;
        yield return StartCoroutine(FocusToUnit());
    }


    IEnumerator ChainAndStandby(List<GameObject> selectedTiles)
    {
        tileManager.ChangeTilesFromSeletedColorToDefaultColor(selectedTiles);
        
        // 스킬 시전에 필요한 ap만큼 선 차감. 
        int requireAP = selectedUnitObject.GetComponent<Unit>().GetSkillList()[indexOfSeletedSkillByUser - 1].GetRequireAP();
        selectedUnitObject.GetComponent<Unit>().UseActionPoint(requireAP);
        // 체인 목록에 추가. 
        ChainList.AddChains(selectedUnitObject, selectedTiles, indexOfSeletedSkillByUser);
        indexOfSeletedSkillByUser = 0; // return to init value.
        yield return new WaitForSeconds(0.5f);

        Camera.main.transform.position = new Vector3(selectedUnitObject.transform.position.x, selectedUnitObject.transform.position.y, -10);
        currentState = CurrentState.Standby;
        alreadyMoved = false;
        yield return StartCoroutine(Standby()); // 이후 대기.
    }

    public void CallbackRightClick()
    {
        rightClicked = true;
    }

    IEnumerator SelectMovingPoint()
    {
        while (currentState == CurrentState.SelectMovingPoint)
        {
            List<GameObject> nearbyTiles = CheckMovableTiles(selectedUnitObject);
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
                    isWaitingUserInput = false;
                    yield break;
                }
                yield return null;
            }
            isSelectedTileByUser = false;
            isWaitingUserInput = false;


            // FIXME : 어딘가로 옮겨야 할 텐데...        
            GameObject destTile = tileManager.GetTile(selectedTilePosition);
            Vector2 currentTilePos = selectedUnitObject.GetComponent<Unit>().GetPosition();
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
            selectDirectionUI.SetActive(true);
            destCheckUI.SetActive(true);
            string newAPText = "소모 AP : " + totalUseActionPoint + "\n" +
                               "잔여 AP : " + (selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() - totalUseActionPoint);
            destCheckUI.transform.Find("APText").GetComponent<Text>().text = newAPText;

            // 카메라를 옮기고
            Camera.main.transform.position = new Vector3(destTile.transform.position.x, destTile.transform.position.y, -10);
            // 클릭 대기
            rightClicked = false;

            isWaitingUserInput = true;
            isSelectedDirectionByUser = false;
            while (!isSelectedDirectionByUser)
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
                    Camera.main.transform.position = new Vector3(selectedUnitObject.transform.position.x,selectedUnitObject.transform.position.y, -10);
                    tileManager.ChangeTilesToSeletedColor(nearbyTiles, TileColor.blue);
                    selectDirectionUI.SetActive(false);
                    destCheckUI.SetActive(false);
                    currentState = CurrentState.SelectMovingPoint;
                    isWaitingUserInput = false;
                    yield break;
                }
                yield return null;
            }
            isSelectedDirectionByUser = false;
            isWaitingUserInput = false;

            // 방향을 클릭하면 그 자리로 이동. MoveToTile 호출 
            if (tileManager.GetTile(selectedTilePosition) == destTile)
            {
                tileManager.ChangeTilesFromSeletedColorToDefaultColor(destTileList);
                currentState = CurrentState.MoveToTile;
                destCheckUI.SetActive(false);
                yield return StartCoroutine(MoveToTile(destTile, selectedDirection, totalUseActionPoint));
            }
            // 아니면 아무 반응 없음.
            else
                yield return null;
        }
        yield return null;
    }
    
    public void CallbackDirection(String directionString)
    {
        if (!isWaitingUserInput)
            return;
        
        if (directionString == "LeftUp")
            selectedDirection = Direction.LeftUp;
        else if (directionString == "LeftDown")
            selectedDirection = Direction.LeftDown;
        else if (directionString == "RightUp")
            selectedDirection = Direction.RightUp;
        else if (directionString == "RightDown")
            selectedDirection = Direction.RightDown;
            
        isSelectedDirectionByUser = true;
        selectDirectionUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (leftClicked)
                leftClicked = false; // 유닛 고정이 되어있을 경우, 고정 해제가 우선으로 된다.
            else
                CallbackRightClick(); // 우클릭 취소를 받기 위한 핸들러. 
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            // 유닛 뷰어가 뜬 상태에서 좌클릭하면, 유닛 뷰어가 고정된다. 단, 유저 인풋을 기다릴 때는 불가능.
            if ((!isWaitingUserInput) && (unitViewerUI.activeInHierarchy))
                leftClicked = true;
        }
    }
    
    public bool IsLeftClicked()
    {
        return leftClicked;
    }

    public void OnMouseDownHandlerFromTile(Vector2 position)
    {
        if (isWaitingUserInput)
        {
            isSelectedTileByUser = true;
            selectedTilePosition = position;
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

    IEnumerator MoveToTile(GameObject destTile, Direction directionAtDest, int totalUseActionPoint)
    {
        GameObject currentTile = tileManager.GetTile(selectedUnitObject.GetComponent<Unit>().GetPosition());
        currentTile.GetComponent<Tile>().SetUnitOnTile(null);
        selectedUnitObject.transform.position = destTile.transform.position + new Vector3(0, 0, -0.01f);
        selectedUnitObject.GetComponent<Unit>().SetPosition(destTile.GetComponent<Tile>().GetTilePos());
        selectedUnitObject.GetComponent<Unit>().SetDirection(directionAtDest);
        destTile.GetComponent<Tile>().SetUnitOnTile(selectedUnitObject);

        selectedUnitObject.GetComponent<Unit>().UseActionPoint(totalUseActionPoint);

        currentState = CurrentState.FocusToUnit;
        alreadyMoved = true;
        yield return StartCoroutine(FocusToUnit());

        yield return null;
    }

    IEnumerator EndPhaseOnGameManager()
    {
        Debug.Log("Phase End.");

        currentPhase++;

        unitManager.EndPhase();
        yield return new WaitForSeconds(0.5f);
    }
}
