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
	None, FocusToUnit, SelectMovingPoint, CheckDestination,
	MoveToTile, SelectSkill, SelectSkillApplyPoint, CheckApplyOrChain,
	ApplySkill, ChainAndStandby, RestAndRecover, Standby
}

enum ActionCommand
{
	Waiting, Move, Attack, Rest, Standby, Cancel
}

enum SkillApplyCommand
{
	Waiting, Apply, Chain
}

public class GameManager : MonoBehaviour
{
	TileManager tileManager;
	UnitManager unitManager;
	UIManager uiManager;

	CurrentState currentState = CurrentState.None;

	bool isSelectedTileByUser = false;
	bool isSelectedDirectionByUser = false;
	int indexOfSeletedSkillByUser = 0;
	bool isWaitingUserInput = false;

	bool rightClicked = false; // 우클릭 : 취소 
	bool leftClicked = false; // 좌클릭 : 유닛뷰어 고정

	bool cancelClicked = false;

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

	// Load from json.
	int partyLevel;

	class LevelData {
		public int level;
	}

	int GetLevelInfoFromJson()
	{
		TextAsset jsonTextAsset = Resources.Load("Data/PartyInfo") as TextAsset;
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

		currentPhase = 1;

		InitCameraPosition(new Vector2(15, 15)); // temp init position;

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

	IEnumerator FocusToUnit()
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

	IEnumerator Standby()
	{
		yield return new WaitForSeconds(0.5f);
	}

	IEnumerator RestAndRecover()
	{
		int usingActivityPointToRest = (int)(selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() * 0.9f);
		int recoverHealthDuringRest = (int)(selectedUnitObject.GetComponent<Unit>().GetMaxHealth() * (usingActivityPointToRest / 100f));
		selectedUnitObject.GetComponent<Unit>().UseActionPoint(usingActivityPointToRest);
		IEnumerator recoverHealthCoroutine = selectedUnitObject.GetComponent<Unit>().RecoverHealth(recoverHealthDuringRest);
		yield return StartCoroutine(recoverHealthCoroutine);

		Debug.Log("Rest. Using " + usingActivityPointToRest + "AP and recover " + recoverHealthDuringRest + " HP");

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
			uiManager.UpdateSkillInfo(selectedUnitObject);
			CheckUsableSkill();

			rightClicked = false;
			cancelClicked = false;

			isWaitingUserInput = true;
			indexOfSeletedSkillByUser = 0;
			while (indexOfSeletedSkillByUser == 0)
			{
				if (rightClicked || cancelClicked)
				{
					rightClicked = false;
					cancelClicked = false;

					uiManager.DisableSkillUI();
					currentState = CurrentState.FocusToUnit;
					isWaitingUserInput = false;
					yield break;
				}
				yield return null;
			}
			isWaitingUserInput = false;

			uiManager.DisableSkillUI();

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
			tileManager.ChangeTilesToSeletedColor(activeRange, TileColor.Red);

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

			// 타겟팅 스킬을 타겟이 없는 장소에 지정했을 경우 적용되지 않도록 예외처리 필요 - 대부분의 스킬은 논타겟팅. 추후 보강.

			tileManager.ChangeTilesFromSeletedColorToDefaultColor(activeRange);
			uiManager.DisableSkillUI();

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

		uiManager.EnableSkillCheckChainButton(isPossible);
	}

	IEnumerator CheckApplyOrChain(Vector2 selectedTilePosition)
	{
		while (currentState == CurrentState.CheckApplyOrChain)
		{
			GameObject selectedTile = tileManager.GetTile(selectedTilePosition);
			Camera.main.transform.position = new Vector3(selectedTile.transform.position.x, selectedTile.transform.position.y, -10);

			Skill selectedSkill = selectedUnitObject.GetComponent<Unit>().GetSkillList()[indexOfSeletedSkillByUser - 1];

			List<GameObject> selectedTiles = tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(), selectedTilePosition, selectedSkill.GetSecondMinReach(), selectedSkill.GetSecondMaxReach(), true);
			tileManager.ChangeTilesToSeletedColor(selectedTiles, TileColor.Red);

			CheckChainPossible();
			uiManager.SetSkillCheckAP(selectedUnitObject, selectedSkill);

			rightClicked = false;

			skillApplyCommand = SkillApplyCommand.Waiting;
			while (skillApplyCommand == SkillApplyCommand.Waiting)
			{
				if (rightClicked)
				{
					rightClicked = false;

					Camera.main.transform.position = new Vector3(selectedUnitObject.transform.position.x, selectedUnitObject.transform.position.y, -10);
					uiManager.DisableSkillCheckUI();
					tileManager.ChangeTilesFromSeletedColorToDefaultColor(selectedTiles);
					currentState = CurrentState.SelectSkillApplyPoint;
					yield break;
				}
				yield return null;
			}

			tileManager.ChangeTilesFromSeletedColorToDefaultColor(selectedTiles);

			if (skillApplyCommand == SkillApplyCommand.Apply)
			{
				skillApplyCommand = SkillApplyCommand.Waiting;
				// 체인이 가능한 스킬일 경우. 체인 발동. 
				if (selectedSkill.GetSkillApplyType() == SkillApplyType.Damage)
				{
					// 자기 자신을 체인 리스트에 추가.
					ChainList.AddChains(selectedUnitObject, selectedTiles, indexOfSeletedSkillByUser);
					// 체인 체크, 순서대로 공격.
					List<ChainInfo> allVaildChainInfo = ChainList.GetAllChainInfoToTargetArea(selectedUnitObject, selectedTiles);
					int chainCombo = allVaildChainInfo.Count;
					currentState = CurrentState.ApplySkill;
					
					foreach (var chainInfo in allVaildChainInfo)
					{
						GameObject focusedTile = chainInfo.GetTargetArea()[0];
						Camera.main.transform.position = new Vector3(focusedTile.transform.position.x, focusedTile.transform.position.y, -10);
						yield return StartCoroutine(ApplySkill(chainInfo, chainCombo)); 
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
		uiManager.DisableSkillCheckUI();
		skillApplyCommand = SkillApplyCommand.Apply;
	}

	public void CallbackChainCommand()
	{
		uiManager.DisableSkillCheckUI();
		skillApplyCommand = SkillApplyCommand.Chain;
	}

	IEnumerator ApplySkillEffect(Skill appliedSkill, GameObject unitObject, List<GameObject> selectedTiles)
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

	// 체인 가능 스킬일 경우의 스킬 시전 코루틴. 체인 정보와 배수를 받는다.
	IEnumerator ApplySkill(ChainInfo chainInfo, int chainCombo)
	{
		GameObject unitObjectInChain = chainInfo.GetUnit();
		Unit unitInChain = unitObjectInChain.GetComponent<Unit>();
		Skill appliedSkill = unitInChain.GetSkillList()[chainInfo.GetSkillIndex() - 1];
		List<GameObject> selectedTiles = chainInfo.GetTargetArea();
		
		// 시전 방향으로 유닛의 바라보는 방향을 돌림.
		unitInChain.SetDirection(Utility.GetDirectionToTarget(unitInChain.gameObject, selectedTiles));
		
		// 자신의 체인 정보 삭제.
		ChainList.RemoveChainsFromUnit(unitObjectInChain);
		
		yield return StartCoroutine(ApplySkillEffect(appliedSkill, unitInChain.gameObject, selectedTiles));

		List<GameObject> targets = new List<GameObject>();

		foreach (var tileObject in selectedTiles)
		{
			Tile tile = tileObject.GetComponent<Tile>();
			if (tile.IsUnitOnTile())
			{
				targets.Add(tile.GetUnitOnTile());
			}
		}

		foreach (var target in targets)
		{
			// 방향 체크.
			Utility.GetDegreeAtAttack(unitObjectInChain, target);
			
			if (appliedSkill.GetSkillApplyType() == SkillApplyType.Damage)
			{
				// 방향 보너스.
				float directionBouns = Utility.GetDirectionBonus(unitObjectInChain, target);
				
				var damageAmount = (int)((chainCombo * chainDamageFactor) * directionBouns * unitInChain.GetActualPower() * appliedSkill.GetPowerFactor());
				var damageCoroutine = target.GetComponent<Unit>().Damaged(unitInChain.GetUnitClass(), damageAmount, false);
				
				if (target == targets[targets.Count-1])
				{
					yield return StartCoroutine(damageCoroutine);	
				}
				else
				{
					StartCoroutine(damageCoroutine);
				}
				Debug.Log("Apply " + damageAmount + " damage to " + target.GetComponent<Unit>().GetName() + "\n" + 
							"ChainCombo : " + chainCombo);
			}
			else if (appliedSkill.GetSkillApplyType() == SkillApplyType.Heal)
			{
				var recoverAmount = (int)(unitInChain.GetActualPower() * appliedSkill.GetPowerFactor());
				var recoverHealthCoroutine = target.GetComponent<Unit>().RecoverHealth(recoverAmount); 

				if (target == targets[targets.Count-1])
				{
					yield return StartCoroutine(recoverHealthCoroutine);
				}
				else
				{
					StartCoroutine(recoverHealthCoroutine);
				}

				Debug.Log("Apply " + recoverAmount + " heal to " + target.GetComponent<Unit>().GetName());
			}
			
			// FIXME : 버프, 디버프는 아직 미구현. 데미지/힐과 별개일 때도 있고 같이 들어갈 때도 있으므로 별도의 if문으로 구현할 것.
			else
			{
				Debug.Log("Apply additional effect to " + target.GetComponent<Unit>().name);
			}
		}

		// tileManager.ChangeTilesFromSeletedColorToDefaultColor(selectedTiles);

		int requireAP = appliedSkill.GetRequireAP();
		if (unitInChain.gameObject == selectedUnitObject)
			unitInChain.UseActionPoint(requireAP); // 즉시시전 대상만 ap를 차감. 나머지는 선차감되었으므로 패스.
		indexOfSeletedSkillByUser = 0; // return to init value.

		yield return new WaitForSeconds(0.5f);

		alreadyMoved = false;
	}
	
	// 체인 불가능 스킬일 경우의 스킬 시전 코루틴. 스킬 적용 범위만 받는다.
	IEnumerator ApplySkill(List<GameObject> selectedTiles)
	{
		Unit selectedUnit = selectedUnitObject.GetComponent<Unit>();
		Skill appliedSkill = selectedUnit.GetSkillList()[indexOfSeletedSkillByUser - 1];
		
		// 시전 방향으로 유닛의 바라보는 방향을 돌림.
		selectedUnit.SetDirection(Utility.GetDirectionToTarget(selectedUnit.gameObject, selectedTiles));
		
		yield return StartCoroutine(ApplySkillEffect(appliedSkill, selectedUnitObject, selectedTiles));
		
		List<GameObject> targets = new List<GameObject>();

		foreach (var tileObject in selectedTiles)
		{
			Tile tile = tileObject.GetComponent<Tile>();
			if (tile.IsUnitOnTile())
			{
				targets.Add(tile.GetUnitOnTile());
			}
		}

		foreach (var target in targets)
		{
			if (appliedSkill.GetSkillApplyType() == SkillApplyType.Damage)
			{
				// 방향 보너스.
				float directionBouns = Utility.GetDirectionBonus(selectedUnitObject, target);
				
				var damageAmount = (int)(directionBouns * selectedUnit.GetActualPower() * appliedSkill.GetPowerFactor());
				var damageCoroutine = target.GetComponent<Unit>().Damaged(selectedUnit.GetUnitClass(), damageAmount, false);
				
				if (target == targets[targets.Count-1])
				{
					yield return StartCoroutine(damageCoroutine);	
				}
				else
				{
					StartCoroutine(damageCoroutine);
				}
				Debug.Log("Apply " + damageAmount + " damage to " + target.GetComponent<Unit>().GetName());
			}
			else if (appliedSkill.GetSkillApplyType() == SkillApplyType.Heal)
			{
				var recoverAmount = (int)(selectedUnit.GetActualPower() * appliedSkill.GetPowerFactor());
				var recoverHealthCoroutine = target.GetComponent<Unit>().RecoverHealth(recoverAmount); 

				if (target == targets[targets.Count-1])
				{
					yield return StartCoroutine(recoverHealthCoroutine);
				}
				else
				{
					StartCoroutine(recoverHealthCoroutine);
				}

				Debug.Log("Apply " + recoverAmount + " heal to " + target.GetComponent<Unit>().GetName());
			}
			
			// FIXME : 버프, 디버프는 아직 미구현. 데미지/힐과 별개일 때도 있고 같이 들어갈 때도 있으므로 별도의 if문으로 구현할 것.
			else
			{
				Debug.Log("Apply additional effect to " + target.GetComponent<Unit>().name);
			}
		}
		
		tileManager.ChangeTilesFromSeletedColorToDefaultColor(selectedTiles);

		int requireAP = appliedSkill.GetRequireAP();
		selectedUnit.UseActionPoint(requireAP);
		indexOfSeletedSkillByUser = 0; // return to init value.

		yield return new WaitForSeconds(0.5f);

		Camera.main.transform.position = new Vector3(selectedUnitObject.transform.position.x, selectedUnitObject.transform.position.y, -10);
		currentState = CurrentState.FocusToUnit;
		alreadyMoved = false;
		yield return StartCoroutine(FocusToUnit());
	}


	IEnumerator ChainAndStandby(List<GameObject> selectedTiles)
	{
		tileManager.ChangeTilesFromSeletedColorToDefaultColor(selectedTiles);
	 
		// 방향 돌리기.
		selectedUnitObject.GetComponent<Unit>().SetDirection(Utility.GetDirectionToTarget(selectedUnitObject, selectedTiles)); 
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
		cancelClicked = false;
		uiManager.EnableCancelButtonUI();

		while (currentState == CurrentState.SelectMovingPoint)
		{
			// List<GameObject> movableTiles = CheckMovableTiles(selectedUnitObject);
			Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(selectedUnitObject);
			List<GameObject> movableTiles = new List<GameObject>();
			foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
			{
				movableTiles.Add(movableTileWithPath.Value.tile);
			}
			
			tileManager.ChangeTilesToSeletedColor(movableTiles, TileColor.Blue);

			rightClicked = false;

			isWaitingUserInput = true;
			isSelectedTileByUser = false;
			while (!isSelectedTileByUser)
			{
				//yield break 넣으면 코루틴 강제종료 
				if (rightClicked || cancelClicked)
				{
					rightClicked = false;
					cancelClicked = false;
					uiManager.DisableCancelButtonUI();

					tileManager.ChangeTilesFromSeletedColorToDefaultColor(movableTiles);

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
			List<GameObject> destPath = movableTilesWithPath[selectedTilePosition].path;
			Vector2 currentTilePos = selectedUnitObject.GetComponent<Unit>().GetPosition();
			Vector2 distanceVector = selectedTilePosition - currentTilePos;
			int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
			int totalUseActionPoint = movableTilesWithPath[selectedTilePosition].requireActivityPoint;

			moveCount += distance;

			tileManager.ChangeTilesFromSeletedColorToDefaultColor(movableTiles);
			currentState = CurrentState.CheckDestination;
			uiManager.DisableCancelButtonUI();
			yield return StartCoroutine(CheckDestination(movableTiles, destTile, destPath, totalUseActionPoint, distance));

			yield return new WaitForSeconds(0.5f);
		}
		yield return null;
	}

	IEnumerator CheckDestination(List<GameObject> nearbyTiles, GameObject destTile, List<GameObject> destPath, int totalUseActionPoint, int distance)
	{
		while (currentState == CurrentState.CheckDestination)
		{
			// 목표지점만 푸른색으로 표시
			// List<GameObject> destTileList = new List<GameObject>();
			// destTileList.Add(destTile);
			List<GameObject> destTileList = destPath;
			destTileList.Add(destTile);
			tileManager.ChangeTilesToSeletedColor(destTileList, TileColor.Blue);
			// UI를 띄우고
			uiManager.EnableSelectDirectionUI();
			uiManager.SetDestCheckUIAP(selectedUnitObject, totalUseActionPoint);

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
					tileManager.ChangeTilesToSeletedColor(nearbyTiles, TileColor.Blue);
					uiManager.DisableSelectDirectionUI();
					uiManager.DisableDestCheckUI();
					currentState = CurrentState.SelectMovingPoint;
					isWaitingUserInput = false;
					yield break;
				}
				yield return null;
			}
			isSelectedDirectionByUser = false;
			isWaitingUserInput = false;

			// 방향을 클릭하면 그 자리로 이동. MoveToTile 호출 
			tileManager.ChangeTilesFromSeletedColorToDefaultColor(destTileList);
			currentState = CurrentState.MoveToTile;
			uiManager.DisableDestCheckUI();
			yield return StartCoroutine(MoveToTile(destTile, selectedDirection, totalUseActionPoint));
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

	IEnumerator MoveToTile(GameObject destTile, Direction directionAtDest, int totalUseActionPoint)
	{
		GameObject currentTile = tileManager.GetTile(selectedUnitObject.GetComponent<Unit>().GetPosition());
		currentTile.GetComponent<Tile>().SetUnitOnTile(null);
		selectedUnitObject.transform.position = destTile.transform.position + new Vector3(0, 0, -5f);
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
