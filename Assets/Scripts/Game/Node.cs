using System;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public float mass = 1f;
    public float airResist = 1f;
    public List<Rope> hangingFrom = new List<Rope>();

    public Vector2 Velocity = Vector2.zero;
    public bool Fixed
    {
        set;
        get;
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
            Velocity += acceleration * Time.fixedDeltaTime;
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

        transform.Translate(Velocity * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (!Fixed)
        {
            Velocity.y += -9.8f * Time.fixedDeltaTime;
        }
    }
}
