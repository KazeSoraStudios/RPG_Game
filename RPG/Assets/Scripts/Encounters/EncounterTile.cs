using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EncounterTile : Tile
{
    public int EncounterId;
    public TileBase Tile;

    #if UNITY_EDITOR
    [MenuItem("Assets/Tiles/EncounterTile")]
    public static void CreateEncounterTile()
    {
        var path = EditorUtility.SaveFilePanelInProject("Create Encounter Tile", "New Encounter Tile", "Asset", "Create Encounter Tile");
        if (path.IsEmptyOrWhiteSpace())
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<EncounterTile>(), path);
    }
    #endif
}
