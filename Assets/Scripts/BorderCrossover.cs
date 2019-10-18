using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderCrossover : MonoBehaviour
{
    // Start is called before the first frame update
    Vector3 stageDimensions;
    Particle2D particle;
    void Start()
    {
        stageDimensions = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        particle = GetComponent<Particle2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(stageDimensions);
        if (particle.position.y > stageDimensions.y)
            particle.position.y = -stageDimensions.y;
        if (particle.position.y < -stageDimensions.y)
            particle.position.y = stageDimensions.y;
        if (particle.position.x > stageDimensions.x)
            particle.position.x = -stageDimensions.x;
        if (particle.position.x < -stageDimensions.x)
            particle.position.x = stageDimensions.x;
    }
}
