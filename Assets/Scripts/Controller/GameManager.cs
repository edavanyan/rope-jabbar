using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Node hangingItem;
    public Rope ropePrefab;
    public GameObject mouseCollider;
    void Start()
    {
        Rope rope = Instantiate(ropePrefab);
        rope.FixAt(new Vector2(3, -2.5f));
        rope.SetNodeCount(5);
        rope.HangItem(hangingItem);
        
        rope = Instantiate(ropePrefab);
        rope.FixAt(new Vector2(2, 2.5f));
        rope.SetNodeCount(15);
        rope.HangItem(hangingItem);
        
        rope = Instantiate(ropePrefab);
        rope.FixAt(new Vector2(-4, -4f));
        rope.SetNodeCount(5);
        rope.HangItem(hangingItem);
        
        rope = Instantiate(ropePrefab);
        rope.FixAt(new Vector2(-2, 2.5f));
        rope.SetNodeCount(10);
        rope.HangItem(hangingItem);
        
        
        // var mesh = GetComponent<MeshCollider>().sharedMesh = new Mesh();
        // GetComponent<MeshFilter>().mesh = mesh;
        // mesh.SetVertices(new List<Vector3>
        // {
        //     new (-0.5f, 1f, 0),
        //     new (0.5f, 1f, 0),
        //     new (1f, -1f, 0),
        //     new (-1f, -1f, 0)
        // });
        // mesh.SetTriangles(new List<int>()
        // {
        //     0, 1, 2,
        //     2, 3, 0
        // }, 0);
        //
        // var triangles = new List<int>();
        // mesh.GetTriangles(triangles, 0);
        //
        // var vertices = new List<Vector3>();
        // mesh.GetVertices(vertices);
        // vertices.Add(new Vector3(-1, -2, 0));
        // vertices.Add(new Vector3(1, -2, 0));
        // mesh.SetVertices(vertices);
        //
        // triangles.Add(3);
        // triangles.Add(5);
        // triangles.Add(4);
        // triangles.Add(5);
        // triangles.Add(3);
        // triangles.Add(2);
        // mesh.SetTriangles(triangles, 0);


    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseCollider.SetActive(true);
            mouseCollider.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            mouseCollider.transform.position =(Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            mouseCollider.SetActive(false);
        }
    }
}
