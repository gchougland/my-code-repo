
using System.Collections.Generic;
using System.Runtime.Serialization;

[System.Serializable]
public class Room
{
    public bool north, south, east, west;
    public int[,] Tiles;
    public int width, height;
    public List<EntityData> entities;
}

[DataContract]
public enum EntityType
{
    [EnumMember(Value="Pickup")]
    Pickup,
    [EnumMember(Value="Enemy")]
    Enemy,
    [EnumMember(Value="ShopItem")]
    ShopItem
}

[System.Serializable]
public class EntityData
{
    public EntityType type;
    public int xPos, yPos;
    public int id;
}