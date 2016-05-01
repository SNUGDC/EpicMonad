using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class SelectSkillState
{
	public static IEnumerator Run(BattleManager battleManager)
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
				yield return battleManager.StartCoroutine(battleManager.CheckApplyOrChain(selectedUnit.GetPosition(), originalDirection));
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
			yield return battleManager.StartCoroutine(battleManager.CheckApplyOrChain(battleManager.selectedTilePosition, originalDirection));
		}
	}

}
