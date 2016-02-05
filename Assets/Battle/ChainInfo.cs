using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChainInfo {

    // 체인에 필요한 정보?
    // 시전자, 영역, 시전스킬
    GameObject unit;
    List<GameObject> targetArea;
    int skillIndex;

    public ChainInfo (GameObject unit, List<GameObject> targetArea, int skillIndex)
    {
        this.unit = unit;
        this.targetArea = targetArea;
        this.skillIndex = skillIndex;
    }
    
    public GameObject GetUnit()
    {
        return unit;
    }
    
    public List<GameObject> GetTargetArea()
    {
        return targetArea;
    }
    
    public int GetSkillIndex()
    {
        return skillIndex;
    }
    
    public bool Overlapped(List<GameObject> anotherTargetArea)
    {
        foreach (var anotherTargetPoint in anotherTargetArea)
        {
            if (targetArea.Contains(anotherTargetPoint))
                return true;
        }
        
        return false;
    }
}
