using UnityEngine;
using System.Collections;
using System;
using Enums;

public class UnitInfo {
	public string name;
    public string nameInCode;
    public Vector2 initPosition;
    public int baseHealth;
    public int basePower;
    public int baseDefense;
    public int baseResistence;
    public int baseDexturity;
    public int baseReach;
    public int baseRange;
    public UnitClass unitClass;
    public Element element;
    public Celestial celestial;
    
    public UnitInfo (string data)
	{
		string[] stringList = data.Split(',');

        this.name = stringList[0];
        this.nameInCode = stringList[1];
        this.initPosition = new Vector2(Int32.Parse(stringList[2]), Int32.Parse(stringList[3]));
        this.baseHealth = Int32.Parse(stringList[4]);
        this.basePower = Int32.Parse(stringList[5]);
        this.baseDefense = Int32.Parse(stringList[6]);
        this.baseResistence = Int32.Parse(stringList[7]);
        this.baseDexturity = Int32.Parse(stringList[8]);
		this.unitClass = (UnitClass)Enum.Parse(typeof(UnitClass), stringList[9]);
		this.element = (Element)Enum.Parse(typeof(Element), stringList[10]);
		this.celestial = (Celestial)Enum.Parse(typeof(Celestial), stringList[11]);
	}
}
