using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle2D : MonoBehaviour
{
    public float speed;
    public float velocityDamping;
    public float angularDamping;


    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;
    public Vector3 force;
    public Vector3 norm;

    public float angle;
    public float angularVelocity;
    public float angularAcceleration;

    public float invInertia;
    public float torque;

    public Vector3 applyForce;
    public Vector3 positionOfForce;

    public enum shape
    {
        circle,
        rect,
    }
    public shape Shape = shape.rect;

    public bool circulation;
    public bool verticalOscillate;
    public bool horizontalOscillate;
    public bool applyDrag;
    public bool applyGravity;
    public bool kineticFriction;
    public bool staticFriction;
    public bool isSliding;
    public bool isSpring;

    private const float GRAVITY = -10;
    private Vector2 GRAVITY_VEC = new Vector2(0, GRAVITY);

    [Range(0, Mathf.Infinity)]
    public float mass;
     public enum EUpdateMethod
    {
        Euler,
        Kinematic
    };
    
    // MASS AND FORCE
    private float invMass;
    private float Mass
    {
        set
        {
            mass = mass > 0.0f ? mass : 0.0f;
            invMass = mass > 0.0f ? 1.0f / mass : 0.0f;
        }
        get
        {
            return mass;
        }
    }

    public void AddForce(Vector3 newForce)
    {
        force += newForce;
    }

    // POSITION AND ROTATION UPDATE FUNCTIONS
    public void UpdateAcceleration()
    {
        acceleration = force * invMass;
        force.Set(0.0f, 0.0f, 0.0f);
    }
    void updatePositionEulerExplicit(float deltaTime)
    {
        position += velocity * deltaTime;
        velocity += acceleration * deltaTime;
    }

    void updatePositionKinematic(float deltaTime)
    {
        position += (velocity * deltaTime) + ((acceleration * deltaTime * deltaTime)/2);
        velocity += acceleration * deltaTime;
        Vector3 dampingVec = velocityDamping * velocity;
        velocity -= dampingVec;
    }

    void updateRotationEulerExplicit(float deltaTime)
    {
        angle += deltaTime * angularVelocity;
        angularVelocity += angularAcceleration * deltaTime;
    }

    void updateRotationKinematic(float deltaTime)
    {
        angle += (angularVelocity * deltaTime) + ((angularAcceleration * deltaTime * deltaTime) / 2);
        angularVelocity += angularAcceleration * deltaTime;
        float damping = angularDamping * angularVelocity;
        //if (angularVelocity > 0)
            angularVelocity -= damping;
    }

    // TEST FUNCTIONS
    void Circulate()
    {
        acceleration.x = -Mathf.Sin(Time.time) * speed;
        acceleration.y = Mathf.Cos(Time.time) * speed;
    }

    void Oscillate()
    {
        if(verticalOscillate)
            acceleration.y = -Mathf.Cos(Time.time) * speed;
        else if(horizontalOscillate)
            acceleration.x = -Mathf.Sin(Time.time) * speed;
    }

    public void applyForceAtLocation(Vector2 pointOfForce, Vector2 newForce)
    {
        if (Shape == shape.rect)
            invInertia = (1 / 12) * mass * ((pointOfForce.x * pointOfForce.x) + (pointOfForce.y * pointOfForce.y));
        else if (Shape == shape.circle)
            invInertia = (1 / 2) * mass * GetComponent<CircleHull>().radius* GetComponent<CircleHull>().radius; // *radius squared
        torque = Vector3.Cross(pointOfForce, newForce).z;
        angularVelocity += torque;

        applyForce = new Vector2(0.0f, 0.0f);
        positionOfForce = new Vector2(0.0f, 0.0f);
    }

    // TIME LOOPS
    void Start()
    {
        position = transform.position;
        Mass = mass;
        
        //circulate = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(applyGravity)
            AddForce(ForceGenerator.GenerateForce_gravity(Vector2.up, GRAVITY, Mass));
        if (applyDrag)
            AddForce(ForceGenerator.GenerateForce_drag(velocity, new Vector2(0f, 5f), 1.0f, 1.0f, 0.05f));

        norm = ForceGenerator.GenerateForce_normal(new Vector2(0, GRAVITY), new Vector2(-1f, 1f));
        if (kineticFriction)
            AddForce(ForceGenerator.GenerateForce_friction_kinetic(norm , velocity, 0.55f));
        if (staticFriction)
            AddForce(ForceGenerator.GenerateForce_friction_static(norm, velocity, 0.60f));
        if (isSliding)
            AddForce(ForceGenerator.GenerateForce_sliding(GRAVITY_VEC, -velocity));
        if (isSpring)
            AddForce(ForceGenerator.GenerateForce_spring(position, new Vector2(1.0f, 0.0f), 2.5f, 100f));   
    }

    private void FixedUpdate()
    {
        UpdateAcceleration();
        
        if (circulation)
            Circulate();
        else if (verticalOscillate || horizontalOscillate)
            Oscillate();

        applyForceAtLocation(positionOfForce, applyForce);
        
        updatePositionKinematic(Time.fixedDeltaTime);
        //updateRotationEulerExplicit(Time.fixedDeltaTime);
        updateRotationKinematic(Time.fixedDeltaTime);
        transform.position = position;
        transform.eulerAngles = new Vector3(0.0f, 0.0f, angle);
   
    }
}
