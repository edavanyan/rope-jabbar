using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [SerializeField] private Node nodePrefab;
    public LinkedList<Node> nodes = new LinkedList<Node>();
    [SerializeField] private float k = 1;
    [SerializeField] float minDistance = 1f;

    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] float friction = 2;
    [SerializeField] float maxLength = 5;
    private float length = 2.3f;

    private bool start = false;
    private bool startDraw = false;

    private Node hangItem;

    private MeshFilter meshFilter;
    
    private void Awake()
    {
        
        var ring = Instantiate(nodePrefab, Vector3.zero, Quaternion.identity, transform);
        nodes.AddFirst(ring);
        nodes.First.Value.Fixed = true;
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.SetVertices(new []
        {
            Vector3.down, Vector3.up
        });

        StartCoroutine(StartPhysics());
        
    }

    private IEnumerator StartPhysics()
    {
        start = true;
        yield return new WaitForSeconds(2);
        startDraw = true;
    } 

    public void FixAt(Vector2 position)
    {
        nodes.First.Value.transform.position = new Vector3(position.x, position.y, 0);
    }

    public void SetNodeCount(int count)
    {
        while (nodes.Count < count)
        {
            AddNode();
        }
        while (nodes.Count > count)
        {
            RemoveNode();
        }
    }

    public void AddNode(Node node = null)
    {
        var ring = node == null ? Instantiate(nodePrefab, transform) : node;
        nodes.AddLast(ring);
        
        var vertices = new List<Vector3>();
        meshFilter.mesh.GetVertices(vertices);
        vertices.Add(Vector3.one);
        vertices.Add(Vector3.zero);
        meshFilter.mesh.SetVertices(vertices);
        
        meshFilter.mesh.SetUVs(0, vertices);

        var triangles = new List<int>();
        meshFilter.mesh.GetTriangles(triangles, 0);
        int index = (nodes.Count - 1) * 2;
        triangles.Add(index - 2);
        triangles.Add(index - 1);
        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index);
        triangles.Add(index - 1);
        
        triangles.Add(index - 2);
        triangles.Add(index);
        triangles.Add(index - 1);
        triangles.Add(index + 1);
        triangles.Add(index - 1);
        triangles.Add(index);
        
        meshFilter.mesh.SetTriangles(triangles, 0, true);
    }

    public void RemoveNode()
    {
        nodes.RemoveLast();
        
        var vertices = new List<Vector3>();
        meshFilter.mesh.GetVertices(vertices);
        vertices.RemoveAt(vertices.Count - 1);
        vertices.RemoveAt(vertices.Count - 1);

        var triangles = new List<int>();
        meshFilter.mesh.GetTriangles(triangles, 0);
        for (int i = 0; i < 12; i++)
        {
            triangles.RemoveAt(triangles.Count - 1);
        }

        meshFilter.mesh.SetTriangles(triangles.ToArray(), 0, true);
        meshFilter.mesh.SetVertices(vertices);
        meshFilter.mesh.RecalculateNormals();
    }

    public void HangItem(Node hangItem)
    {
        this.hangItem = hangItem;
        this.hangItem.hangingFrom.Add(this);
        AddNode(hangItem);
    }

    public void RemoveHangItem()
    {
        if (hangItem != null)
        {
            hangItem.hangingFrom.Remove(this);
            hangItem = null;
            RemoveNode();
        }
    }

    private void FixedUpdate()
    {

        length = 0;
        var node = nodes.First.Next;
        while (node != null)
        {
            length += (node.Value.transform.position - node.Previous.Value.transform.position).magnitude;
            node = node.Next;
        }

        var lastPosition = nodes.Last.Value.transform.position;

        var direction = (Vector2)(lastPosition - nodes.First.Value.transform.position);
        var distance = direction.magnitude;
        node = nodes.First;

        int index = 1;
        while (node != null)
        {
            if (!(Input.GetMouseButton(0) && distance >= maxLength))
            {
                var acceleration = ApplyForces(node) / node.Value.mass;
                node.Value.ApplyAccelration(acceleration);
            }
            else if (!node.Equals(nodes.First))
            {
                const float speed = 20;
                var destination = (Vector2)nodes.First.Value.transform.position + direction.normalized * (index * distance / (nodes.Count - 1));

                var dist = destination - (Vector2)node.Value.transform.position;
                node.Value.Velocity = dist * speed;
                index++;
            }

            node = node.Next;
        }
    }

    private void OnDrawGizmos()
    {
        if (Input.GetMouseButton(0))
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(touchDownPosition, touchDirection);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(nodes.Last.Value.transform.position, nodes.First.Value.transform.position);
        }
    }

    private Vector2 touchDirection;
    public void Cut(Vector2 direction)
    {
        touchDirection = direction;
        Vector2 intersectionPoint;
        if (Utils.IntersectLineSegments2D(
                touchDownPosition,
                direction,
        nodes.First.Value.transform.position,
                nodes.Last.Value.transform.position,
                out intersectionPoint
            ))
        {
            RemoveHangItem();
        }
    }

    private Vector2 ApplyForces(LinkedListNode<Node> node)
    {
        var ropeNode = node.Value;
        var forcePrev = Vector2.zero;

        if (node.Previous != null)
        {
            var previousNode = node.Previous.Value;
            var direction = (Vector2)(previousNode.transform.position - ropeNode.transform.position);
            if (direction.sqrMagnitude >= minDistance * minDistance)
            {
                var magnitude = direction.magnitude;
                var frictionForce = friction * Vector3.Dot(previousNode.Velocity - ropeNode.Velocity, direction) /
                                      magnitude;

                forcePrev = direction.normalized * (((magnitude - minDistance) * k) + frictionForce);
            }
        }

        var forceNext = Vector2.zero;
        if (node.Next != null)
        {
            var nextNode = node.Next.Value;
            var direction = (Vector2)(nextNode.transform.position - ropeNode.transform.position);
            if (direction.sqrMagnitude >= minDistance * minDistance)
            {
                var magnitude = direction.magnitude;
                var frictionForce = friction * Vector3.Dot(nextNode.Velocity - ropeNode.Velocity, direction) / magnitude;

                forceNext = direction.normalized * ((magnitude - minDistance) * k + frictionForce);
            }
        }

        var force = new Vector2(forcePrev.x + forceNext.x, forcePrev.y + forceNext.y);

        return force;
        
    }

    private List<Vector3> vertices = new List<Vector3>();
    private Vector2 touchDownPosition;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchDownPosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            var direction = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Cut(direction);
        }

        if (Input.GetMouseButtonUp(0))
        {
            nodes.Last.Value.Fixed = false;
        }
        var node = nodes.First;
        int index = 0;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = nodes.Count;
        
        vertices.Clear();
        meshFilter.mesh.GetVertices(vertices);
        while (node != null)
        {
            var ropeNode = node.Value;
            vertices[index * 2] = new Vector3(ropeNode.transform.position.x - 0.05f, ropeNode.transform.position.y, 0);
            vertices[index * 2 + 1] = new Vector3(ropeNode.transform.position.x + 0.05f, ropeNode.transform.position.y, 0);
            
            if (startDraw)
            {
                lineRenderer.SetPosition(index, ropeNode.transform.position);
            }

            index++;

            var dir = (Vector2)(nodes.Last.Value.transform.position - nodes.First.Value.transform.position);
            var dis = dir.magnitude;
            if (start)
            {
                if (dis < maxLength)
                {
                    if (index > 1 && index < nodes.Count - 1)
                    {
                        float speed = 1f;
                        if (length > maxLength * 2 && dis < maxLength)
                        {
                            speed = 25f;
                        }

                        var destination = (Vector2)nodes.First.Value.transform.position +
                                          Vector2.down.normalized * (index * (maxLength - 0.1f) / (nodes.Count - 1));

                        var dist = destination - (Vector2)node.Value.transform.position;
                        node.Value.transform.Translate(dist * (speed * Time.deltaTime));
                    }
                }
            }

            node = node.Next;
        }
        meshFilter.mesh.SetVertices(vertices);

        if (Input.GetMouseButton(1))
        {
            if (start)
            {
                start = false;
                node = nodes.First.Next;
                var position = nodes.First.Value.transform.position = new Vector2(-5, 4.5f);
                while (node != null)
                {
                    var ropeNode = node.Value;
                    ropeNode.transform.position = new Vector2(position.x + 0.9f, 4.5f);
                    node = node.Next;
                }
            }
        }
    }
}
