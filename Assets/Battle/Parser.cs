using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Parser : MonoBehaviour {

	public static List<UnitInfo> GetParsedUnitInfo()
	{
        List<UnitInfo> unitInfoList = new List<UnitInfo>();
		
		TextAsset csvFile = Resources.Load("testStageUnitInfo") as TextAsset;
		string csvText = csvFile.text;
		string[] unparsedUnitInfoStrings = csvText.Split('\n');
		
		for (int i = 1; i < unparsedUnitInfoStrings.Length; i++)
		{
			UnitInfo unitInfo = new UnitInfo(unparsedUnitInfoStrings[i]);
			unitInfoList.Add(unitInfo);
		}
        
        return unitInfoList;
	}
    
    public static List<SkillInfo> GetParsedSkillInfo()
	{
        List<SkillInfo> skillInfoList = new List<SkillInfo>();
		
		TextAsset csvFile = Resources.Load("testSkillInfo") as TextAsset;
		string csvText = csvFile.text;
		string[] unparsedSkillInfoStrings = csvText.Split('\n');
		
		for (int i = 1; i < unparsedSkillInfoStrings.Length; i++)
		{
			SkillInfo skillInfo = new SkillInfo(unparsedSkillInfoStrings[i]);
			skillInfoList.Add(skillInfo);
		}
        
        return skillInfoList;
	}
}
