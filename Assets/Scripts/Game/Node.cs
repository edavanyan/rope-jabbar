using System;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{

    public float mass = 1f;
    public float airResist = 1f;
    public List<Rope> hangingFrom = new List<Rope>();
    [SerializeField]private MeshRenderer meshRenderer;

    public Vector3 Velocity = Vector3.zero;
    private bool isFixed;
    public bool Fixed
    {
        set
        {
            isFixed = value;
            meshRenderer.enabled = value;
        }
        get => isFixed;
    }

    private void Awake()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    public void ApplyAccelration(Vector2 acceleration)
    {
        if (!Fixed)
        {
            if (Math.Abs(acceleration.x) < 0.5f)
            {
                acceleration.x = 0;
            }
            if (Math.Abs(acceleration.y) < 0.5f)
            {
                acceleration.y = 0;
            }
            Velocity += (Vector3)acceleration * Time.fixedDeltaTime;
        }
    }

    private void Update()
    {
        if (!Fixed)
        {
            const float epsilon1 = 0.01f;
            if (Math.Abs(Velocity.x) < epsilon1)
            {
                Velocity.x = 0;
            }
            else
            {
                Velocity.x += -Math.Sign(Velocity.x) * Time.fixedDeltaTime * airResist;
            }

            if (Math.Abs(Velocity.y) < epsilon1)
            {
                Velocity.y = 0;
            }
            else
            {
                Velocity.y += -Math.Sign(Velocity.y) * Time.fixedDeltaTime * airResist;
            }
        }

        Velocity.z = 0;
        transform.Translate(Vector3.right * (Velocity.x * Time.deltaTime));
        transform.Translate(Vector3.up * (Velocity.y * Time.deltaTime));
    }

    private void FixedUpdate()
    {
        if (!Fixed)
        {
            Velocity.y += -9.8f * Time.fixedDeltaTime;
        }
    }
}
