using UnityEngine;
using System.Collections;
using Enums;

public class TileInfo {
    Vector2 tilePosition;
    TileForm tileForm;
    Element tileElement;
    bool isEmptyTile;
    
    public Vector2 GetTilePosition() { return tilePosition; }
    public TileForm GetTileForm() { return tileForm; }
    public Element GetTileElement() { return tileElement; }
    public bool IsEmptyTile() { return isEmptyTile; }    
    
    public TileInfo(Vector2 tilePosition, string tileInfoString)
    {    
        char tileInfoChar = tileInfoString[0];    
        this.tilePosition = tilePosition;
        
        if (tileInfoChar == '-')
        {
            this.isEmptyTile = true;
            return;    
        }
        else if (tileInfoChar == 'F')
        {
            this.isEmptyTile = false;
            this.tileForm = TileForm.Flatland;
            this.tileElement = Element.Plant;
        }
        else if (tileInfoChar == 'H')
        {
            this.isEmptyTile = false;
            this.tileForm = TileForm.Hill;
            this.tileElement = Element.None;
        }
        else
        {
            Debug.LogError("Undefined tileInfo: <" + tileInfoChar + ">" + " at " + tilePosition);
        }
    } 
}
