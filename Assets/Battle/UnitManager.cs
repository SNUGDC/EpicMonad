using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour {

	int standardActionPoint = 100; // FIXME : stdAP는 파티 레벨 + 60

	// FIXME : unit is only one. - 현재는 해당하는 유닛의 프리펩을 직접 넣어줘야 하지만, 추후 데이터에서 읽어와서 유닛을 띄워야 함. 
	public GameObject[] unitPrefabs;
	public List<GameObject> units = new List<GameObject>();
	public List<GameObject> readiedUnits = new List<GameObject>();

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
			
            x = (int)unit.GetComponent<Unit>().initPosition.x;
            y = (int)unit.GetComponent<Unit>().initPosition.y;
			
			Vector3 respawnPos = new Vector3(tileWidth * (y + x) * 0.5f, tileHeight * (y - x) * 0.5f, (y - x) * 0.1f - 0.01f);
			unit.transform.position = respawnPos;
			unit.GetComponent<Unit>().SetPosition(new Vector2(x, y));
			
			GameObject tileUnderUnit = FindObjectOfType<TileManager>().GetTile(x, y);
			tileUnderUnit.GetComponent<Tile>().SetUnitOnTile(unit);
			
			units.Add(unit);
		}
		
		Debug.Log("Generate units complete");
	}

	public void EndPhase()
	{
		// check each unit and enqueue all readied units.
		foreach (var unit in units)
		{
			unit.GetComponent<Unit>().RegenerateActionPoint();
			if (unit.GetComponent<Unit>().GetCurrentActivityPoint() >= standardActionPoint)
			{
				readiedUnits.Add(unit);
				Debug.Log(unit.GetComponent<Unit>().name + " is readied");
			}
		}
        
        // FIXME : AP가 큰 순서대로 소팅. - 추가할 것.
	}

	// Use this for initialization
	void Start () {
		GenerateUnits ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
