using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[Serializable]
public class TileInfo
{
    public char key;
    public TileBase tile;
    public int tileID;
}

public class MultiDimensionalArrayConverter : JsonConverter
{
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Read the JSON data
        JArray jsonArray = JArray.Load(reader);

        // Get dimensions of the 2D array
        int rows = jsonArray.Count;
        int cols = jsonArray[0].Count();

        // Create a new 2D array
        int[,] result = new int[rows, cols];

        // Populate the array
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = (jsonArray[i][j] ?? -1).Value<int>();
            }
        }

        return result;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType.IsArray && objectType.GetArrayRank() > 1;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var array = (Array)value;
        System.Diagnostics.Debug.Assert(array != null, nameof(array) + " != null");
        var rows = array.GetLength(0);
        var cols = array.GetLength(1);

        writer.WriteStartArray();
        for (int i = 0; i < rows; i++)
        {
            writer.Formatting = Formatting.Indented;
            writer.WriteStartArray();
            writer.Formatting = Formatting.None;
            for (int j = 0; j < cols; j++)
            {
                int val = (int)array.GetValue(i, j);
                if(val != -1 && val < 10)
                    writer.WriteWhitespace(" ");
                serializer.Serialize(writer, array.GetValue(i, j));
            }
            writer.WriteEndArray();
        }
        writer.Formatting = Formatting.Indented;
        writer.WriteEndArray();
    }
}

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private TextAsset[] roomTemplate;
    public  TileInfo[] tiles;
    public TileBase tempTile;
    public TileBase[] allTiles;
    [SerializeField] private AssetReference _startRoom;
    private AsyncOperationHandle<TextAsset> _roomLoadOpHandle;
    

    readonly JsonSerializerSettings _jsonSettings = new()
    {
        Converters = { new StringEnumConverter(), new MultiDimensionalArrayConverter() },
        Formatting = Formatting.Indented
    };

    private int _height = 0;
    
    void Start()
    {
        if (_startRoom.RuntimeKeyIsValid())
        {
            _roomLoadOpHandle = _startRoom.LoadAssetAsync<TextAsset>();
            _roomLoadOpHandle.Completed += LoadRoom;
        }
    }

    void LoadRoom(AsyncOperationHandle<TextAsset> asyncOperationHandle)
    {
        if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("Failed to load room asset!");
            return;
        }

        TextAsset roomFile = asyncOperationHandle.Result;
        Room room = JsonConvert.DeserializeObject<Room>(roomFile.text);
        Vector3Int[] positions = new Vector3Int[room.width * room.height];
        TileBase[] tileArray = new TileBase[positions.Length];
        for (int x = 0; x < room.width; x++)
        {
            for (int y = 0; y < room.height; y++)
            {
                positions[x + y * room.width] = new Vector3Int(x, -y);
                tileArray[x + y * room.width] = allTiles[room.Tiles[x, y]];
            }
        }
        Tilemap tileMap = GetComponent<Tilemap>();
        tileMap.SetTiles(positions, tileArray);
    }

    public void UpdateTileList()
    {
        allTiles = new TileBase[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].tileID = i;
            allTiles[i] = tiles[i].tile;
        }
    }

    public void ConvertRooms()
    {
        foreach (TextAsset template in roomTemplate)
        {
            Room newRoom = new();
            _height = 0;
            int minX = 100, minY = 100;
            int maxX = 0, maxY = 0;
            int x = 0, y = 0;
            foreach (char character in template.text)
            {
                if (character.Equals('\r'))
                    continue;
                if (character.Equals('e'))
                {
                    x++;
                    continue;
                }
                if (character.Equals('\n'))
                {
                    _height++;
                    y++;
                    x = 0;
                    if (_height >= 31)
                        break;
                    continue;
                }

                switch (character)
                {
                    case '2':
                        newRoom.north = true;
                        break;
                    case '3':
                        newRoom.south = true;
                        break;
                    case '4':
                        newRoom.east = true;
                        break;
                    case '5':
                        newRoom.west = true;
                        break;
                }

                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
                x++;
            }

            newRoom.width = maxX - minX + 1;
            newRoom.height = maxY - minY + 1;
            newRoom.Tiles = new int[newRoom.width, newRoom.height];
            x = 0;
            y = 0;
            _height = 0;
            EntityData currentEntity = new();
            newRoom.entities = new List<EntityData>();
            string entityString = "";
            foreach (char character in template.text)
            {
                // Parse Entity
                if (_height >= 31)
                {
                    if (character.Equals('\r'))
                        continue;
                    if (character.Equals('\n'))
                    {
                        string[] values = entityString.Split(' ');
                        switch (values[0][0])
                        {
                            case 'e':
                                currentEntity.type = EntityType.Enemy;
                                break;
                            case 'p':
                                currentEntity.type = EntityType.Pickup;
                                break;
                            case 's':
                                currentEntity.type = EntityType.ShopItem;
                                break;
                        }
                        currentEntity.id = int.Parse(values[1]);
                        currentEntity.xPos = int.Parse(values[2]);
                        currentEntity.yPos = int.Parse(values[3]);
                        newRoom.entities.Add(currentEntity);
                        currentEntity = new();
                        entityString = "";
                        continue;
                    }
                    entityString += character;
                    continue;
                }
                // Parse Tile
                if (character.Equals('\r'))
                    continue;
                if (character.Equals('\n'))
                {
                    _height++;
                    y++;
                    x = 0;
                    continue;
                }
                if (x < minX || x > maxX || y < minY || y > maxY)
                {
                    x++;
                    continue;
                }

                if (character.Equals('e'))
                    newRoom.Tiles[x - minX, y - minY] = -1;
                foreach (TileInfo tileType in tiles)
                {
                    if (character.Equals(tileType.key))
                    {
                        newRoom.Tiles[x - minX, y - minY] = tileType.tileID;
                        break;
                    }
                }
                x++;
            }
            
            string jsonRoom = JsonConvert.SerializeObject(newRoom, _jsonSettings);
            File.WriteAllText(Path.Combine(Application.dataPath, "Rooms/" + template.name + ".json"), jsonRoom);
        }
    }

    [MenuItem("Utils/ReloadLevel")]
    static void ReloadLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
