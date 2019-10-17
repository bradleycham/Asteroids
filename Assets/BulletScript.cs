using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float startingRotation;
    public Vector3 startingPosition;
    public Vector3 force;
    private Particle2D particle;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        particle = GetComponent<Particle2D>();
        this.transform.eulerAngles = new Vector3 (0.0f,0.0f,startingRotation);
        this.gameObject.GetComponent<Particle2D>().position += startingPosition;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.eulerAngles = new Vector3(0.0f, 0.0f, startingRotation);
        particle.velocity = (force * speed);
    }
}
