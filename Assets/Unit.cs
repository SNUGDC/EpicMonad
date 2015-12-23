using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

	// FIXME : public -> private
	public new string name;
	public Vector2 position;
	public int actionPoint;
	public int regenerationActionPoint;

	public int GetActionPoint()
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
