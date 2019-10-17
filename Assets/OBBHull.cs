using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBBHull : CollisionHull2D
{
    public Vector3 offset;
    public float halfX;
    public float halfY;
    public float startingRotation;
    public float currentRotation;
    // Start is called before the first frame update
    void Start()
    {
        hull = hullType.OBB;
        GameObject.Find("CollisionManager").GetComponent<CollisionManager>().AddCollisionHull(this);

    }

    // Update is called once per frame
    void Update()
    {
       currentRotation = this.gameObject.transform.eulerAngles.z + startingRotation;
    }
}
