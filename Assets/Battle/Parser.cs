using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Parser : MonoBehaviour {

    private static List<UnitInfo> unitInfoList = new List<UnitInfo>();

    public static void ParsingUnitInfo()
    {
        unitInfoList.Clear();
        
        TextAsset csvFile = Resources.Load("testStageUnitInfo") as TextAsset;
		string csvText = csvFile.text;
		string[] unparsedUnitInfoStrings = csvText.Split('\n');
        
        for (int i = 1; i < unparsedUnitInfoStrings.Length; i++)
		{
			UnitInfo unitInfo = new UnitInfo(unparsedUnitInfoStrings[i]);
			unitInfoList.Add(unitInfo);
		}
    }
    
    public static List<UnitInfo> GetParsedUnitInfo()
    {
        ParsingUnitInfo();
        
        return unitInfoList;
    }
}
