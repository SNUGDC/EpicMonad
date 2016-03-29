using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {

	public TextAsset dialogueData;

	Sprite transparent;
	
	Image leftPortrait;
	Image rightPortrait; 
	Text nameText;
	Text dialogueText;
	
	string leftUnit;
	string rightUnit;
	
	List<DialogueData> dialogueDataList;
	int line;
	int endLine;
	bool isWaitingMouseInput;

	void Initialize()
	{
		transparent = Resources.Load("StandingImage/" + "transparent", typeof(Sprite)) as Sprite;
		
		leftPortrait = GameObject.Find("LeftPortrait").GetComponent<Image>();
		rightPortrait = GameObject.Find("RightPortrait").GetComponent<Image>();
		nameText = GameObject.Find("NameText").GetComponent<Text>();
		dialogueText = GameObject.Find("DialogueText").GetComponent<Text>();
		
		leftPortrait.sprite = transparent;
		rightPortrait.sprite = transparent;
		
		leftUnit = null;
		rightUnit = null;
		
		dialogueDataList = Parser.GetParsedDialogueData(dialogueData);
		
		line = 0;
		endLine = dialogueDataList.Count; 
		
		StartCoroutine(PrintLine());
	}

	IEnumerator PrintLine()
	{
		while (line < endLine)
		{
			leftPortrait.color = Color.gray;
			rightPortrait.color = Color.gray;
			
			if (!dialogueDataList[line].IsEffect())
			{
				if (leftUnit == dialogueDataList[line].GetNameInCode())
					leftPortrait.color = Color.white;
				else if (rightUnit == dialogueDataList[line].GetNameInCode())
					rightPortrait.color = Color.white;
				
				if (dialogueDataList[line].GetName() != "-")
					nameText.text = "[" + dialogueDataList[line].GetName() + "]";
				else 
					nameText.text = null;
				dialogueText.text = dialogueDataList[line].GetDialogue();
				
							
				isWaitingMouseInput = true;
				while (isWaitingMouseInput)
				{
					yield return null;
				}
			}
			else 
			{
				if (dialogueDataList[line].GetEffectType() == "appear")
				{
					if (dialogueDataList[line].GetEffectSubType() == "left")
					{
						leftUnit = dialogueDataList[line].GetNameInCode();
						Debug.Log("StandingImage/" + dialogueDataList[line].GetNameInCode() + "_standing");
						leftPortrait.sprite = Resources.Load("StandingImage/" + dialogueDataList[line].GetNameInCode() + "_standing", typeof(Sprite)) as Sprite;
					}
					else if (dialogueDataList[line].GetEffectSubType() == "right")
					{
						rightUnit = dialogueDataList[line].GetNameInCode();
						rightPortrait.sprite = Resources.Load("StandingImage/" + dialogueDataList[line].GetNameInCode() + "_standing", typeof(Sprite)) as Sprite;
					}
					else 
					{
						Debug.LogError("Undefined effectSubType : " + dialogueDataList[line].GetEffectSubType());
					}
				}
				else if (dialogueDataList[line].GetEffectType() == "disappear")
				{
					if (dialogueDataList[line].GetEffectSubType() == "left")
					{
						leftUnit = null;
						leftPortrait.sprite = Resources.Load("StandingImage/" + "transparent", typeof(Sprite)) as Sprite;
					}
					else if (dialogueDataList[line].GetEffectSubType() == "right")
					{
						rightUnit = null;
						rightPortrait.sprite = Resources.Load("StandingImage/" + "transparent", typeof(Sprite)) as Sprite;
					}
					else 
					{
						Debug.LogError("Undefined effectSubType : " + dialogueDataList[line].GetEffectSubType());
					}
				}
				else
				{
					Debug.LogError("Undefined effectType : " + dialogueDataList[line].GetEffectType());
				}
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
