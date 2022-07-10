using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SpriteGrid : MonoBehaviour
{
    MeshFilter filter;
    MeshRenderer render;

    public int width;
    public int height;

    
    int spriteCount;
    int numSprites;

    Vector2Int center;

    int spriteSize;

    Vector2[] uvs;

    Mesh mesh;

    private void Awake()
    {
        filter = GetComponent<MeshFilter>();
        render = GetComponent<MeshRenderer>();
    }

    public void Build(int width, int height, int numSprites, int spriteSize)
    {
        this.width = width;
        this.height = height;
        this.numSprites = numSprites;
        this.spriteSize = spriteSize;

        filter.mesh = mesh = new Mesh();
        mesh.name = "Sprite Grid Mesh";

        for (int i = 1; i <= 5; i++)
        {
            if (numSprites + 1 <= i*i)
            {
                numSprites = i * i;
                spriteCount = i;
                break;
            }
        }

        Vector3[] vertices = new Vector3[4 * width * height];
        uvs = new Vector2[vertices.Length];
        int[] triangles = new int[6 * (width) * (height)];

        for (int vert = 0, tri = 0, y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++, vert +=4, tri += 6)
            {
                vertices[vert + 0] = new Vector3(x    , y    );
                vertices[vert + 1] = new Vector3(x + 1, y    );
                vertices[vert + 2] = new Vector3(x    , y + 1);
                vertices[vert + 3] = new Vector3(x + 1, y + 1);

                triangles[tri + 0] = vert;
                triangles[tri + 1] = vert + 2;
                triangles[tri + 2] = vert + 1;
                triangles[tri + 3] = vert + 1;
                triangles[tri + 4] = vert + 2;
                triangles[tri + 5] = vert + 3;

                uvs[vert + 0] = new Vector2(0, 0);
                uvs[vert + 1] = new Vector2(1f / spriteCount, 0);
                uvs[vert + 2] = new Vector2(0, 1f / spriteCount);
                uvs[vert + 3] = new Vector2(1f / spriteCount, 1f / spriteCount);
                
            }
        }

        //Set up default mesh content
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        //Set up renderer data (by stealing it from a default object)
        GameObject materialObj = new GameObject("BAD Hack");
        render.material = materialObj.AddComponent<SpriteRenderer>().material;
        Destroy(materialObj);

        //Set up default texture data
        Texture2D texture = new Texture2D(spriteCount * spriteSize, spriteCount * spriteSize, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;

        Color[] cols = texture.GetPixels();
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i] = new Color(1, 1, 1, 0.0f);
        }
        texture.SetPixels(cols);
        texture.Apply();

        render.material.mainTexture = texture;
        render.material.renderQueue = 4000;
    }

    public void AddSprite(int index, Sprite sprite)
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (index < 0 || index >= numSprites)
        {
            Debug.LogError($"You cannot access that sprite! (Index {index} out of {spriteCount})");
            return;
        }
        if (sprite.textureRect.width != spriteSize || sprite.textureRect.height != spriteSize)
        {
            Debug.LogError($"Sprite has incorrect size! Must be square, with h/w of {spriteSize} instead of {sprite.texture.width}x{sprite.texture.height}");
            return;
        }
        #endif

        Rect rect = sprite.textureRect;

        index++;

        int posX = index % spriteCount;
        int posY = index / spriteCount;

        //Write the sprite into the appropriate location
        Texture2D tex = render.material.mainTexture as Texture2D;
        tex.SetPixels(posX * spriteSize, posY * spriteSize, spriteSize, spriteSize, sprite.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height));
        tex.Apply();
        render.material.mainTexture = tex;
    }

    public void AddSprites(params Sprite[] sprites)
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (sprites.Length > numSprites)
        {
            Debug.LogError($"Attempted to add too many sprites to a sprite grid! ({sprites.Length} vs {numSprites})");
            return;
        }
        foreach (Sprite sprite in sprites)
        {
            if (sprite.textureRect.width != spriteSize || sprite.textureRect.height != spriteSize)
            {
                Debug.LogError($"Sprite has incorrect size! Must be square, with h/w of {spriteSize} instead of {sprite.texture.width}x{sprite.texture.height}");
                return;
            }
        }
        #endif

        int index = 0;
        Texture2D tex = render.material.mainTexture as Texture2D;

        foreach (Sprite sprite in sprites)
        {
            Rect rect = sprite.textureRect;

            index++;

            int posX = index % spriteCount;
            int posY = index / spriteCount;

            //Write the sprite into the appropriate location
            tex.SetPixels(posX * spriteSize, posY * spriteSize, spriteSize, spriteSize, sprite.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height));
        }

        tex.Apply();
        render.material.mainTexture = tex;
    }

    public void SetSpriteImmediate(int x, int y, int spriteNum)
    {
        int vertIndex = (y * width + x) * 4;

        spriteNum++;
        int spriteX = spriteNum % spriteCount;
        int spriteY = spriteNum / spriteCount;
        float split = spriteCount;

        float lowerX = spriteX / split;
        float upperX = (spriteX + 1) / split;
        float lowerY = spriteY / split;
        float upperY = (spriteY + 1) / split;

        uvs = filter.mesh.uv;

        uvs[vertIndex + 0] = new Vector2(lowerX, lowerY);
        uvs[vertIndex + 1] = new Vector2(upperX, lowerY);
        uvs[vertIndex + 2] = new Vector2(lowerX, upperY);
        uvs[vertIndex + 3] = new Vector2(upperX, upperY);

        filter.mesh.uv = uvs;
    }

    public void SetSprite(int x, int y, int spriteNum)
    {
        int vertIndex = (y * width + x) * 4;

        spriteNum++;
        int spriteX = spriteNum % spriteCount;
        int spriteY = spriteNum / spriteCount;
        float split = spriteCount;

        float lowerX = spriteX / split;
        float upperX = (spriteX + 1) / split;
        float lowerY = spriteY / split;
        float upperY = (spriteY + 1) / split;

        uvs[vertIndex + 0] = new Vector2(lowerX, lowerY);
        uvs[vertIndex + 1] = new Vector2(upperX, lowerY);
        uvs[vertIndex + 2] = new Vector2(lowerX, upperY);
        uvs[vertIndex + 3] = new Vector2(upperX, upperY);
    }

    public void Apply()
    {
        filter.mesh.uv = uvs;
    }

    public void ClearSprite(int x, int y)
    {
        int vertIndex = (y * width + x) * 4;

        uvs[vertIndex + 0] = new Vector2(0, 0);
        uvs[vertIndex + 1] = new Vector2(1f / spriteCount, 0);
        uvs[vertIndex + 2] = new Vector2(0, 1f / spriteCount);
        uvs[vertIndex + 3] = new Vector2(1f / spriteCount, 1f / spriteCount);
    }

    public void ClearAll()
    {
        for (int vertIndex = 0; vertIndex < uvs.Length; vertIndex += 4)
        {
            uvs[vertIndex + 0] = new Vector2(0, 0);
            uvs[vertIndex + 1] = new Vector2(1f / spriteCount, 0);
            uvs[vertIndex + 2] = new Vector2(0, 1f / spriteCount);
            uvs[vertIndex + 3] = new Vector2(1f / spriteCount, 1f / spriteCount);
        }
    }

    public void ClearAllImmediate()
    {
        for (int vertIndex = 0; vertIndex < uvs.Length; vertIndex += 4)
        {
            uvs[vertIndex + 0] = new Vector2(0, 0);
            uvs[vertIndex + 1] = new Vector2(1f / spriteCount, 0);
            uvs[vertIndex + 2] = new Vector2(0, 1f / spriteCount);
            uvs[vertIndex + 3] = new Vector2(1f / spriteCount, 1f / spriteCount);
        }

        Apply();
    }

    public void ClearSpriteImmediate(int x, int y)
    {
        int vertIndex = (y * width + x)*4;

        Vector2[] uvs = filter.mesh.uv;

        uvs[vertIndex + 0] = new Vector2(0, 0);
        uvs[vertIndex + 1] = new Vector2(1f / spriteCount, 0);
        uvs[vertIndex + 2] = new Vector2(0, 1f / spriteCount);
        uvs[vertIndex + 3] = new Vector2(1f / spriteCount, 1f / spriteCount);

        filter.mesh.uv = uvs;
    }

    public void SetCenter(Vector2 location)
    {
        location = location - (new Vector2(width, height) / 2);// + (.5f * Vector2.one);
        transform.position = (Vector3) location + 100 * Vector3.forward;
    }


}
