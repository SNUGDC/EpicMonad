using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class APText : MonoBehaviour {

	Text text;
	UnitManager unitManager;

	// Use this for initialization
	void Start () {
		text = gameObject.GetComponent<Text>();
		unitManager = FindObjectOfType<UnitManager>();
	}
	
	// Update is called once per frame
	void Update () {
		string newText = "Current Action Point\n";
		foreach (var unit in unitManager.GetAllUnits())
		{
			string unitText = unit.GetComponent<Unit>().name + " : " + unit.GetComponent<Unit>().GetCurrentActionPoint() + "\n";
			newText += unitText;
		}
		text.text = newText;
	}
}
