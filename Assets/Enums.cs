using UnityEngine;
using System.Collections;

namespace Enums {
    public enum TileForm
    {
        flatland,
        hill
    }
    
    public enum TileElement
    {
        None,
        Fire,
        Water,
        Plant,
        Metal    
    }
    
    public enum Direction
    {
        up,
        down,
        left,
        right
    }
    
    public enum RangeForm
    {
        square,
        cross,
        diagonal
    }
    
    public enum DamageType
    {
        Melee,
        Magic
    }
    
    public enum BuffType
    {
        None
    }
    
    public enum DebuffType
    {
        poison,
        bleed,
        exhaust,
        bind,
        silence,
        slow,
        wound
    }
    
    public class Buff
    {
        BuffType name;
        int degree;
        int remainTurn;
        
        public Buff(BuffType name, int degree, int remainTurn)
        {
            this.name = name;
            this.degree = degree;
            this.remainTurn = remainTurn;
        }
    }
    
    public class Debuff
    {
        DebuffType name;
        int degree;
        int remainTurn;
        
        public Debuff(DebuffType name, int degree, int remainTurn)
        {
            this.name = name;
            this.degree = degree;
            this.remainTurn = remainTurn;
        }
    }
}
