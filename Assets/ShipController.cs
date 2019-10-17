using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    Particle2D thrust;
    OBBHull hull;
    public Vector2 turnPower;
    public Vector2 turnPosition;
    public float speed;
    GameObject bulletPrefab;
    GameObject thisBullet;
    float theta;
    Transform bulletSpawn;
    Vector3 forwardVec = new Vector2(0.0f, 1.0f);
    // Start is called before the first frame update
    void Start()
    {
        thrust = GetComponent<Particle2D>();
        hull = GetComponent<OBBHull>();
        bulletPrefab = Resources.Load("Bullet") as GameObject;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        theta = hull.currentRotation;
        forwardVec = CollisionHull2D.getRotatedPoint(new Vector3(0.0f, 1.0f), new Vector3(0.0f, 0.0f), theta);

        if (Input.GetKey(KeyCode.W))
        {
            //Debug.Log(forwardVec);    
            thrust.AddForce(forwardVec * speed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            thrust.applyForceAtLocation(turnPosition, -turnPower);
        }
        if (Input.GetKey(KeyCode.D))
        {
            thrust.applyForceAtLocation(turnPosition, turnPower);
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            FireCanons();
        }
    }

    private void OnDestroy()
    {
        Application.Quit();
    }
    void FireCanons()
    {
        thisBullet = Instantiate(bulletPrefab, this.transform.position + CollisionHull2D.getRotatedPoint(new Vector3(0.0f, 0.3f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), theta), this.transform.rotation);
        Destroy(thisBullet, 3);
        thisBullet.GetComponent<BulletScript>().force = forwardVec;
        thisBullet.GetComponent<BulletScript>().startingRotation = theta;
    }
}
