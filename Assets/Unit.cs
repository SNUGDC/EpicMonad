using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

	// FIXME : public -> private
	public new string name;
    
    // Base stats.
    float baseHp; //체력 
    float baseAttackDamage; //공격력
    float baseDefense; //방어력
    float baseResist; //저항력
    float baseSpeed; //행동력
    float baseRange; //거리
    float baseArea; //범위
    
    // Applied stats.
    int maxHp;
    int attackDamage;
    int defense;
    int resist;
    int speed;
    int range;
    int area;
    
    // Variable values.
	public Vector2 position;
    public int currentHp; 
	public int actionPoint;
	public int regenerationActionPoint;

    public int GetMaxHp()
    {
        return maxHp;
    }

	public int GetCurrentActionPoint()
	{
		return actionPoint;
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

    public void RecoverHp(int amount)
    {
        currentHp += amount;
        if (currentHp > maxHp)
            currentHp = maxHp;
    }

	public void RegenerateActionPoint()
	{
		actionPoint += regenerationActionPoint;
		Debug.Log(name + " recover " + regenerationActionPoint + "AP. Current AP : " + actionPoint);
	}

	public void UseActionPoint(int amount)
	{
		actionPoint -= amount;
		Debug.Log(name + " use " + amount + "AP. Current AP : " + actionPoint);
	}

	// Use this for initialization
	void Start () {
		actionPoint = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
