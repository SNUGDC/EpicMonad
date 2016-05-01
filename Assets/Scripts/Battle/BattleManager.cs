﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Enums;
using LitJson;
using System;
using Battle.Turn;

public class BattleManager : MonoBehaviour
{
	private BattleData battleData = new BattleData();

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
		return battleData.partyLevel;
	}

	public List<ChainInfo> GetChainList()
	{
		return battleData.chainList;
	}

	void Awake ()
	{
		battleData.tileManager = FindObjectOfType<TileManager>();
		battleData.unitManager = FindObjectOfType<UnitManager>();
		battleData.uiManager = FindObjectOfType<UIManager>();
		battleData.battleManager = this;
	}

	// Use this for initialization
	void Start()
	{
		battleData.partyLevel = GetLevelInfoFromJson();
		battleData.unitManager.SetStandardActionPoint(battleData.partyLevel);

		battleData.selectedUnitObject = null;

		battleData.currentPhase = 0;

		InitCameraPosition(); // temp init position;

		StartCoroutine(InstantiateTurnManager());
	}

	public int GetCurrentPhase()
	{
		return battleData.currentPhase;
	}

	public GameObject GetSelectedUnit()
	{
		return battleData.selectedUnitObject;
	}

	void InitCameraPosition()
	{
		Camera.main.transform.position = new Vector3(0, 0, -10);
	}

	IEnumerator InstantiateTurnManager()
	{
		while (true)
		{
			battleData.readiedUnits = battleData.unitManager.GetUpdatedReadiedUnits();

			while (battleData.readiedUnits.Count != 0)
			{
				FindObjectOfType<APDisplayCurrentViewer>().UpdateAPDisplay(battleData.unitManager.GetAllUnits());
				FindObjectOfType<APDisplayNextViewer>().UpdateAPDisplay(battleData.unitManager.GetAllUnits());

				yield return StartCoroutine(ActionAtTurn(battleData.readiedUnits[0]));
				battleData.selectedUnitObject = null;

				battleData.readiedUnits = battleData.unitManager.GetUpdatedReadiedUnits();
				yield return null;
			}

			yield return StartCoroutine(EndPhaseOnGameManager());
		}
	}

	IEnumerator ActionAtTurn(GameObject unit)
	{
		FindObjectOfType<APDisplayCurrentViewer>().UpdateAPDisplay(battleData.unitManager.GetAllUnits());
		FindObjectOfType<APDisplayNextViewer>().UpdateAPDisplay(battleData.unitManager.GetAllUnits());

		Debug.Log(unit.GetComponent<Unit>().GetName() + "'s turn");
		battleData.selectedUnitObject = unit;
		battleData.moveCount = 0; // 누적 이동 수
		battleData.alreadyMoved = false; // 연속 이동 불가를 위한 변수.
		ChainList.RemoveChainsFromUnit(battleData.selectedUnitObject); // 턴이 돌아오면 자신이 건 체인 삭제.
		battleData.currentState = CurrentState.FocusToUnit;

		battleData.uiManager.SetSelectedUnitViewerUI(battleData.selectedUnitObject);
		battleData.selectedUnitObject.GetComponent<Unit>().SetActive();

		yield return StartCoroutine(FocusToUnit(battleData));

		battleData.uiManager.DisableSelectedUnitViewerUI();
		battleData.selectedUnitObject.GetComponent<Unit>().SetInactive();
	}

	static void CheckStandbyPossible(BattleData battleData)
	{
		bool isPossible = false;

		foreach (var unit in battleData.unitManager.GetAllUnits())
		{
			if ((unit != battleData.selectedUnitObject) &&
			(unit.GetComponent<Unit>().GetCurrentActivityPoint() > battleData.selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint()))
			{
				isPossible = true;
			}
		}

		GameObject.Find("StandbyButton").GetComponent<Button>().interactable = isPossible;
	}

	static void CheckSkillPossible(BattleData battleData)
	{
		bool isPossible = false;

		isPossible = !(battleData.selectedUnitObject.GetComponent<Unit>().IsSilenced() ||
					 battleData.selectedUnitObject.GetComponent<Unit>().IsFainted());

		GameObject.Find("SkillButton").GetComponent<Button>().interactable = isPossible;
	}

	static void CheckMovePossible(BattleData battleData)
	{
		bool isPossible = false;

		isPossible = !(battleData.selectedUnitObject.GetComponent<Unit>().IsBound() ||
					 battleData.selectedUnitObject.GetComponent<Unit>().IsFainted() ||
					 battleData.alreadyMoved);

		GameObject.Find("MoveButton").GetComponent<Button>().interactable = isPossible;
	}

	public static IEnumerator FocusToUnit(BattleData battleData)
	{
		while (battleData.currentState == CurrentState.FocusToUnit)
		{
			Camera.main.transform.position = new Vector3(
				battleData.selectedUnitObject.transform.position.x,
				battleData.selectedUnitObject.transform.position.y,
				-10);

			battleData.uiManager.SetSelectedUnitViewerUI(battleData.selectedUnitObject);

			battleData.uiManager.SetCommandUIName(battleData.selectedUnitObject);
			CheckStandbyPossible(battleData);
			CheckMovePossible(battleData);
			CheckSkillPossible(battleData);

			BattleManager battleManager = battleData.battleManager;
			battleData.command = ActionCommand.Waiting;
			while (battleData.command == ActionCommand.Waiting)
			{
				yield return null;
			}

			if (battleData.command == ActionCommand.Move)
			{
				battleData.command = ActionCommand.Waiting;
				battleData.currentState = CurrentState.SelectMovingPoint;
				yield return battleManager.StartCoroutine(MoveStates.SelectMovingPointState(battleData));
			}
			else if (battleData.command == ActionCommand.Attack)
			{
				battleData.command = ActionCommand.Waiting;
				battleData.currentState = CurrentState.SelectSkill;
				yield return battleManager.StartCoroutine(SkillAndChainStates.SelectSkillState(battleData));
			}
			else if (battleData.command == ActionCommand.Rest)
			{
				battleData.command = ActionCommand.Waiting;
				battleData.currentState = CurrentState.RestAndRecover;
				yield return battleManager.StartCoroutine(RestAndRecover.Run(battleData));
			}
			else if (battleData.command == ActionCommand.Standby)
			{
				battleData.command = ActionCommand.Waiting;
				battleData.currentState = CurrentState.Standby;
				yield return battleManager.StartCoroutine(Standby());
			}
		}
		yield return null;
	}

	public void CallbackMoveCommand()
	{
		battleData.uiManager.DisableCommandUI();
		battleData.command = ActionCommand.Move;
	}

	public void CallbackAttackCommand()
	{
		battleData.uiManager.DisableCommandUI();
		battleData.command = ActionCommand.Attack;
	}

	public void CallbackRestCommand()
	{
		battleData.uiManager.DisableCommandUI();
		battleData.command = ActionCommand.Rest;
	}

	public void CallbackStandbyCommand()
	{
		battleData.uiManager.DisableCommandUI();
		battleData.command = ActionCommand.Standby;
	}

	public void CallbackCancel()
	{
		battleData.cancelClicked = true;
	}

	public static IEnumerator Standby()
	{
		yield return new WaitForSeconds(0.5f);
	}

	public void CallbackSkillIndex(int index)
	{
		battleData.indexOfSeletedSkillByUser = index;
		Debug.Log(index + "th skill is selected");
	}

	public void CallbackSkillUICancel()
	{
		battleData.cancelClicked = true;
	}

	public void CallbackApplyCommand()
	{
		battleData.uiManager.DisableSkillCheckUI();
		battleData.skillApplyCommand = SkillApplyCommand.Apply;
	}

	public void CallbackChainCommand()
	{
		battleData.uiManager.DisableSkillCheckUI();
		battleData.skillApplyCommand = SkillApplyCommand.Chain;
	}

	public void CallbackRightClick()
	{
		battleData.rightClicked = true;
	}

	public void CallbackDirection(String directionString)
	{
		if (!battleData.isWaitingUserInput)
			return;

		if (directionString == "LeftUp")
			battleData.selectedDirection = Direction.LeftUp;
		else if (directionString == "LeftDown")
			battleData.selectedDirection = Direction.LeftDown;
		else if (directionString == "RightUp")
			battleData.selectedDirection = Direction.RightUp;
		else if (directionString == "RightDown")
			battleData.selectedDirection = Direction.RightDown;

		battleData.isSelectedDirectionByUser = true;
		battleData.uiManager.DisableSelectDirectionUI();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButtonDown(1))
		{
			if (battleData.leftClicked)
				battleData.leftClicked = false; // 유닛 고정이 되어있을 경우, 고정 해제가 우선으로 된다.
			else
				CallbackRightClick(); // 우클릭 취소를 받기 위한 핸들러.
		}

		if (battleData.currentState != CurrentState.FocusToUnit)
		{
			battleData.leftClicked = false; // 행동을 선택하면 홀드가 자동으로 풀림.
		}

		if (Input.GetMouseButtonDown(0))
		{
			// 유닛 뷰어가 뜬 상태에서 좌클릭하면, 유닛 뷰어가 고정된다. 단, 행동 선택 상태(FocusToUnit)에서만 가능.
			if ((battleData.currentState == CurrentState.FocusToUnit) && (battleData.uiManager.IsUnitViewerShowing()))
				battleData.leftClicked = true;
		}
	}

	public bool IsLeftClicked()
	{
		return battleData.leftClicked;
	}

	public void OnMouseDownHandlerFromTile(Vector2 position)
	{
		if (battleData.isWaitingUserInput)
		{
			battleData.isSelectedTileByUser = true;
			battleData.selectedTilePosition = position;
		}
	}

	IEnumerator EndPhaseOnGameManager()
	{
		Debug.Log("Phase End.");

		battleData.currentPhase++;

		battleData.unitManager.EndPhase();
		yield return new WaitForSeconds(0.5f);
	}
}
