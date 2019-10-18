using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidScript : MonoBehaviour
{
    Particle2D particle;
    public bool shouldMultiply;
    public GameObject smallerAsteroid;
    // Start is called before the first frame update
    void Start()
    {
        particle = gameObject.GetComponent<Particle2D>();
        particle.velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDestroy()
    {
        
        //thisBullet.GetComponent<BulletScript>().startingPosition = CollisionHull2D.getRotatedPoint(new Vector3(0.0f, 0.5f, 0.0f), new Vector3(0.0f,0.0f,0.0f), theta);
        //Instantiate(thisBullet);
    }

    public void Multiply()
    {
        if(shouldMultiply)
        {
            Instantiate(smallerAsteroid, this.transform.position, this.transform.rotation);
            Instantiate(smallerAsteroid, this.transform.position, this.transform.rotation);
        }
            
        Destroy(this.gameObject);
    }
}
