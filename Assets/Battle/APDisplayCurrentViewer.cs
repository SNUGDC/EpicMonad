using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class APDisplayCurrentViewer : MonoBehaviour {

    public GameObject portraitPrefab;

    List<GameObject> portraits;

    public void UpdateAPDisplay(List<GameObject> units)
    {
        ClearViewer();
        
        int count = 0;
        foreach (var unit in units)
        {
            bool isFirst = (count == 0);
            GameObject portrait = Instantiate(portraitPrefab) as GameObject;
            string imagePath = "UnitImage/portrait_" + unit.GetComponent<Unit>().GetUnitName().ToString();
            portrait.GetComponent<Image>().sprite = Resources.Load(imagePath, typeof(Sprite)) as Sprite;
            
            portraits.Add(portrait);
            
            portrait.transform.SetParent(GameObject.Find("APDisplayCurrentPanel").transform);
            portrait.transform.localPosition = new Vector3 (120 + 40 * count, -20, 0); // ?? 왜 y좌표 반영이 이상하게 되는것인가..
            portrait.transform.localScale = new Vector3 (1, 1, 1);
            
            // 첫번째 아이콘 크게.
            if (isFirst)
            {
                portrait.transform.localPosition = new Vector3 (100, -10, 0);
                portrait.transform.localScale = new Vector3 (1.5f, 1.5f, 1);    
            }
            
            ActiveBorder(unit, portrait, isFirst);
           
            count ++;
        }
    }
    
    void ActiveBorder(GameObject unit, GameObject portrait, bool isFirst)
    {
        int standardActionPoint = FindObjectOfType<UnitManager>().GetStandardActionPoint();
        int unitActionPoint = unit.GetComponent<Unit>().GetCurrentActivityPoint();
        
        if (isFirst)
        {
            portrait.transform.Find("GreenBorder").gameObject.SetActive(false);
        }
        else if (unitActionPoint >= standardActionPoint)
        {
            portrait.transform.Find("RedBorder").gameObject.SetActive(false);
        }
        else
        {
            portrait.transform.Find("GreenBorder").gameObject.SetActive(false);
            portrait.transform.Find("RedBorder").gameObject.SetActive(false);
        }
    }

    void ClearViewer()
    {
        int length = portraits.Count;
        for (int i = 0; i < length; i++)
        {
            Destroy(portraits[i]);
        }   
    }

	// Use this for initialization
	void Start () {
	   portraits = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
