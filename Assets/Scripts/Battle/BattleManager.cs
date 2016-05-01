using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Enums;
using LitJson;
using System;
using Battle.Turn;

public enum CurrentState
{
	None, FocusToUnit, SelectMovingPoint, CheckDestination,
	MoveToTile, SelectSkill, SelectSkillApplyPoint, SelectSkillApplyDirection, CheckApplyOrChain,
	ApplySkill, ChainAndStandby, RestAndRecover, Standby
}

public enum ActionCommand
{
	Waiting, Move, Attack, Rest, Standby, Cancel
}

public enum SkillApplyCommand
{
	Waiting, Apply, Chain
}

public class BattleManager : MonoBehaviour
{
	public TileManager tileManager;
	public UnitManager unitManager;
	public UIManager uiManager;

	public CurrentState currentState = CurrentState.None;

	public bool isSelectedTileByUser = false;
	public bool isSelectedDirectionByUser = false;
	public int indexOfSeletedSkillByUser = 0;
	public bool isWaitingUserInput = false;

	public bool rightClicked = false; // 우클릭 : 취소
	public bool leftClicked = false; // 좌클릭 : 유닛뷰어 고정

	public bool cancelClicked = false;

	public ActionCommand command = ActionCommand.Waiting;
	public SkillApplyCommand skillApplyCommand = SkillApplyCommand.Waiting;

	public int moveCount;
	public bool alreadyMoved;
	public Vector2 selectedTilePosition;
	public Direction selectedDirection;
	public GameObject selectedUnitObject;
	public List<GameObject> readiedUnits = new List<GameObject>();

	public List<ChainInfo> chainList = new List<ChainInfo>();

	public int currentPhase;

	// temp values.
	public int chainDamageFactor = 1;

	// Load from json.
	public int partyLevel;

	public class LevelData {
		public int level;
	}

	public int GetLevelInfoFromJson()
	{
		TextAsset jsonTextAsset = Resources.Load("Data/PartyData") as TextAsset;
		string jsonString = jsonTextAsset.text;
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
		uiManager = FindObjectOfType<UIManager>();
	}

	// Use this for initialization
	void Start()
	{
		partyLevel = GetLevelInfoFromJson();
		unitManager.SetStandardActionPoint(partyLevel);

		selectedUnitObject = null;

		currentPhase = 0;

		InitCameraPosition(); // temp init position;

		StartCoroutine(InstantiateTurnManager());
	}

	public int GetCurrentPhase()
	{
		return currentPhase;
	}

	public GameObject GetSelectedUnit()
	{
		return selectedUnitObject;
	}

	void InitCameraPosition()
	{
		Camera.main.transform.position = new Vector3(0, 0, -10);
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

		uiManager.SetSelectedUnitViewerUI(selectedUnitObject);
		selectedUnitObject.GetComponent<Unit>().SetActive();

		yield return StartCoroutine(FocusToUnit());

		uiManager.DisableSelectedUnitViewerUI();
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

	public IEnumerator FocusToUnit()
	{
		while (currentState == CurrentState.FocusToUnit)
		{
			Camera.main.transform.position = new Vector3(selectedUnitObject.transform.position.x, selectedUnitObject.transform.position.y, -10);

			uiManager.SetSelectedUnitViewerUI(selectedUnitObject);

			uiManager.SetCommandUIName(selectedUnitObject);
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
				yield return StartCoroutine(MoveStates.SelectMovingPointState(this));
			}
			else if (command == ActionCommand.Attack)
			{
				command = ActionCommand.Waiting;
				currentState = CurrentState.SelectSkill;
				yield return StartCoroutine(SkillAndChainStates.SelectSkillState(this));
			}
			else if (command == ActionCommand.Rest)
			{
				command = ActionCommand.Waiting;
				currentState = CurrentState.RestAndRecover;
				yield return StartCoroutine(RestAndRecover.Run(this));
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
		uiManager.DisableCommandUI();
		command = ActionCommand.Move;
	}

	public void CallbackAttackCommand()
	{
		uiManager.DisableCommandUI();
		command = ActionCommand.Attack;
	}

	public void CallbackRestCommand()
	{
		uiManager.DisableCommandUI();
		command = ActionCommand.Rest;
	}

	public void CallbackStandbyCommand()
	{
		uiManager.DisableCommandUI();
		command = ActionCommand.Standby;
	}

	public void CallbackCancel()
	{
		cancelClicked = true;
	}

	public IEnumerator Standby()
	{
		yield return new WaitForSeconds(0.5f);
	}

	public void CallbackSkillIndex(int index)
	{
		indexOfSeletedSkillByUser = index;
		Debug.Log(index + "th skill is selected");
	}

	public void CallbackSkillUICancel()
	{
		cancelClicked = true;
	}

	public void CallbackApplyCommand()
	{
		uiManager.DisableSkillCheckUI();
		skillApplyCommand = SkillApplyCommand.Apply;
	}

	public void CallbackChainCommand()
	{
		uiManager.DisableSkillCheckUI();
		skillApplyCommand = SkillApplyCommand.Chain;
	}

	public IEnumerator ApplySkillEffect(Skill appliedSkill, GameObject unitObject, List<GameObject> selectedTiles)
	{
		string effectName = appliedSkill.GetEffectName();
		EffectVisualType effectVisualType = appliedSkill.GetEffectVisualType();
		EffectMoveType effectMoveType = appliedSkill.GetEffectMoveType();

		if ((effectVisualType == EffectVisualType.Area) && (effectMoveType == EffectMoveType.Move))
		{
			// 투사체, 범위형 이펙트.
			Vector3 startPos = unitObject.transform.position;
			Vector3 endPos = new Vector3(0, 0, 0);
			foreach (var tile in selectedTiles)
			{
				endPos += tile.transform.position;
			}
			endPos = endPos / (float)selectedTiles.Count;

			GameObject particle = Instantiate(Resources.Load("Particle/" + effectName)) as GameObject;
			particle.transform.position = startPos - new Vector3(0, 0, 0.01f);
			yield return new WaitForSeconds(0.2f);
			iTween.MoveTo(particle, endPos - new Vector3(0, 0, 0.01f) - new Vector3(0, 0, 5f), 0.5f); // 타일 축 -> 유닛 축으로 옮기기 위해 z축으로 5만큼 앞으로 빼준다.
			yield return new WaitForSeconds(0.3f);
			Destroy(particle, 0.5f);
			yield return null;
		}
		else if ((effectVisualType == EffectVisualType.Area) && (effectMoveType == EffectMoveType.NonMove))
		{
			// 고정형, 범위형 이펙트.
			Vector3 targetPos = new Vector3(0, 0, 0);
			foreach (var tile in selectedTiles)
			{
				targetPos += tile.transform.position;
			}
			targetPos = targetPos / (float)selectedTiles.Count;
			targetPos = targetPos - new Vector3(0, 0, 5f); // 타일 축 -> 유닛 축으로 옮기기 위해 z축으로 5만큼 앞으로 빼준다.

			GameObject particle = Instantiate(Resources.Load("Particle/" + effectName)) as GameObject;
			particle.transform.position = targetPos - new Vector3(0, 0, 0.01f);
			yield return new WaitForSeconds(0.5f);
			Destroy(particle, 0.5f);
			yield return null;
		}
		else if ((effectVisualType == EffectVisualType.Individual) && (effectMoveType == EffectMoveType.NonMove))
		{
			// 고정형, 개별 대상 이펙트.
			List<Vector3> targetPosList = new List<Vector3>();
			foreach (var tileObject in selectedTiles)
			{
				Tile tile = tileObject.GetComponent<Tile>();
				if (tile.IsUnitOnTile())
				{
					targetPosList.Add(tile.GetUnitOnTile().transform.position);
				}
			}

			foreach (var targetPos in targetPosList)
			{
				GameObject particle = Instantiate(Resources.Load("Particle/" + effectName)) as GameObject;
				particle.transform.position = targetPos - new Vector3(0, 0, 0.01f);
				Destroy(particle, 0.5f + 0.3f); // 아랫줄에서의 지연시간을 고려한 값이어야 함.
			}
			if (targetPosList.Count == 0) // 대상이 없을 경우. 일단 가운데 이펙트를 띄운다.
			{
				Vector3 midPos = new Vector3(0, 0, 0);
				foreach (var tile in selectedTiles)
				{
					midPos += tile.transform.position;
				}
				midPos = midPos / (float)selectedTiles.Count;

				GameObject particle = Instantiate(Resources.Load("Particle/" + effectName)) as GameObject;
				particle.transform.position = midPos - new Vector3(0, 0, 0.01f);
				Destroy(particle, 0.5f + 0.3f); // 아랫줄에서의 지연시간을 고려한 값이어야 함.
			}

			yield return new WaitForSeconds(0.5f);
		}
	}

	public void CallbackRightClick()
	{
		rightClicked = true;
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
		uiManager.DisableSelectDirectionUI();
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

		if (currentState != CurrentState.FocusToUnit)
		{
			leftClicked = false; // 행동을 선택하면 홀드가 자동으로 풀림.
		}

		if (Input.GetMouseButtonDown(0))
		{
			// 유닛 뷰어가 뜬 상태에서 좌클릭하면, 유닛 뷰어가 고정된다. 단, 행동 선택 상태(FocusToUnit)에서만 가능.
			if ((currentState == CurrentState.FocusToUnit) && (uiManager.IsUnitViewerShowing()))
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

	IEnumerator EndPhaseOnGameManager()
	{
		Debug.Log("Phase End.");

		currentPhase++;

		unitManager.EndPhase();
		yield return new WaitForSeconds(0.5f);
	}
}
