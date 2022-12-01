using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [SerializeField] private Node nodePrefab;
    private LinkedList<Node> nodes = new LinkedList<Node>();
    [SerializeField] private float k = 1;
    [SerializeField] float minDistance = 1f;

    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] float friction = 2;
    [SerializeField] float maxLength = 5;
    private float length = 2.3f;

    private bool start = true;
    private bool startDraw = true;

    private Node hangItem;
    
    private void Awake()
    {
        var ring = Instantiate(nodePrefab, transform);
        nodes.AddFirst(ring);
        nodes.First.Value.Fixed = true;
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
        if (node == null)
        {
            node = Instantiate(nodePrefab, nodes.First.Value.transform.position, Quaternion.identity, transform);
        }

        nodes.AddLast(node);
    }

    public void RemoveNode()
    {
        nodes.RemoveLast();
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
        if (nodes.Count == 0)
        {
            return;
        }

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
            // if (!(Input.GetMouseButton(0) && distance >= maxLength))
            // {
                var acceleration = ApplyForces(node) / node.Value.mass;
                node.Value.ApplyAccelration(acceleration);
            // }
            // else if (!node.Equals(nodes.First))
            // {
            //     const float speed = 20;
            //     var destination = (Vector2)nodes.First.Value.transform.position + direction.normalized * (index * distance / (nodes.Count - 1));
            //
            //     var dist = destination - (Vector2)node.Value.transform.position;
            //     node.Value.Velocity = dist * speed;
            //     index++;
            // }

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
        for (int i = 0; i < nodes.Count; i++)
        {
            if (Utils.IntersectLineSegments2D(
                    touchDownPosition,
                    direction, 
                    nodes.First.Value.transform.position, 
                    nodes.Last.Value.transform.position,
                    out intersectionPoint
                ))
            {
                CutAt(intersectionPoint);
                return;
            }
        }
    }

    private bool intersected = false;
    private void CutAt(Vector2 intersectionPoint)
    {
        var node = nodes.First;
        while (node.Next != null)
        {
            var nextNode = node.Next;
            if (nextNode != null)
            {
                var curPos = node.Value.transform.position;
                var nextPos = nextNode.Value.transform.position;
                if (curPos.x > intersectionPoint.x && nextPos.x < intersectionPoint.x ||
                    curPos.x < intersectionPoint.x && nextPos.x > intersectionPoint.x ||
                    curPos.y < intersectionPoint.y && nextPos.y > intersectionPoint.y ||
                    curPos.y > intersectionPoint.y && nextPos.y < intersectionPoint.y)
                {
                    RemoveHangItem();
                    var cutRope = new LinkedList<Node>();
                    var cacheNode = node;
                    node = nodes.First;
                    node.Value.Fixed = false;
                    while (node != null)
                    {
                        var nNode = node.Next;
                        node.Value.transform.SetParent(null, true);
                        nodes.Remove(node);
                        cutRope.AddLast(node);
                        if (node == cacheNode)
                        {
                            break;
                        }
                        node = nNode;
                    }

                    if (nodes.Count == 1)
                    {
                        var linkedListNode = cutRope.Last;
                        cutRope.RemoveLast();
                        nodes.AddLast(linkedListNode);
                    }

                    if (cutRope.Count == 1)
                    {
                        var linkedListNode = nodes.Last;
                        nodes.RemoveLast();
                        cutRope.AddLast(linkedListNode);
                    }

                    GameManager.Instance.CreateRope(cutRope);
                    intersected = true;
                    break;
                }

                node = nextNode;
            }
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

    private Vector2 touchDownPosition;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchDownPosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            if (!intersected)
            {
                var direction = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Cut(direction);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            intersected = false;
        }

        var node = nodes.First;
        int index = 0;
        lineRenderer.startWidth = 0.13f;
        lineRenderer.endWidth = 0.13f;
        lineRenderer.positionCount = nodes.Count;
        while (node != null)
        {
            var ropeNode = node.Value;

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
                    if (index > 1)
                    {
                        if (!nodes.Last.Value.Equals(hangItem))
                        {
                            float speed = 1f;
                            if (length > maxLength * 2 && dis < maxLength)
                            {
                                speed = 25f;
                            }

                            var destination = (Vector3)nodes.First.Value.transform.position +
                                              Vector3.down.normalized *
                                              (index * (maxLength - 0.1f) / (nodes.Count - 1));

                            var dist = destination - node.Value.transform.position;
                            dist.z = 0;
                            node.Value.transform.Translate(Vector3.right * (dist.x * speed * Time.deltaTime));
                            node.Value.transform.Translate(Vector3.up * (dist.y * speed * Time.deltaTime));
                        }
                    }
                }
            }

            node = node.Next;
        }

        if (nodes.Count == 0 || (nodes.First.Value.transform.position.y < -8 && nodes.Last.Value.transform.position.y < -8))
        {
            var ropeNode = nodes.First;
            while (ropeNode != null)
            {
                Destroy(ropeNode.Value.gameObject);
                var curNode = ropeNode;
                ropeNode = ropeNode.Next;
                nodes.Remove(curNode);
            }

            Destroy(gameObject);
        }
    }

    public void AddAll(LinkedList<Node> ropeNodes)
    {
        Destroy(nodes.First.Value.gameObject);
        nodes = ropeNodes;
    }
}
