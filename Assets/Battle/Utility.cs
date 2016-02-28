using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class Utility : MonoBehaviour {

    public static float GetDegreeToTarget(GameObject unit, Vector2 targetPosition)
    {
        Vector2 unitPosition = unit.GetComponent<Unit>().GetPosition();
        
        Debug.LogWarning("target : " + targetPosition + ", unit : " + unitPosition);
        
        float deltaDegree = Mathf.Atan2(targetPosition.y - unitPosition.y, targetPosition.x - unitPosition.x) * Mathf.Rad2Deg;
        
        return deltaDegree;
    }
    
    public static Direction GetDirectionToTarget(GameObject unit, List<GameObject> selectedTiles)
    {
        Vector2 averagePos = new Vector2(0, 0);
        foreach (var tile in selectedTiles)
        {
            averagePos += tile.GetComponent<Tile>().GetTilePos();
        }
        averagePos = averagePos / (float)selectedTiles.Count;
        
        return GetDirectionToTarget(unit, averagePos);
    }
    
    public static Direction GetDirectionToTarget(GameObject unit, Vector2 targetPosition)
    {
        float deltaDegree = GetDegreeToTarget(unit, targetPosition);
        
        if ((-45 < deltaDegree) && (deltaDegree <= 45)) return Direction.RightDown;
        else if ((45 < deltaDegree) && (deltaDegree <= 135)) return Direction.RightUp;
        else if ((-135 < deltaDegree) && (deltaDegree <= -45)) return Direction.LeftDown;
        else if ((deltaDegree <= -135) || (135 < deltaDegree)) return Direction.LeftUp;
        
        
        else 
        {
            Debug.LogWarning("Result degree : " + deltaDegree);
            return Direction.RightUp;    
        }
    }
    
    // public static float GetDegreeAtAttack(Game)
}
