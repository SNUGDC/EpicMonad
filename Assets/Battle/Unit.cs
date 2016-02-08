using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;
using System.Linq;

public class Unit : MonoBehaviour {

	// FIXME : public -> private
	public new string name; // 한글이름
    public string nameInCode; // 영어이름 
    
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

    // FIXME : 임시로 공격력만 외부에서 참조.
    public int GetActualPower()
    {
        int actualPower = power;
        
        // 공격력 감소 효과 적용.
        if (debuffList.Any(k => k.GetName() == DebuffType.PowerDecrease))
        {
            // 상대치 곱연산 
            float totalDegree = 1.0f;
            foreach (var debuff in debuffList)
            {
                if (debuff.GetName() == DebuffType.PowerDecrease)
                {
                    totalDegree *= (100.0f - debuff.GetDegree())/100.0f;
                }
            }
            actualPower = (int)((float)actualPower * totalDegree);  
            
            // 절대치 합연산 
            int totalAmount = 0;
            foreach (var debuff in debuffList)
            {
                if (debuff.GetName() == DebuffType.PowerDecrease)
                {
                    totalAmount += debuff.GetAmount();
                }
            }
            actualPower -= totalAmount;  
        }

        return actualPower;
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
    
    public bool IsBound()
    {
        return debuffList.Any(k => k.GetName() == DebuffType.Bind);
    }
    
    public bool IsSilenced()
    {
        return debuffList.Any(k => k.GetName() == DebuffType.Silence);
    }
    
    public bool IsFainted()
    {
        return debuffList.Any(k => k.GetName() == DebuffType.Bind) &&
               debuffList.Any(k => k.GetName() == DebuffType.Silence);
    }
    
    public int GetTrueDexturity()
    {
        return dexturity;
    }
    
    public int GetActualDexturity()
    {
        int actualDexturity = dexturity;
        // FIXME : 버프 / 디버프 값 적용
        
        // 디버프값만 적용.
        if (debuffList.Any(k => k.GetName() == DebuffType.Faint))
        {
            actualDexturity = 0;
        }
        else if (debuffList.Any(k => k.GetName() == DebuffType.Exhaust))
        {
            // 상대치 곱연산 
            float totalDegree = 1.0f;
            foreach (var debuff in debuffList)
            {
                if (debuff.GetName() == DebuffType.ResistanceDecrease)
                {
                    totalDegree *= (100.0f - debuff.GetDegree())/100.0f;
                }
            }
            actualDexturity = (int)((float)actualDexturity * totalDegree);
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

    public string GetNameInCode()
    {
        return nameInCode;
    }

    public string GetName()
    {
        return name;
    }

	public void SetName(string name)
	{
		this.name = name;
	}
	
    public void SetDirection(Direction direction)
    {
        this.direction = direction;
    }
    
    public Direction GetDirection()
    {
        return direction;
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
        // 침묵이나 기절상태가 될 경우 체인 해제.
        // FIXME : 행동불능 / 넉백 추가할 것. (넉백은 디버프가 아니라서 다른 곳에서 적용할 듯?)
        if (debuff.GetName() == DebuffType.Faint ||
            debuff.GetName() == DebuffType.Silence)
        {
            ChainList.RemoveChainsFromUnit(gameObject);
        }
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
    
    public int GetActualDefense()
    {
        int actualDefense = defense;
        
        // 방어력 감소 효과 적용.
        if (debuffList.Any(k => k.GetName() == DebuffType.DefenseDecrease))
        {
            // 상대치 곱연산 
            float totalDegree = 1.0f;
            foreach (var debuff in debuffList)
            {
                if (debuff.GetName() == DebuffType.DefenseDecrease)
                {
                    totalDegree *= (100.0f - debuff.GetDegree())/100.0f;
                }
            }
            actualDefense = (int)((float)actualDefense * totalDegree);  
            
            // 절대치 합연산 
            int totalAmount = 0;
            foreach (var debuff in debuffList)
            {
                if (debuff.GetName() == DebuffType.DefenseDecrease)
                {
                    totalAmount += debuff.GetAmount();
                }
            }
            actualDefense -= totalAmount;  
        }

        return actualDefense;
    }

    public int GetActualResistance()
    {
        int actualResistance = resistence;
        
        // 저항력 감소 효과 적용. 
        if (debuffList.Any(k => k.GetName() == DebuffType.ResistanceDecrease))
        {
            // 상대치 곱연산 
            float totalDegree = 1.0f;
            foreach (var debuff in debuffList)
            {
                if (debuff.GetName() == DebuffType.ResistanceDecrease)
                {
                    totalDegree *= (100.0f - debuff.GetDegree())/100.0f;
                }
            }
            actualResistance = (int)((float)actualResistance * totalDegree);
            
            // 절대치 합연산 
            int totalAmount = 0;
            foreach (var debuff in debuffList)
            {
                if (debuff.GetName() == DebuffType.ResistanceDecrease)
                {
                    totalAmount += debuff.GetAmount();
                }
            }
            actualResistance -= totalAmount;
        }

        return actualResistance;
    }

    public void Damaged(UnitClass unitClass, int amount)
    {
        int actualDamage = 0;
        // 공격이 물리인지 마법인지 체크
        // 방어력 / 저항력 중 맞는 값을 적용
        // 방어 증가/감소 / 저항 증가/감소 적용             // FIXME : 증가분 미적용 
        // 체력 깎임 
        if (unitClass == UnitClass.Melee)
        {
            // 실제 피해 = 원래 피해 x 200/(200+방어력)
            actualDamage = amount * 200 / (200 + GetActualDefense());
            Debug.Log("Actual melee damage : " + actualDamage);
        }
        else if (unitClass == UnitClass.Magic)
        {
            actualDamage = amount * 200 / (200 + GetActualResistance());
            Debug.Log("Actual magic damage : " + actualDamage);
        }
        else if (unitClass == UnitClass.None)
        {
            actualDamage = amount;
        }
        
        currentHealth -= actualDamage;
        if (currentHealth < 0)
            currentHealth = 0;
    }

    public void ApplyDamageOverPhase()
    {
        int totalAmount = 0;
        
        if (debuffList.Any(k => k.GetName() == DebuffType.DamageOverPhase))
        {
            foreach (var debuff in debuffList)
            {
                if (debuff.GetName() == DebuffType.DamageOverPhase)
                {
                    totalAmount += debuff.GetAmount();
                }
            }
            
            // FIXME : 도트데미지는 물뎀인가 마뎀인가? 현재는 트루뎀.
            Damaged(UnitClass.None, totalAmount);
        }
    }

    public void RecoverHealth(int amount)
    {
        // FIXME : 치유량 증가 효과
         
        // 내상 효과
        if (debuffList.Any(k => k.GetName() == DebuffType.Wound))
        {
            // 상대치 곱연산 
            float totalDegree = 1.0f;
            foreach (var debuff in debuffList)
            {
                if (debuff.GetName() == DebuffType.Exhaust)
                {
                    totalDegree *= (100.0f - debuff.GetDegree())/100.0f;
                }
            }
            amount = (int)((float)amount * totalDegree);
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
