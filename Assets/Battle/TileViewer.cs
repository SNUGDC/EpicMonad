using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Enums;

public class TileViewer : MonoBehaviour {

    Image tileImage;
    Text nameText;
    Text apText;
    Image elementImage;

    public void UpdateTileViewer(GameObject tile)
    {
        Tile tileInfo = tile.GetComponent<Tile>();
        tileImage.sprite = tile.GetComponent<SpriteRenderer>().sprite;
        nameText.text = tileInfo.GetTileName();
        apText.text = "AP " + tileInfo.GetRequireAPAtTile();
        SetElementImage(tileInfo.GetTileElement());
    }

    void SetElementImage(Element element)
    {
        if (element == Element.Fire)
            elementImage.sprite = Resources.Load("Icon/fire", typeof(Sprite)) as Sprite;
        else if (element == Element.Water)
            elementImage.sprite = Resources.Load("Icon/water", typeof(Sprite)) as Sprite;
        else if (element == Element.Plant)
            elementImage.sprite = Resources.Load("Icon/plant", typeof(Sprite)) as Sprite;
        else if (element == Element.Metal)
            elementImage.sprite = Resources.Load("Icon/metal", typeof(Sprite)) as Sprite;
        else
            elementImage.sprite = Resources.Load("Icon/transparent", typeof(Sprite)) as Sprite;
    }

	// Use this for initialization
	void Start () {
	   tileImage = transform.Find("TileImage").GetComponent<Image>();
       nameText = transform.Find("NameText").GetComponent<Text>();
       apText = transform.Find("APText").GetComponent<Text>();
       elementImage = transform.Find("ElementImage").GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
