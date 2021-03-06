﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Enums;

// This component is used in two UI.
// SelectedUnitViewer and UnitViewer.
public class UnitViewer : MonoBehaviour {

	TileManager tileManager;

	Image unitImage;
	Text nameText;
	Image classImage;
	Image elementImage;
	GameObject elementBuffIcon;
	GameObject elementDebuffIcon;
	Image celestialImage;
	GameObject celestialBuffIcon;
	GameObject celestialDebuffIcon;

	Text hpText;

	Text apText;

	Text powerText;
	Text defenseText;
	Text resistanceText;
	// FIXME : 버프/디버프는 아직 미구현.

	public void UpdateUnitViewer(GameObject unitObject)
	{
		Unit unit = unitObject.GetComponent<Unit>();
		unitImage.sprite = unit.GetDefaultSprite();
		nameText.text = unit.GetName();
		SetClassImage(unit.GetUnitClass());
		SetElementImage(unit.GetElement());
		CheckElementBuff(unit);
		SetCelestialImage(unit.GetCelestial());
		UpdateHp(unit);
		UpdateAp(unit);
		UpdatePower(unit);
		UpdateDefense(unit);
		UpdateResistance(unit);
	}

	void CheckElementBuff(Unit unit)
	{
		elementBuffIcon.SetActive(false);
		elementDebuffIcon.SetActive(false);

		if (unit.GetElement() == tileManager.GetTile(unit.GetPosition()).GetComponent<Tile>().GetTileElement())
		{
			elementBuffIcon.SetActive(true);
		}
	}

	void UpdateHp(Unit unit)
	{
		hpText.text = unit.GetCurrentHealth() + " / " + unit.GetMaxHealth();
	}

	void UpdateAp(Unit unit)
	{
		apText.text = unit.GetCurrentActivityPoint() + " (+" + unit.GetActualDexturity() + ")";
	}

	void UpdatePower(Unit unit)
	{
		powerText.text = unit.GetActualPower().ToString();
	}

	void UpdateDefense(Unit unit)
	{
		defenseText.text = unit.GetActualDefense().ToString();
	}

	void UpdateResistance(Unit unit)
	{
		resistanceText.text = unit.GetActualResistance().ToString();
	}

	void SetClassImage(UnitClass unitClass)
	{
		if (unitClass == UnitClass.Melee)
			classImage.sprite = Resources.Load("Icon/meleeClass", typeof(Sprite)) as Sprite;
		else if (unitClass == UnitClass.Magic)
			classImage.sprite = Resources.Load("Icon/magicClass", typeof(Sprite)) as Sprite;
		else
			classImage.sprite = Resources.Load("Icon/transparent", typeof(Sprite)) as Sprite;
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

	void SetCelestialImage(Celestial celestial)
	{
		if (celestial == Celestial.Sun)
			celestialImage.sprite = Resources.Load("Icon/sun", typeof(Sprite)) as Sprite;
		else if (celestial == Celestial.Moon)
			celestialImage.sprite = Resources.Load("Icon/moon", typeof(Sprite)) as Sprite;
		else if (celestial == Celestial.Earth)
			celestialImage.sprite = Resources.Load("Icon/earth", typeof(Sprite)) as Sprite;
		else
			celestialImage.sprite = Resources.Load("Icon/transparent", typeof(Sprite)) as Sprite;
	}

	void Awake () {
		tileManager = FindObjectOfType<TileManager>();

		unitImage = transform.Find("UnitImage").GetComponent<Image>();
		nameText = transform.Find("NameText").GetComponent<Text>();
		classImage = transform.Find("ClassImage").GetComponent<Image>();

		elementImage = transform.Find("ElementImage").GetComponent<Image>();
		elementBuffIcon = transform.Find("ElementImage").Find("BuffImage").gameObject;
		elementDebuffIcon = transform.Find("ElementImage").Find("DebuffImage").gameObject;
		elementBuffIcon.SetActive(false);
		elementDebuffIcon.SetActive(false);

		celestialImage = transform.Find("CelestialImage").GetComponent<Image>();
		celestialBuffIcon = transform.Find("CelestialImage").Find("BuffImage").gameObject;
		celestialDebuffIcon = transform.Find("CelestialImage").Find("DebuffImage").gameObject;
		celestialBuffIcon.SetActive(false);
		celestialDebuffIcon.SetActive(false);

		hpText = transform.Find("HP").Find("HPText").GetComponent<Text>();
		apText = transform.Find("AP").Find("APText").GetComponent<Text>();

		powerText = transform.Find("Power").Find("PowerText").GetComponent<Text>();
		defenseText = transform.Find("Defense").Find("DefenseText").GetComponent<Text>();
		resistanceText = transform.Find("Resistance").Find("ResistanceText").GetComponent<Text>();
	}
}
