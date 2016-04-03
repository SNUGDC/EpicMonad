using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class Editor : MonoBehaviour {

	public void SetApGap (string input)
	{
		EditInfo.ApGap = Int32.Parse(input);
		GameObject.Find("CurrentApGap").GetComponent<Text>().text = "Cur : " + EditInfo.ApGap.ToString();
	}
	
	public void SetRequireApAtFlatland (string input)
	{
		EditInfo.RequireApAtFlatland = Int32.Parse(input);
		GameObject.Find("CurrentApAtFlatland").GetComponent<Text>().text = "Cur : " + EditInfo.RequireApAtFlatland.ToString();
	}
	
	public void SetRequireApAtHill (string input)
	{
		EditInfo.RequireApAtHill = Int32.Parse(input);
		GameObject.Find("CurrentApAtHill").GetComponent<Text>().text = "Cur : " + EditInfo.RequireApAtHill.ToString();
	}

	// Use this for initialization
	void Start () {
		GameObject.Find("CurrentApGap").GetComponent<Text>().text = "Cur : " + EditInfo.ApGap.ToString();
		GameObject.Find("CurrentApAtFlatland").GetComponent<Text>().text = "Cur : " + EditInfo.RequireApAtFlatland.ToString();
		GameObject.Find("CurrentApAtHill").GetComponent<Text>().text = "Cur : " + EditInfo.RequireApAtHill.ToString();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
