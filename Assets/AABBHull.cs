using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AABBHull : CollisionHull2D
{
    // Start is called before the first frame update
    public Vector3 offset;
    public float halfX;
    public float halfY;
    private void Start()
    {
        hull = hullType.AABB;
        GameObject.Find("CollisionManager").GetComponent<CollisionManager>().AddCollisionHull(this);

    }
    private void Update()
    {
        
    }
    Vector2 getMinCorner()
    {
        return this.transform.position - new Vector3(halfX, halfY) + offset;
    }

    Vector2 getMaxCorner()
    {
        return this.transform.position + new Vector3(halfX, halfY) + offset;
    }
}
