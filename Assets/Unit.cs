using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

	// FIXME : public -> private
	public new string name;
    
    // Base stats.
    float baseHealth; //체력 
    float basePower; //공격력
    float baseDefense; //방어력
    float baseResistence; //저항력
    float baseDexturity; //행동력
    float baseReach; //거리
    float baseRange; //범위
    
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
    public int currentHealth; 
	public int activityPoint;
	public int regenerationActionPoint;

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
		activityPoint += regenerationActionPoint;
		Debug.Log(name + " recover " + regenerationActionPoint + "AP. Current AP : " + activityPoint);
	}

	public void UseActionPoint(int amount)
	{
		activityPoint -= amount;
		Debug.Log(name + " use " + amount + "AP. Current AP : " + activityPoint);
	}

	// Use this for initialization
	void Start () {
		activityPoint = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
