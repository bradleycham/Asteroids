using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceGenerator : MonoBehaviour
{
    //private float frictionCoefficient = 0.01f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static Vector2 GenerateForce_gravity(Vector2 worldUp, float gravitationalConstant, float particleMass)
    {
        Vector2 f_gravity = particleMass * gravitationalConstant * worldUp;
        return f_gravity;
    }
    
    public static Vector2 GenerateForce_normal(Vector2 f_gravity, Vector2 surfaceNormal_unit)
    {
        Vector2 f_normal = Vector3.Project(f_gravity, surfaceNormal_unit);
        return f_normal;
    }
    
    public static Vector2 GenerateForce_sliding(Vector2 f_gravity, Vector2 f_normal)
    {
        Vector2 f_sliding = f_normal + f_gravity;
        return f_sliding;
    }

    public static Vector2 GenerateForce_friction_static(Vector2 f_normal, Vector2 f_opposing, float frictionCoefficient_static)
    {
        float max = f_normal.magnitude * frictionCoefficient_static;
        Vector2 f_friction_s;
        if (f_opposing.magnitude * frictionCoefficient_static < max)
        {
            f_friction_s = -f_opposing;
            return f_friction_s;
        }

        else
        {
            f_friction_s = -frictionCoefficient_static * f_normal;
            return f_friction_s;
        }
    }
    public static Vector2 GenerateForce_friction_kinetic(Vector2 f_normal, Vector2 particleVelocity, float frictionCoefficient_kinetic)
    {
        Vector2 f_friction_k;
        f_friction_k = -frictionCoefficient_kinetic * f_normal.magnitude * particleVelocity;
        return f_friction_k;
    }

    public static Vector2 GenerateForce_drag(Vector2 particleVelocity, Vector2 fluidVelocity, float fluidDensity, float objectArea_crossSection, float objectDragCoefficient)
    {
        Vector2 f_drag = -particleVelocity.normalized * (fluidDensity * (fluidVelocity * fluidVelocity) * objectArea_crossSection * objectDragCoefficient);
        return f_drag;
    }
    
    public static Vector2 GenerateForce_spring(Vector2 particlePosition, Vector2 anchorPosition, float springRestingLength, float springStiffnessCoefficient)
    {
        Vector2 force = particlePosition - anchorPosition;

        float magnitude = force.magnitude;
        magnitude = (magnitude - springRestingLength) * springStiffnessCoefficient;
        //magnitude *= springStiffnessCoefficient;
        force.Normalize();
        force *= magnitude;
        return -force;
    }
    
}
