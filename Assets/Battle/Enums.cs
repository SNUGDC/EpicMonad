using UnityEngine;
using System.Collections;

namespace Enums {
    
    public enum Side
    {
        Ally,
        Enemy
    }
    
    public enum TileColor // highlighting selected tile
    {
        Blue,
        Red
    }
    
    public enum TileForm
    {
        Flatland,
        Hill
    }
    
    public enum UnitClass
    {
        None,
        Melee,
        Magic
    }
    
    public enum Celestial
    {
        None,
        Sun,
        Moon,
        Earth
    }
    
    public enum Element
    {
        None,
        Fire,
        Water,
        Plant,
        Metal    
    }
    
    public enum Direction
    {
        LeftUp,
        LeftDown,
        RightUp,
        RightDown
    }
    
    public enum SkillType
    {
        Point,
        Area,
        Route
    }
    
    public enum RangeForm
    {
        Square,
        Cross,
        Diagonal
    }
    
    public enum SkillApplyType
    {
        Damage,
        Heal,
        Etc
    }
    
    public enum BuffType
    {
        None
    }
    
    public enum DebuffType
    {
        Retire,
        DamageOverPhase,
        Exhaust,
        Bind,
        Silence,
        Faint,
        Slow,
        Wound,
        PowerDecrease,
        DefenseDecrease,
        ResistanceDecrease,
        Mark,
        Poison,
        Bleed
    }
    
    public class Buff
    {
        BuffType name;
        int degree;
        int amount;
        int remainPhase;
        
        public Buff(BuffType name, int degree, int amount, int remainPhase)
        {
            this.name = name;
            this.degree = degree;
            this.amount = amount;
            this.remainPhase = remainPhase;
        }
        
        public BuffType GetName()
        {
            return name;
        }
        
        public int GetDegree()
        {
            return degree;
        }
        
        public int GetAmount()
        {
            return amount;
        }
        
        public int GetRemainPhase()
        {
            return remainPhase;   
        }
        
        public void AddRemainPhase(int phase)
        {
            remainPhase += phase;
        }
        
        public void SubRemainPhase(int phase)
        {
            remainPhase -= phase;
        }
        
        public void DecreaseRemainPhase()
        {
            remainPhase --;
        }
    }
    
    public class Debuff
    {
        DebuffType name;
        int degree;
        int amount;
        int remainPhase;
        
        public Debuff(DebuffType name, int degree, int amount, int remainPhase)
        {
            this.name = name;
            this.degree = degree;
            this.amount = amount;
            this.remainPhase = remainPhase;
        }
        
        public DebuffType GetName()
        {
            return name;
        }
        
        public int GetDegree()
        {
            return degree;
        }
        
        public int GetAmount()
        {
            return amount;
        }
        
        public int GetRemainPhase()
        {
            return remainPhase;   
        }
        
        public void AddRemainPhase(int phase)
        {
            remainPhase += phase;
        }
        
        public void SubRemainPhase(int phase)
        {
            remainPhase -= phase;
        }
        
        public void DecreaseRemainPhase()
        {
            remainPhase --;
        }
    }
}
