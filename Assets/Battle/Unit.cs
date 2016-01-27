using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;
using System.Linq;

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

    List<Buff> buffList;
    List<Debuff> debuffList;

    // FIXME : 임시로 공격력만 외부에서 읽음.
    public int GetPower()
    {
        return power;
    }

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
        int actualDexturity = dexturity;
        // FIXME : 버프 / 디버프 값 적용
        
        // 디버프값만 적용.
        if (debuffList.Any(k => k.GetName() == DebuffType.faint))
        {
            actualDexturity = 0;
        }
        else if (debuffList.Any(k => k.GetName() == DebuffType.exhaust))
        {
            int totalDegree = 0;
            foreach (var debuff in debuffList)
            {
                if (debuff.GetName() == DebuffType.exhaust)
                {
                    totalDegree = debuff.GetDegree();
                }
            }
            actualDexturity *= (totalDegree/100);
        }
        return actualDexturity;
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
        buffList.Add(buff);
    }
    
    public void RemainBuff()
    {
        buffList.Remove(buffList[0]);
    }
    
    public void RemainAllBuff()
    {
        buffList.Clear();
    }

    public void SetDebuff(Debuff debuff)
    {
        debuffList.Add(debuff);
    }
    
    public void RemainDebuff()
    {
        debuffList.Remove(debuffList[0]);
    }
    
    public void RemainAllDebuff()
    {
        debuffList.Clear();
    }
    
    public void DecreaseRemainPhaseBuffAndDebuff()
    {
        List<Buff> newBuffList = new List<Buff>();
        foreach (var buff in buffList)
        {
            buff.DecreaseRemainPhase();
            if (buff.GetRemainPhase() > 0)
            {
                newBuffList.Add(buff);
            }
        }
        buffList = newBuffList;
        
        List<Debuff> newDebuffList = new List<Debuff>();
        foreach (var debuff in debuffList)
        {
            debuff.DecreaseRemainPhase();
            if (debuff.GetRemainPhase() > 0)
            {
                newDebuffList.Add(debuff);
            }
        }
        debuffList = newDebuffList;
    }

    public void Damaged(UnitClass unitClass, int amount)
    {
        int actualDamage = 0;
        // 공격이 물리인지 마법인지 체크
        // 방어력 / 저항력 중 맞는 값을 적용
        // 방어 증가/감소 / 저항 증가/감소 적용             // FIXME : 미적용
        // 체력 깎임 
        if (unitClass == UnitClass.Melee)
        {
            // 실제 피해 = 원래 피해 x 100/(100+방어력)
            actualDamage = amount * 100 / (100 + defense);
            Debug.Log("Actual melee damage : " + actualDamage);
        }
        else if (unitClass == UnitClass.Magic)
        {
            actualDamage = amount * 100 / (100 + resistence);
            Debug.Log("Actual magic damage : " + actualDamage);
        }
        else
        {
            actualDamage = amount * 100 / (100 + Mathf.Max(defense, resistence));
        }
        
        currentHealth -= actualDamage;
        if (currentHealth < 0)
            currentHealth = 0;
    }

    public void RecoverHealth(int amount)
    {
        // 내상 효과
        if (debuffList.Any(k => k.GetName() == DebuffType.wound))
        {
            int totalDegree = 0;
            foreach (var debuff in debuffList)
            {
                if (debuff.GetName() == DebuffType.exhaust)
                {
                    totalDegree = debuff.GetDegree();
                }
            }
            amount *= (totalDegree/100);
        }
        
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

    void ApplyStats()
    {
        maxHealth = (int)baseHealth;
        power = (int)basePower;
        defense = (int)baseDefense;
        resistence = (int)baseResistence;
        dexturity = (int)baseDexturity;
        reach = (int)baseReach;
        range = (int)baseRange;
    }

    void Initialize()
    {
        currentHealth = maxHealth;
        activityPoint = (int)(dexturity * 1.5f);
        skillList = SkillLoader.MakeSkillList();
        buffList = new List<Buff>();
        debuffList = new List<Debuff>();
    }

	// Use this for initialization
	void Start () {
        ApplyStats();
        Initialize();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
