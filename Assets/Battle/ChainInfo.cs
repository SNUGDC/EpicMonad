using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChainInfo {

    // 체인에 필요한 정보?
    // 시전자, 대상, 시전스킬?
    GameObject unit;
    int skillIndex;
    List<GameObject> targets;

    public ChainInfo (GameObject unit, int skillIndex, List<GameObject> targets)
    {
        this.unit = unit;
        this.skillIndex = skillIndex;
        this.targets = targets; 
    }
}
