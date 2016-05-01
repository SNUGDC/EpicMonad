using UnityEngine;
using System.Collections;

public class RestAndRecover {

	public static IEnumerator Run(BattleManager battleManager)
	{
		int usingActivityPointToRest = (int)(battleManager.selectedUnitObject.GetComponent<Unit>().GetCurrentActivityPoint() * 0.9f);
		int recoverHealthDuringRest = (int)(battleManager.selectedUnitObject.GetComponent<Unit>().GetMaxHealth() * (usingActivityPointToRest / 100f));
		battleManager.selectedUnitObject.GetComponent<Unit>().UseActionPoint(usingActivityPointToRest);
		IEnumerator recoverHealthCoroutine = battleManager.selectedUnitObject.GetComponent<Unit>().RecoverHealth(recoverHealthDuringRest);
		yield return battleManager.StartCoroutine(recoverHealthCoroutine);

		Debug.Log("Rest. Using " + usingActivityPointToRest + "AP and recover " + recoverHealthDuringRest + " HP");

		yield return new WaitForSeconds(0.5f);
	}

}
