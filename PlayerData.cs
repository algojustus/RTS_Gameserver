﻿using System.Numerics;

namespace RtsServer;

public class PlayerData
{
    public Dictionary<int, UnitData> UnitDictionary;
    public Dictionary<int, BuildingData> BuildingDictionary;

    public int id;
    public string username;
    public int color;

    public PlayerData(int _id, string _username, int _color)
    {
        id = _id;
        username = _username;
        color = _color;
        UnitDictionary = new Dictionary<int, UnitData>();
        BuildingDictionary = new Dictionary<int, BuildingData>();
    }

    public void AddBuilding(int receiver_id, int building_id, string prefab_name, Vector3 spawnpos,
        Quaternion spawnrota, int hitpoints,
        int rangedResistance, int meleeResistance)
    {
        BuildingData building = new BuildingData(building_id, prefab_name, spawnpos, spawnrota, hitpoints,
            rangedResistance,
            meleeResistance);
        BuildingDictionary.Add(building_id, building);
        MessageSend.SendBuildingCreated(receiver_id, building);
    }

    public void DestroyBuilding(int building_id)
    {
        BuildingDictionary.Remove(building_id);
    }

    public void AddUnit(int receiver_id, int unit_id, string prefab_name, Vector3 spawnpos, Quaternion spawnrota,
        int hitpoints,
        int _damage, int rangedResistance, int meleeResistance)
    {
        UnitData unit = new UnitData(unit_id, prefab_name, spawnpos, spawnrota, hitpoints, _damage, rangedResistance,
            meleeResistance);
        UnitDictionary.Add(unit_id, unit);
        MessageSend.SendUnitCreated(receiver_id, unit);
    }

    public void DestroyUnit(int unit_id)
    {
        UnitDictionary.Remove(unit_id);
    }

    public void DamageUnit(int unit_id, int damage)
    {
        UnitData unit = UnitDictionary[unit_id];
        unit.unit_hp -= damage;
        if (unit.unit_hp < 0)
            DestroyUnit(unit_id);
    }

    public void DamageBuilding(int building_id, int damage)
    {
        BuildingData building = BuildingDictionary[building_id];
        building.building_hp -= damage;
        if (building.building_hp < 0)
            DestroyBuilding(building_id);
    }

    public void UpdateUnitPosition(int receiver_id, int unit_id, Vector3 pos)
    {
        Console.WriteLine("Unit pos received: {0}", pos);
        UnitData unit = UnitDictionary[unit_id];
        unit.position = pos;
        MessageSend.SendUnitPos(receiver_id, unit_id, pos);
    }
}