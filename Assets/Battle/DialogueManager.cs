using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {

	public TextAsset dialogueData;

	Image leftPortrait;
	Image rightPortrait; 
	Text nameText;
	Text dialogueText;
	
	List<DialogueData> dialogueDataList;
	int line;
	int endLine;
	bool isWaitingMouseInput;

	void Initialize()
	{
		leftPortrait = GameObject.Find("LeftPortrait").GetComponent<Image>();
		rightPortrait = GameObject.Find("RightPortrait").GetComponent<Image>();
		nameText = GameObject.Find("NameText").GetComponent<Text>();
		dialogueText = GameObject.Find("DialogueText").GetComponent<Text>();
		
		dialogueDataList = Parser.GetParsedDialogueData(dialogueData);
		
		line = 0;
		endLine = dialogueDataList.Count; 
		
		StartCoroutine(PrintLine());
	}

	IEnumerator PrintLine()
	{
		while (line < endLine)
		{
			if (!dialogueDataList[line].IsEffect())
			{
				if (dialogueDataList[line].GetName() != "-")
					nameText.text = "[" + dialogueDataList[line].GetName() + "]";
				else 
					nameText.text = "";
				dialogueText.text = dialogueDataList[line].GetDialogue();
				
							
				isWaitingMouseInput = true;
				while (isWaitingMouseInput)
				{
					yield return null;
				}
			}
			else 
			{
				line++;
			}
			yield return null;
		}
		yield return null;
	}
	
	void OnMouseDown()
	{
		if (isWaitingMouseInput)
		{
			isWaitingMouseInput = false;
			line++;
		}
	}
	
	// Use this for initialization
	void Start () {
		Initialize();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
