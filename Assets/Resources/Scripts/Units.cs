using UnityEngine;
using System.Collections.Generic;

public class Weapon
{
    int Damage { get; set; }
    int Range { get; set; }
    int Speed { get; set; }
    string Name { get; set; }
}

public class Tool
{
    public enum ToolType
    {
        Axe,
        FishingRod,
        Hoe,
        Shovel,
    }
    ToolType Type { get; set; }
    int Damage { get; set; }
    int Range { get; set; }
    int Speed { get; set; }
    string Name { get; set; }
}

public class Armour
{
    int Protection { get; set; }
    int ReduceSpeed { get; set; }
    string Name { get; set; }
}

public struct UnitDetail
{
    public enum UnitType
    {
        Villager,
        Warrior,
        Mage,
        Archer,
        Rogue,
    }
    public UnitType Type { get; set; }

    public int DefHealth { get; set; }
    public int Health { get; set; }
    public int DefSpeed { get; set; }
    public int Speed { get; set; }

    public Weapon Weapon { get; set; }
    public Tool Tool { get; set; }
    public Armour Armour { get; set; }
    public string Name { get; set; }
}

public static class UnitDetailFactory
{
    public static UnitDetail MakeVillager()
    {
        return new UnitDetail
        {
            DefHealth = 60,
            DefSpeed = 5,
            Health = 60,
            Speed = 5,
            Name = "Villager",
            Type = UnitDetail.UnitType.Villager,
            Weapon = null,
            Armour = null,
            Tool = null
        };
    }

    public static UnitDetail MakeWarrior()
    {
        return new UnitDetail
        {
            DefHealth = 120,
            Health = 120,
            DefSpeed = 4,
            Speed = 4,
            Name = "Warrior",
            Type = UnitDetail.UnitType.Warrior,
            Weapon = null,
            Armour = null,
            Tool = null
        };
    }
}

public static class UnitObjectFactory
{
    public static GameObject MakeVillager(GameLogic game, int x, int y)
    {
        GameObject go = Object.Instantiate(
                Resources.Load("Prefabs/Villager"),
                new Vector3(x, 0, y),
                Quaternion.identity)
            as GameObject;

        var unit = go.AddComponent<Unit>();
        unit.Init(
            game,
            UnitDetailFactory.MakeVillager(),
            new VillagerAI(unit),
            x, 
            y
        );

        return go;
    }

    public static GameObject MakeWarrior(GameLogic game, int x, int y)
    {
        var go = new GameObject("Warrior");

        var unit = go.AddComponent<Unit>();
        unit.Init(
            game,
            UnitDetailFactory.MakeWarrior(),
            new WarriorAI(unit),
            x, 
            y
        );

        return go;
    }
}

public class Unit : MonoBehaviour
{
    public GameLogic Game { get; set; }
    public UnitDetail Details { get; set; }
    public IUnitAI AIStrategy { get; set; }

    public int TicksTilNextAction;

    public int X { get; private set; }
    public int Y { get; private set; }

    public Transform Target;
    public Stack<MapTile> Path;

    void Start()
    {        
    }

    public void Init(GameLogic game, UnitDetail details, IUnitAI ai, int x, int y)
    {
        Game = game;
        Details = details;
        AIStrategy = ai;
        X = x;
        Y = y;
    }
    
    public void OnTick()
    {
        if (TicksTilNextAction > 0)
            TicksTilNextAction--;

        if (TicksTilNextAction <= 0)
            AIStrategy.NextAction();
    }

    public void Move(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public interface IUnitAI
{
    void NextAction();
}

public class VillagerAI : IUnitAI
{    
    private Unit unit;

    public VillagerAI(Unit unit)
    {
        this.unit = unit;
    }

    public void NextAction()
    {
        unit.TicksTilNextAction = unit.Details.Speed;
    }
}

public class WarriorAI : IUnitAI
{
    private Unit unit;

    public WarriorAI(Unit unit)
    {
        this.unit = unit;
    }

    public void NextAction()
    {
        unit.TicksTilNextAction = unit.Details.Speed;
    }
}
