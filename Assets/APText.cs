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
		foreach (var unit in unitManager.units)
		{
			string unitText = unit.GetComponent<Unit>().name + " : " + unit.GetComponent<Unit>().GetActionPoint() + "\n";
			newText += unitText;
		}
		text.text = newText;
	}
}
