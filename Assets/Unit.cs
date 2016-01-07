using UnityEngine;
using System.Collections;
using Enums;

public class Unit : MonoBehaviour {

	// FIXME : public -> private
	public new string name;
    
    // Base stats.
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
    
    // Variable values.
	public Vector2 position;
    public Direction direction;
    public int currentHealth; 
	public int activityPoint;
	// public int regenerationActionPoint;

    public int GetMaxHealth()
    {
        return maxHealth;
    }

	public int GetCurrentActivityPoint()
	{
		return activityPoint;
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

    public void RecoverHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

	public void RegenerateActionPoint()
	{
		activityPoint += dexturity; // 페이즈당 행동력 회복량 = 민첩성
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
        activityPoint = 0;
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
