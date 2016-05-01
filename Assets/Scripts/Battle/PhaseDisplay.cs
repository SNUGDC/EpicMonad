using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PhaseDisplay : MonoBehaviour {

	Text phaseText;
	Text standardAPText;
	UnitManager unitManager;
	BattleManager gameManager;

	// Use this for initialization
	void Start () {
		phaseText = transform.Find("PhaseText").GetComponent<Text>();
		standardAPText = transform.Find("StandardAPText").GetComponent<Text>();
		unitManager = FindObjectOfType<UnitManager>();
		gameManager = FindObjectOfType<BattleManager>();
	}

	// Update is called once per frame
	void Update () {
		phaseText.text = "[Phase " + FindObjectOfType<BattleManager>().GetCurrentPhase() + " / 30]";
		standardAPText.text = "Standard AP : " + unitManager.GetStandardActionPoint() + "";
	}
}
