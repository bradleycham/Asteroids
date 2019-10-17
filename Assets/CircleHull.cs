using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleHull : CollisionHull2D
{
    // Start is called before the first frame update
    public float radius;
    //public float restitution;
    public Vector3 offset;

    void Start()
    {
        hull = hullType.CIRCLE;
        GameObject.Find("CollisionManager").GetComponent<CollisionManager>().AddCollisionHull(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
