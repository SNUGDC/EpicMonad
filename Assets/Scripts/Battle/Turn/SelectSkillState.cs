using UnityEngine;
using System.Collections;
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
				yield return battleManager.StartCoroutine(battleManager.SelectSkillApplyDirection(battleManager.selectedUnitObject.GetComponent<Unit>().GetDirection()));
			}
			else
			{
				battleManager.currentState = CurrentState.SelectSkillApplyPoint;
				yield return battleManager.StartCoroutine(battleManager.SelectSkillApplyPoint(battleManager.selectedUnitObject.GetComponent<Unit>().GetDirection()));
			}
		}
	}
}
