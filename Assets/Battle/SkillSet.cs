using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;
    
public class SkillSet {

    List<Skill> skillList;

	public SkillSet ()
    {
        // 일단 임의로 하드코딩된 스킬셋을 사용.
        // 스킬에 필요한 정보는 아래와 같음. 
        // string name, int requireAP, float powerFactor,
        // RangeForm firstRangeForm, int firstMinReach, int firstMaxReach, int firstWidth,
        // bool includeMyself
        skillList = new List<Skill>();
        Skill skill1 = new Skill("암흑 폭발", 40, 1.0f, 
                                 RangeForm.square, 0, 4, 0, false);
        Skill skill2 = new Skill("태초의 빛", 35, 1.0f, 
                                 RangeForm.square, 0, 4, 0, true);
        Skill skill3 = new Skill("사념 포박", 100, 1.5f, 
                                 RangeForm.square, 0, 3, 0, false);
        Skill skill4 = new Skill("마력 보호막", 80, 1.3f, 
                                 RangeForm.square, 0, 0, 0, true);
        Skill skill5 = new Skill("영혼의 불꽃", 130, 2.0f, 
                                 RangeForm.square, 0, 3, 0, false);
        skillList.Add(skill1);
        skillList.Add(skill2);
        skillList.Add(skill3);
        skillList.Add(skill4);
        skillList.Add(skill5);
    }
}
