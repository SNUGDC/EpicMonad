using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class SkillAndChainState
{
	public static IEnumerator SelectSkillState(BattleManager battleManager)
	{
		while (battleManager.currentState == CurrentState.SelectSkill)
		{
			battleManager.uiManager.UpdateSkillInfo(battleManager.selectedUnitObject);
			battleManager.uiManager.CheckUsableSkill(battleManager.selectedUnitObject);

			battleManager.rightClicked = false;
			battleManager.cancelClicked = false;

			battleManager.isWaitingUserInput = true;
			battleManager.indexOfSeletedSkillByUser = 0;
			while (battleManager.indexOfSeletedSkillByUser == 0)
			{
				if (battleManager.rightClicked || battleManager.cancelClicked)
				{
					battleManager.rightClicked = false;
					battleManager.cancelClicked = false;

					battleManager.uiManager.DisableSkillUI();
					battleManager.currentState = CurrentState.FocusToUnit;
					battleManager.isWaitingUserInput = false;
					yield break;
				}
				yield return null;
			}
			battleManager.isWaitingUserInput = false;

			battleManager.uiManager.DisableSkillUI();

			Skill selectedSkill = battleManager.selectedUnitObject.GetComponent<Unit>().GetSkillList()[battleManager.indexOfSeletedSkillByUser - 1];
			SkillType skillTypeOfSelectedSkill = selectedSkill.GetSkillType();
			if (skillTypeOfSelectedSkill == SkillType.Area)
			{
				battleManager.currentState = CurrentState.SelectSkillApplyDirection;
				yield return battleManager.StartCoroutine(SelectSkillApplyDirection(battleManager, battleManager.selectedUnitObject.GetComponent<Unit>().GetDirection()));
			}
			else
			{
				battleManager.currentState = CurrentState.SelectSkillApplyPoint;
				yield return battleManager.StartCoroutine(SelectSkillApplyPoint(battleManager, battleManager.selectedUnitObject.GetComponent<Unit>().GetDirection()));
			}
		}
	}

	private static IEnumerator SelectSkillApplyDirection(BattleManager battleManager, Direction originalDirection)
	{
		Direction beforeDirection = originalDirection;
		List<GameObject> selectedTiles = new List<GameObject>();
		Unit selectedUnit = battleManager.selectedUnitObject.GetComponent<Unit>();
		Skill selectedSkill = selectedUnit.GetSkillList()[battleManager.indexOfSeletedSkillByUser - 1];

		battleManager.rightClicked = false;
		battleManager.isWaitingUserInput = true;
		battleManager.isSelectedTileByUser = false;

		if (battleManager.currentState == CurrentState.SelectSkill)
		{
			battleManager.uiManager.DisableCancelButtonUI();
			yield break;
		}

		if (battleManager.currentState == CurrentState.SelectSkillApplyDirection)
		{
			selectedTiles = battleManager.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
														selectedUnit.GetPosition(),
														selectedSkill.GetSecondMinReach(),
														selectedSkill.GetSecondMaxReach(),
														selectedUnit.GetDirection(),
														false);

			battleManager.tileManager.ChangeTilesToSeletedColor(selectedTiles, TileColor.Red);
		}

		while (battleManager.currentState == CurrentState.SelectSkillApplyDirection)
		{
			Direction newDirection = Utility.GetMouseDirectionByUnit(battleManager.selectedUnitObject);
			// Debug.LogWarning(newDirection);
			if (beforeDirection != newDirection)
			{
				battleManager.tileManager.ChangeTilesFromSeletedColorToDefaultColor(selectedTiles);

				beforeDirection = newDirection;
				selectedUnit.SetDirection(newDirection);
				selectedTiles = battleManager.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
															selectedUnit.GetPosition(),
															selectedSkill.GetSecondMinReach(),
															selectedSkill.GetSecondMaxReach(),
															selectedUnit.GetDirection(),
															false);

				battleManager.tileManager.ChangeTilesToSeletedColor(selectedTiles, TileColor.Red);
			}

			if (battleManager.rightClicked || battleManager.cancelClicked)
			{
				battleManager.rightClicked = false;
				battleManager.cancelClicked = false;
				battleManager.uiManager.DisableCancelButtonUI();

				selectedUnit.SetDirection(originalDirection);
				battleManager.tileManager.ChangeTilesFromSeletedColorToDefaultColor(selectedTiles);
				battleManager.currentState = CurrentState.SelectSkill;
				yield break;
			}

			if (battleManager.isSelectedTileByUser)
			{
				battleManager.isWaitingUserInput = false;
				battleManager.uiManager.DisableCancelButtonUI();
				battleManager.tileManager.ChangeTilesFromSeletedColorToDefaultColor(selectedTiles);

				battleManager.currentState = CurrentState.CheckApplyOrChain;
				yield return battleManager.StartCoroutine(CheckApplyOrChain(battleManager, selectedUnit.GetPosition(), originalDirection));
			}
			yield return null;
		}
		yield return null;
	}

	private static IEnumerator SelectSkillApplyPoint(BattleManager battleManager, Direction originalDirection)
	{
		Direction beforeDirection = originalDirection;
		Unit selectedUnit = battleManager.selectedUnitObject.GetComponent<Unit>();

		if (battleManager.currentState == CurrentState.SelectSkill)
		{
			battleManager.uiManager.DisableCancelButtonUI();
			yield break;
		}

		while (battleManager.currentState == CurrentState.SelectSkillApplyPoint)
		{
			Vector2 selectedUnitPos = battleManager.selectedUnitObject.GetComponent<Unit>().GetPosition();

			List<GameObject> activeRange = new List<GameObject>();
			Skill selectedSkill = battleManager.selectedUnitObject.GetComponent<Unit>().GetSkillList()[battleManager.indexOfSeletedSkillByUser - 1];
			activeRange = battleManager.tileManager.GetTilesInRange(selectedSkill.GetFirstRangeForm(),
													  selectedUnitPos,
													  selectedSkill.GetFirstMinReach(),
													  selectedSkill.GetFirstMaxReach(),
													  battleManager.selectedUnitObject.GetComponent<Unit>().GetDirection(),
													  selectedSkill.GetIncludeMyself());
			battleManager.tileManager.ChangeTilesToSeletedColor(activeRange, TileColor.Red);

			battleManager.rightClicked = false;
			battleManager.cancelClicked = false;
			battleManager.uiManager.EnableCancelButtonUI();

			battleManager.isWaitingUserInput = true;
			battleManager.isSelectedTileByUser = false;
			while (!battleManager.isSelectedTileByUser)
			{
				Direction newDirection = Utility.GetMouseDirectionByUnit(battleManager.selectedUnitObject);
				if (beforeDirection != newDirection)
				{
					beforeDirection = newDirection;
					selectedUnit.SetDirection(newDirection);
				}

				if (battleManager.rightClicked || battleManager.cancelClicked)
				{
					battleManager.rightClicked = false;
					battleManager.cancelClicked = false;
					battleManager.uiManager.DisableCancelButtonUI();

					battleManager.tileManager.ChangeTilesFromSeletedColorToDefaultColor(activeRange);
					battleManager.currentState = CurrentState.SelectSkill;
					battleManager.isWaitingUserInput = false;
					yield break;
				}
				yield return null;
			}
			battleManager.isSelectedTileByUser = false;
			battleManager.isWaitingUserInput = false;
			battleManager.uiManager.DisableCancelButtonUI();

			// 타겟팅 스킬을 타겟이 없는 장소에 지정했을 경우 적용되지 않도록 예외처리 필요 - 대부분의 스킬은 논타겟팅. 추후 보강.

			battleManager.tileManager.ChangeTilesFromSeletedColorToDefaultColor(activeRange);
			battleManager.uiManager.DisableSkillUI();

			battleManager.currentState = CurrentState.CheckApplyOrChain;
			yield return battleManager.StartCoroutine(CheckApplyOrChain(battleManager, battleManager.selectedTilePosition, originalDirection));
		}
	}

	private static IEnumerator CheckApplyOrChain(BattleManager battleManager, Vector2 selectedTilePosition, Direction originalDirection)
	{
		while (battleManager.currentState == CurrentState.CheckApplyOrChain)
		{
			GameObject selectedTile = battleManager.tileManager.GetTile(selectedTilePosition);
			Camera.main.transform.position = new Vector3(selectedTile.transform.position.x, selectedTile.transform.position.y, -10);

			Skill selectedSkill = battleManager.selectedUnitObject.GetComponent<Unit>().GetSkillList()[battleManager.indexOfSeletedSkillByUser - 1];

			List<GameObject> selectedTiles = battleManager.tileManager.GetTilesInRange(selectedSkill.GetSecondRangeForm(),
																		 selectedTilePosition,
																		 selectedSkill.GetSecondMinReach(),
																		 selectedSkill.GetSecondMaxReach(),
																		 battleManager.selectedUnitObject.GetComponent<Unit>().GetDirection(),
																		 true);
			if ((selectedSkill.GetSkillType() == SkillType.Area) && (!selectedSkill.GetIncludeMyself()))
				selectedTiles.Remove(battleManager.tileManager.GetTile(selectedTilePosition));
			battleManager.tileManager.ChangeTilesToSeletedColor(selectedTiles, TileColor.Red);

			CheckChainPossible(battleManager);
			battleManager.uiManager.SetSkillCheckAP(battleManager.selectedUnitObject, selectedSkill);

			battleManager.rightClicked = false;
			battleManager.cancelClicked = false;

			battleManager.skillApplyCommand = SkillApplyCommand.Waiting;
			while (battleManager.skillApplyCommand == SkillApplyCommand.Waiting)
			{
				if (battleManager.rightClicked || battleManager.cancelClicked)
				{
					battleManager.rightClicked = false;
					battleManager.cancelClicked = false;

					Camera.main.transform.position = new Vector3(battleManager.selectedUnitObject.transform.position.x, battleManager.selectedUnitObject.transform.position.y, -10);
					battleManager.uiManager.DisableSkillCheckUI();
					battleManager.tileManager.ChangeTilesFromSeletedColorToDefaultColor(selectedTiles);
					battleManager.selectedUnitObject.GetComponent<Unit>().SetDirection(originalDirection);
					if (selectedSkill.GetSkillType() == SkillType.Area)
						battleManager.currentState = CurrentState.SelectSkill;
					else
						battleManager.currentState = CurrentState.SelectSkillApplyPoint;
					yield break;
				}
				yield return null;
			}

			battleManager.tileManager.ChangeTilesFromSeletedColorToDefaultColor(selectedTiles);

			if (battleManager.skillApplyCommand == SkillApplyCommand.Apply)
			{
				battleManager.skillApplyCommand = SkillApplyCommand.Waiting;
				// 체인이 가능한 스킬일 경우. 체인 발동.
				if (selectedSkill.GetSkillApplyType() == SkillApplyType.Damage)
				{
					// 자기 자신을 체인 리스트에 추가.
					ChainList.AddChains(battleManager.selectedUnitObject, selectedTiles, battleManager.indexOfSeletedSkillByUser);
					// 체인 체크, 순서대로 공격.
					List<ChainInfo> allVaildChainInfo = ChainList.GetAllChainInfoToTargetArea(battleManager.selectedUnitObject, selectedTiles);
					int chainCombo = allVaildChainInfo.Count;
					battleManager.currentState = CurrentState.ApplySkill;

					foreach (var chainInfo in allVaildChainInfo)
					{
						GameObject focusedTile = chainInfo.GetTargetArea()[0];
						Camera.main.transform.position = new Vector3(focusedTile.transform.position.x, focusedTile.transform.position.y, -10);
						yield return battleManager.StartCoroutine(battleManager.ApplySkill(chainInfo, chainCombo));
					}

					Camera.main.transform.position = new Vector3(battleManager.selectedUnitObject.transform.position.x, battleManager.selectedUnitObject.transform.position.y, -10);
					battleManager.currentState = CurrentState.FocusToUnit;
					yield return battleManager.StartCoroutine(battleManager.FocusToUnit());
				}
				// 체인이 불가능한 스킬일 경우, 그냥 발동.
				else
				{
					battleManager.currentState = CurrentState.ApplySkill;
					yield return battleManager.StartCoroutine(battleManager.ApplySkill(selectedTiles));
				}
			}
			else if (battleManager.skillApplyCommand == SkillApplyCommand.Chain)
			{
				battleManager.skillApplyCommand = SkillApplyCommand.Waiting;
				battleManager.currentState = CurrentState.ChainAndStandby;
				yield return battleManager.StartCoroutine(battleManager.ChainAndStandby(selectedTiles));
			}
			else
				yield return null;
		}
		yield return null;
	}

	private static void CheckChainPossible(BattleManager battleManager)
	{
		bool isPossible = false;

		// ap 조건으로 체크.
		int requireAP = battleManager.selectedUnitObject.GetComponent<Unit>().GetSkillList()[battleManager.indexOfSeletedSkillByUser - 1].GetRequireAP();
		int remainAPAfterChain = battleManager.selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() - requireAP;

		foreach (var unit in battleManager.unitManager.GetAllUnits())
		{
			if ((unit != battleManager.selectedUnitObject) &&
			(unit.GetComponent<Unit>().GetCurrentActivityPoint() > remainAPAfterChain))
			{
				isPossible = true;
			}
		}

		// 스킬 타입으로 체크. 공격스킬만 체인을 걸 수 있음.
		if (battleManager.selectedUnitObject.GetComponent<Unit>().GetSkillList()[battleManager.indexOfSeletedSkillByUser - 1].GetSkillApplyType()
			!= SkillApplyType.Damage)
		{
			isPossible = false;
		}

		battleManager.uiManager.EnableSkillCheckChainButton(isPossible);
	}

}
