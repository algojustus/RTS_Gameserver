using System.Numerics;

namespace RtsServer;

public class BuildingData
{
    public int id;
    public string prefabname;
    public Vector3 position;
    public Quaternion rotation;
    public int building_hp;
    public int ranged_resistance;
    public int melee_resistance;
    
    public BuildingData(int _id, string _prefabname, Vector3 _spawnposition, Quaternion _spawnrota, int hitpoints, int rangedResistance, int meleeResistance)
    {
        id = _id;
        prefabname = _prefabname;
        position = _spawnposition;
        rotation = _spawnrota;
        building_hp = hitpoints;
        ranged_resistance = rangedResistance;
        melee_resistance = meleeResistance;
    }
}