using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour {

	int standardActionPoint;

	// FIXME : unit is only one. - 현재는 해당하는 유닛의 프리펩을 직접 넣어줘야 하지만, 추후 데이터에서 읽어와서 유닛을 띄워야 함. 
	public GameObject[] unitPrefabs;
    public GameObject unitPrefab;
	public List<GameObject> units = new List<GameObject>();
	public List<GameObject> readiedUnits = new List<GameObject>();

    public List<GameObject> GetAllUnits()
    {
        return units;
    }
    
    public int GetStandardActionPoint()
    {
        return standardActionPoint;
    }
    
    public void SetStandardActionPoint(int partyLevel)
    {
        standardActionPoint = partyLevel + 60;
    }

	void GenerateUnits ()
	{
		float tileWidth = 0.7f*200/100;
		float tileHeight = 0.7f*100/100;
		
        List<UnitInfo> unitInfoList = Parser.GetParsedUnitInfo();
        
        foreach (var unitInfo in unitInfoList)
        {
            GameObject unit = Instantiate(unitPrefab) as GameObject;
            
            unit.GetComponent<Unit>().ApplyUnitInfo(unitInfo);
            
            Vector2 initPosition = unit.GetComponent<Unit>().GetInitPosition();
            Vector3 respawnPos = new Vector3(tileWidth * (initPosition.y + initPosition.x) * 0.5f, 
                                             tileHeight * (initPosition.y - initPosition.x) * 0.5f, 
                                             (initPosition.y - initPosition.x) * 0.1f - 0.01f);
			unit.transform.position = respawnPos;
			
			GameObject tileUnderUnit = FindObjectOfType<TileManager>().GetTile((int)initPosition.x, (int)initPosition.y);
			tileUnderUnit.GetComponent<Tile>().SetUnitOnTile(unit);
			
			units.Add(unit);
        }
        
		// foreach (var unitPrefab in unitPrefabs)
		// {
		// 	int x, y;
		// 	GameObject unit = Instantiate(unitPrefab) as GameObject;
        //     unit.name = unit.GetComponent<Unit>().GetNameInCode();
			
        //     x = (int)unit.GetComponent<Unit>().initPosition.x;
        //     y = (int)unit.GetComponent<Unit>().initPosition.y;
			
		// 	Vector3 respawnPos = new Vector3(tileWidth * (y + x) * 0.5f, tileHeight * (y - x) * 0.5f, (y - x) * 0.1f - 0.01f);
		// 	unit.transform.position = respawnPos;
		// 	unit.GetComponent<Unit>().SetPosition(new Vector2(x, y));
			
		// 	GameObject tileUnderUnit = FindObjectOfType<TileManager>().GetTile(x, y);
		// 	tileUnderUnit.GetComponent<Tile>().SetUnitOnTile(unit);
			
		// 	units.Add(unit);
		// }
		
		Debug.Log("Generate units complete");
	}
    
    public List<GameObject> GetUpdatedReadiedUnits()
    {
        readiedUnits.Clear();
        // check each unit and add all readied units.
		foreach (var unit in units)
		{
			if (unit.GetComponent<Unit>().GetCurrentActivityPoint() >= standardActionPoint)
			{
				readiedUnits.Add(unit);
				Debug.Log(unit.GetComponent<Unit>().name + " is readied");
			}
		}
        
        // AP가 큰 순서대로 소팅.
        readiedUnits.Sort(delegate(GameObject x, GameObject y)
        {
            if (x.GetComponent<Unit>() == null && y.GetComponent<Unit>() == null) return 0;
            else if (y.GetComponent<Unit>() == null) return -1;
            else if (x.GetComponent<Unit>() == null) return 1;
            else return CompareByActionPoint(x, y);
        });
        
        return readiedUnits;
    }

	public void EndPhase()
	{
        // Decrease each buff & debuff phase
        foreach (var unit in units)
            unit.GetComponent<Unit>().DecreaseRemainPhaseBuffAndDebuff();
        
        foreach (var unit in units)
            unit.GetComponent<Unit>().RegenerateActionPoint();
	}
    
    int CompareByActionPoint(GameObject unit, GameObject anotherUnit)
    {
        int compareResultByCurrentActionPoint = anotherUnit.GetComponent<Unit>().GetCurrentActivityPoint().CompareTo(unit.GetComponent<Unit>().GetCurrentActivityPoint());
        if (compareResultByCurrentActionPoint == 0)
        {
            int compareResultByTrueDexturity = anotherUnit.GetComponent<Unit>().GetTrueDexturity().CompareTo(unit.GetComponent<Unit>().GetTrueDexturity());
            if (compareResultByTrueDexturity == 0)
                return anotherUnit.GetInstanceID().CompareTo(unit.GetInstanceID());
            else
                return compareResultByTrueDexturity;
        }
        else
            return compareResultByCurrentActionPoint;
    }

	// Use this for initialization
	void Start () {
		GenerateUnits ();
	}
	
	// Update is called once per frame
	void Update () {
	   // 유닛 전체에 대해서도 소팅. 변경점이 있을때마다 반영된다.
        units.Sort(delegate(GameObject x, GameObject y)
        {
            if (x.GetComponent<Unit>() == null && y.GetComponent<Unit>() == null) return 0;
            else if (y.GetComponent<Unit>() == null) return -1;
            else if (x.GetComponent<Unit>() == null) return 1;
            else return CompareByActionPoint(x, y);
        });
	}
}
