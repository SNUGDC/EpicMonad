using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour {

	public int standardActionPoint = 50;

	// FIXME : unit is only one.
	public GameObject[] unitPrefabs;
	public List<GameObject> units = new List<GameObject>();
	public Queue<GameObject> readiedUnits = new Queue<GameObject>();

    public List<GameObject> GetAllUnits()
    {
        return units;
    }

	void GenerateUnits ()
	{
		int tileWidth = 200/100;
		int tileHeight = 100/100;
		
		foreach (var unitPrefab in unitPrefabs)
		{
			// int x = Random.Range(0, 10);
			// int y = Random.Range(0, 10);
			int x, y;
			GameObject unit = Instantiate(unitPrefab) as GameObject;
			
			// Using test.
			if (unit.GetComponent<Unit>().name == "Jade")
			{
				x = 8;
				y = 3;
			}
			else if (unit.GetComponent<Unit>().name == "Pale")
			{
				x = 5;
				y = 3;
			}
            else
            {
                x = 1;
                y = 1;
            }
			
			Vector3 respawnPos = new Vector3(tileWidth * (y + x) * 0.5f, tileHeight * (y - x) * 0.5f, (y - x) * 0.1f - 0.01f);
			unit.transform.position = respawnPos;
			unit.GetComponent<Unit>().SetPosition(new Vector2(x, y));
			
			GameObject tileUnderUnit = FindObjectOfType<TileManager>().GetTile(x, y);
			tileUnderUnit.GetComponent<Tile>().SetUnitOnTile(unit);
			
			units.Add(unit);
		}
		
		Debug.Log("Generate units complete");
	}

	public void EndTurn()
	{
		// check each unit and enqueue all readied units.
		foreach (var unit in units)
		{
			unit.GetComponent<Unit>().RegenerateActionPoint();
			if (unit.GetComponent<Unit>().GetCurrentActivityPoint() >= standardActionPoint)
			{
				readiedUnits.Enqueue(unit);
				Debug.Log(unit.GetComponent<Unit>().name + " is readied");
			}
		}
	}

	// Use this for initialization
	void Start () {
		GenerateUnits ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
