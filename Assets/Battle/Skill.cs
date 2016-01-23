using Enums;
using System.Collections;
using UnityEngine;

public class Skill {

    // base info.
    string name;
    int requireAP;
    
    // damage factors. - temp.
    float powerFactor;
    
    // reach & range
    // 지정/범위/경로. 아직 보류중. 
    // SkillType skillType;
    RangeForm firstRangeForm;
    int firstMinReach;
    int firstMaxReach;
    int firstWidth;
    bool includeMyself;
    // FIXME : 대상 포함 여부 넣기
    // 2차범위. 지금은 안 씀.
    // RangeForm secondRangeForm;
    // int secondMinReach;
    // int secondMaxReach;
    // int secondWidth;
    
    public Skill(string name, int requireAP, float powerFactor,
                 RangeForm firstRangeForm, int firstMinReach, int firstMaxReach, int firstWidth,
                 bool includeMyself)
    {
        this.name = name;
        this.requireAP = requireAP;
        this.powerFactor = powerFactor;
        this.firstRangeForm = firstRangeForm;
        this.firstMinReach = firstMinReach;
        this.firstMaxReach = firstMaxReach;
        this.firstWidth = firstWidth;
        this.includeMyself = includeMyself;
    }  
    
    public string GetName()
    {
        return name;
    }
    
    public int GetRequireAP()
    {
        return requireAP;
    }

    public RangeForm GetFirstRangeForm()
    {
        return firstRangeForm;
    }
    
    public int GetFirstMinReach()
    {
        return firstMinReach;
    }
    
    public int GetFirstMaxReach()
    {
        return firstMaxReach;
    }
    
    public int GetFirstWidth()
    {
        return firstWidth;
    }
    
    public bool IncludeMyself()
    {
        return includeMyself;
    }
}
