using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class CustomTile : TileBase
{
    
    [Header("Preset information")]
    public Sprite sprite;
    
    [Header("Information for in use (Available for debugging)")]
    public Color color = Color.white;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
        tileData.color = color;
        var m = tileData.transform;
        m.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
        tileData.transform = m;
        tileData.flags = TileFlags.LockTransform;
        tileData.colliderType = Tile.ColliderType.None;
    }
    
    
    #if UNITY_EDITOR
    // The following is a helper that adds a menu item to create a RoadTile Asset
        [MenuItem("Assets/Create/Custom Tile")]
        public static void CreateRoadTile()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Custom Tile", "New Custom Tile", "Asset", "Save Custom Tile", @"Assets\Scriptable Objects\Tiles");
            if (path == "")
                return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CustomTile>(), path);
        }
    #endif
}
