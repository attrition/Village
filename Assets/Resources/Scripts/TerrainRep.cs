using UnityEngine;
using System.Collections.Generic;

public class TerrainRep : MonoBehaviour
{
    private Map map;

    private GameObject groundObj;
    private Mesh groundMesh;
    private Material groundMat;

    private GameObject roadObj;
    private Mesh roadMesh;
    private Material roadMat;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Generate(Map map)
    {
        this.map = map;

        CreateGroundMesh();
        CreateRoadMesh();
    }

    private void CreateRoadMesh()
    {
        var verts = new List<Vector3>();
        var idxs = new List<int>();
        var uvs = new List<Vector2>();

        var size = map.Size;
        for (int z = 0; z < size; z++)
        {
            for (int x = 0; x < size; x++)
            {
                if (map.GetTileTypeAt(x, z) == TileType.Road)
                {
                    var i = verts.Count;

                    verts.Add(new Vector3(x, 0.01f, z));
                    verts.Add(new Vector3(x, 0.01f, z + 1));
                    verts.Add(new Vector3(x + 1, 0.01f, z + 1));
                    verts.Add(new Vector3(x + 1, 0.01f, z));

                    idxs.Add(i);
                    idxs.Add(i + 1);
                    idxs.Add(i + 2);
                    idxs.Add(i + 2);
                    idxs.Add(i + 3);
                    idxs.Add(i);

                    uvs.Add(new Vector2(0, 1));
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(1, 0));
                    uvs.Add(new Vector2(1, 1));
                }
            }
        }

        roadObj = new GameObject("RoadRep");
        roadObj.transform.parent = this.transform;

        var rend = roadObj.GetOrAddComponent<MeshRenderer>();
        var filt = roadObj.GetOrAddComponent<MeshFilter>();
        var coll = roadObj.GetOrAddComponent<MeshCollider>();
        var click = roadObj.GetOrAddComponent<ClickableObject>();

        Destroy(roadMesh);
        roadMesh = new Mesh();
        roadMesh.name = "Road Mesh";
        roadMesh.vertices = verts.ToArray();
        roadMesh.triangles = idxs.ToArray();
        roadMesh.uv = uvs.ToArray();
        roadMesh.RecalculateNormals();

        Destroy(roadMat);
        roadMat = Resources.Load<Material>("Materials/Road");
        rend.material = roadMat;

        Destroy(filt.sharedMesh);
        filt.sharedMesh = roadMesh;

        Destroy(coll.sharedMesh);
        coll.sharedMesh = roadMesh;

        click.clickHandler = this.GroundClickHandler;
    }

    private void CreateGroundMesh()
    {
        var size = map.Size;

        var verts = new List<Vector3>();
        verts.Add(new Vector3(0, 0, 0));
        verts.Add(new Vector3(0, 0, size));
        verts.Add(new Vector3(size, 0, size));
        verts.Add(new Vector3(size, 0, 0));

        var idxs = new List<int>() { 0, 1, 2, 2, 3, 0 };
        var uvs = new List<Vector2>() {
            { new Vector2(0, 0) },
            { new Vector2(0, size) },
            { new Vector2(size, size) },
            { new Vector2(size, 0) },
        };

        groundObj = new GameObject("GroundRep");
        groundObj.transform.parent = this.transform;
        
        Destroy(groundMesh);
        groundMesh = new Mesh();
        groundMesh.name = "Ground Mesh";
        groundMesh.vertices = verts.ToArray();
        groundMesh.triangles = idxs.ToArray();
        groundMesh.uv = uvs.ToArray();
        groundMesh.RecalculateNormals();

        var rend = groundObj.GetOrAddComponent<MeshRenderer>();
        var filt = groundObj.GetOrAddComponent<MeshFilter>();
        var coll = groundObj.GetOrAddComponent<MeshCollider>();
        var click = groundObj.GetOrAddComponent<ClickableObject>();

        Destroy(groundMat);
        groundMat = Resources.Load<Material>("Materials/GrassGround");
        rend.material = groundMat;

        Destroy(filt.sharedMesh);
        filt.sharedMesh = groundMesh;

        Destroy(coll.sharedMesh);
        coll.sharedMesh = groundMesh;

        click.clickHandler = this.GroundClickHandler;
    }

    private void GroundClickHandler(GameObject ground, Vector3 clickPos)
    {
        Vector2 tilePos = map.WorldToTilePos(clickPos);
        Debug.Log("Clicked on " + ground.name + 
            " " + tilePos + ": " + map.GetTileTypeAt(tilePos));
    }
}
