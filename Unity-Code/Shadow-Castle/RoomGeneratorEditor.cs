
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomGenerator))]
public class RoomGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        RoomGenerator myTarget = (RoomGenerator)target; 
        
        if(GUILayout.Button("Update Tile List"))
        {
            myTarget.UpdateTileList();
        }

        if (GUILayout.Button("Convert Rooms"))
        {
            myTarget.ConvertRooms();
        }
    }
}