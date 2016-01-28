using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChainInfo {

    // 체인에 필요한 정보?
    // 시전자, 대상, 시전스킬
    GameObject unit;
    GameObject target;
    int skillIndex;

    public ChainInfo (GameObject unit, GameObject target, int skillIndex)
    {
        this.unit = unit;
        this.target = target;
        this.skillIndex = skillIndex;
    }
    
    public GameObject GetUnit()
    {
        return unit;
    }
    
    public GameObject GetTarget()
    {
        return target;
    }
    
    public int GetSkillIndex()
    {
        return skillIndex;
    }
}
