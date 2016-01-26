using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class Unit : MonoBehaviour {

	// FIXME : public -> private
	public new string name;
    
    // 하드코딩된 기본 스킬리스트를 받아옴.
    List<Skill> skillList = new List<Skill>();
    
    // FIXME : temp values
    // public int[] requireAPOfSkills;
    public Vector2 initPosition;
    
    // Base stats. FIXME : 지금은 수동으로 셋팅.
    public float baseHealth; //체력 
    public float basePower; //공격력
    public float baseDefense; //방어력
    public float baseResistence; //저항력
    public float baseDexturity; //행동력
    public float baseReach; //거리
    public float baseRange; //범위
    
    // Applied stats.
    int maxHealth;
    int power;
    int defense;
    int resistence;
    int dexturity;
    int reach;
    int range;
    
    // type. FIXME : 지금은 수동으로 셋팅
    public UnitClass unitClass;
    public Element element;
    public Celestial celestial;
    
    // Variable values.
	public Vector2 position;
    public Direction direction;
    public int currentHealth; 
	public int activityPoint;
	// public int regenerationActionPoint;

    public List<Skill> GetSkillList()
    {
        return skillList;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    public int GetActualDexturity()
    {
        // FIXME : 버프 / 디버프 값 적용
        return dexturity;
    }

	public int GetCurrentActivityPoint()
	{
		return activityPoint;
	}

    public void SetUnitClass(UnitClass unitClass)
    {
        this.unitClass = unitClass;
    }
    
    public UnitClass GetUnitClass()
    {
        return unitClass;
    }
    
    public void SetElement(Element element)
    {
        this.element = element;
    }
    
    public Element GetElement()
    {
        return element;
    }
    
    public void SetCelestial(Celestial celestial)
    {
        this.celestial = celestial;
    }
    
    public Celestial GetCelestial()
    {
        return celestial;
    }

    public string GetName()
    {
        return name;
    }

	public void SetName(string name)
	{
		this.name = name;
	}
	
	public void SetPosition(Vector2 position)
	{
		this.position = position;
	}
	
	public Vector2 GetPosition()
	{
		return position;
	}

    public void SetBuff(Buff buff)
    {
        // 껍데기.
    }

    public void SetDebuff(Debuff debuff)
    {
        // 껍데기.
    }

    public void Damaged(DamageType type, int amount)
    {
        // 공격이 물리인지 마법인지 체크
        // 방어력 / 저항력 중 맞는 값을 적용
        // 방어 감소 / 저항 감소 적용
        // 체력 깎임 
    }

    public void RecoverHealth(int amount)
    {
        // 내상 효과 적용할 것! 아직 미적용
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

	public void RegenerateActionPoint()
	{
		activityPoint += GetActualDexturity(); // 페이즈당 행동력 회복량 = 민첩성 * 보정치(버프/디버프) 
		Debug.Log(name + " recover " + dexturity + "AP. Current AP : " + activityPoint);
	}

	public void UseActionPoint(int amount)
	{
		activityPoint -= amount;
		Debug.Log(name + " use " + amount + "AP. Current AP : " + activityPoint);
	}

    void applyStats()
    {
        maxHealth = (int)baseHealth;
        power = (int)basePower;
        defense = (int)baseDefense;
        resistence = (int)baseResistence;
        dexturity = (int)baseDexturity;
        reach = (int)baseReach;
        range = (int)baseRange;
    }

    void initialize()
    {
        currentHealth = maxHealth;
        activityPoint = (int)(dexturity * 1.5f);
        skillList = SkillLoader.MakeSkillList();
    }

	// Use this for initialization
	void Start () {
        applyStats();
        initialize();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
