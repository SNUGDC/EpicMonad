using UnityEngine;
using System.Collections;

namespace Enums {
    
    
    
    public enum TileColor // highlighting selected tile
    {
        blue,
        red
    }
    
    public enum TileForm
    {
        flatland,
        hill
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
        up,
        down,
        left,
        right
    }
    
    public enum SkillType
    {
        point,
        area,
        route
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
        faint,
        silence,
        slow,
        wound
    }
    
    public class Buff
    {
        BuffType name;
        int degree;
        int remainPhase;
        
        public Buff(BuffType name, int degree, int remainPhase)
        {
            this.name = name;
            this.degree = degree;
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
        int remainPhase;
        
        public Debuff(DebuffType name, int degree, int remainPhase)
        {
            this.name = name;
            this.degree = degree;
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
