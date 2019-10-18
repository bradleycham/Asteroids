using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHull2D : MonoBehaviour
{
   
    public float restitution;

    public class HullCollision
    {
        public struct Contact
        {
            public Vector3 point;
            public Vector3 normal;
            public float restitution;
        }

        public Vector3 closingVelocity;
        public Vector3 penetration;
        public CollisionHull2D a;
        public CollisionHull2D b;
        public Contact[] contacts = new Contact[4];
        public bool status;
    }
    public enum hullType
    {
        AABB,
        OBB,
        CIRCLE
    }

    public hullType hull;

    // Start is called before the first frame update
    void Start()
    {
    }

    public static void ResolveCollision(HullCollision col)
    {
        Particle2D A = col.a.GetComponent<Particle2D>();
        Particle2D B = col.b.GetComponent<Particle2D>();
        float invAMass;
        float invBMass;
        if (A.mass == 0) invAMass = 0;
        else invAMass = 1 / A.mass;
        if (B.mass == 0) invBMass = 0;
        else invBMass = 1 / B.mass;


        float velAlongNormal = Vector3.Dot(col.closingVelocity, col.contacts[0].normal);
        //Debug.Log("velAlongNormal " + velAlongNormal);

        if (velAlongNormal > 0) return; // > makes square work properly
        //Debug.Log(velAlongNormal);
        // restitustion
        float e = col.contacts[0].restitution;
        // impulse scalar
        float j = -(1 + e) * velAlongNormal;
        j /= invAMass + invBMass;
        //Debug.Log(j);

        Vector3 impulse = j * col.contacts[0].normal;
        //Debug.Log(impulse);

        //A.velocity = new Vector3(0.0f, 0.0f, 0.0f);
        //B.velocity = new Vector3(0.0f, 0.0f, 0.0f);

        A.velocity -= invAMass * impulse;
        B.velocity += invBMass * impulse;

        // Positional Correction
        if (col.status)
        {
            float percent = 0.8f;
            float slop = 0.01f;
            Vector3 correction = Mathf.Max(velAlongNormal - slop, 0) / (invAMass + invBMass) * percent * col.contacts[0].normal;
            A.position += invAMass * correction; // started -
            B.position -= invBMass * correction; // started +
        }


    }

    public static HullCollision CircleCircleCollision(CircleHull circleHull1, CircleHull circleHull2)
    {
        // *IMPORTANT* for circle and square the collision only wirks with obejct1 - object 2 and not viceversa, must be a prob in clollision resolution
        Vector3 c1Offset = circleHull1.offset;
        Vector3 c2Offset = circleHull2.offset;

        Vector3 range = (circleHull2.transform.position + c2Offset) - (circleHull1.transform.position + c1Offset); // make sure offsets arent screwing things up
        float overlap = (circleHull2.radius + circleHull1.radius) - range.magnitude;

        HullCollision col = new HullCollision();
        col.a = circleHull1;
        col.b = circleHull2;
        col.penetration = range * overlap;

        HullCollision.Contact con0 = new HullCollision.Contact();
        con0.point =  (range.normalized * circleHull1.radius);
        con0.point += new Vector3(circleHull1.transform.position.x, circleHull1.transform.position.y);
        con0.normal = range.normalized;
        con0.restitution = Mathf.Min(circleHull1.restitution, circleHull2.restitution);

        col.contacts[0] = con0;

        Particle2D c1 = circleHull1.GetComponentInParent<Particle2D>();
        Particle2D c2 = circleHull2.GetComponentInParent<Particle2D>();

        Vector3 closingVel = c2.velocity - c1.velocity; // started as c1 -c2

        col.closingVelocity = closingVel;

        if (overlap > 0)
        {
            col.status = true;
            return col;
        }
        else
        {
            col.status = false;
            return col;
        }
    }

    public static HullCollision CircleAABBCollision(CircleHull circleHull, AABBHull boxHull)
    {
        Particle2D A = boxHull.GetComponent<Particle2D>();
        Particle2D B = circleHull.GetComponent<Particle2D>();

        Vector3 closestPoint = new Vector3(0.0f, 0.0f);
        Vector3 range = (circleHull.transform.position + circleHull.offset) - (boxHull.transform.position + boxHull.offset);
        

        closestPoint = new Vector3(Mathf.Clamp(range.x, -boxHull.halfX, boxHull.halfX), Mathf.Clamp(range.y, -boxHull.halfY, boxHull.halfY));

        HullCollision col = new HullCollision();
        col.a = boxHull;
        col.b = circleHull;
        Vector3 closingVel = B.velocity - A.velocity;
        Vector3 penetration = range - (closestPoint - circleHull.transform.position + circleHull.offset);
        col.closingVelocity = closingVel;
        col.penetration = penetration;

        HullCollision.Contact con0 = new HullCollision.Contact();
        con0.point = closestPoint;
        con0.restitution = Mathf.Min(boxHull.restitution, circleHull.restitution);

        Vector3 collisionNormal = new Vector3();


        if ((range - closestPoint).magnitude - circleHull.radius < 0)
        {
            if (con0.point.x == boxHull.halfX)//added mathf
                collisionNormal = new Vector3(1.0f, 0.0f);
            if (con0.point.x == -boxHull.halfX)//added mathf
                collisionNormal = new Vector3(-1.0f, 0.0f);
            if (con0.point.y == boxHull.halfY)
                collisionNormal = new Vector3(0.0f, 1.0f);
            if (con0.point.y == -boxHull.halfY)
                collisionNormal = new Vector3(0.0f, -1.0f);

            con0.normal = collisionNormal;

            col.status = true;
            col.contacts[0] = con0;
        }
        else col.status = false; 

        return col;
    }
    
    public static HullCollision CircleOBBCollision(CircleHull circleHull, OBBHull OBBHull)
    { 
        Particle2D A = circleHull.GetComponent<Particle2D>();
        Particle2D B = OBBHull.GetComponent<Particle2D>();
        Vector3[] OBBCorners;

        OBBCorners = new Vector3[2];//was 4
        Vector3[] normals = new Vector3[2];
        float[] OBBMinMax = new float[2];
        float[] circleMinMax = new float[2];

        OBBCorners = getRotatedCorners(OBBHull);

        normals[0] = getUpNormal(-OBBHull.currentRotation);
        normals[1] = getRightNormal(-OBBHull.currentRotation);
        //normals[2] = getUpNormal(-OBBHull2.currentRotation);
        //normals[3] = getRightNormal(-boxHull2.currentRotation);

        HullCollision col = new HullCollision();
        col.a = circleHull;
        col.b = OBBHull;
        Vector3 range = (OBBHull.transform.position + OBBHull.offset) - (circleHull.transform.position + circleHull.offset);

        Vector3 rotatedRange = getRotatedPoint(range, new Vector3 (0.0f,0.0f), -OBBHull.currentRotation);// 2 circleHull.transform.position
        Vector3 point = new Vector3(Mathf.Clamp(rotatedRange.x, -OBBHull.halfX, OBBHull.halfX), Mathf.Clamp(rotatedRange.y, -OBBHull.halfY, OBBHull.halfY));
        //Debug.Log("range " + range);
        //Debug.Log("rotrange " + rotatedRange);

        //float xOverlap = boxHull1.halfX + boxHull2.halfX - Mathf.Abs(range.x);
        //float yOverlap = boxHull1.halfY + boxHull2.halfY - Mathf.Abs(range.y);

        //col.penetration = new Vector3(xOverlap, yOverlap);

        Vector3 closingVel = B.velocity - A.velocity;
        col.closingVelocity = closingVel;

        HullCollision.Contact con0 = new HullCollision.Contact();
        con0.point = new Vector3(Mathf.Clamp(range.x, -OBBHull.halfX, OBBHull.halfX), Mathf.Clamp(range.y, -OBBHull.halfY, OBBHull.halfY));
        con0.restitution = Mathf.Min(OBBHull.restitution, circleHull.restitution);
        con0.normal = range.normalized;
        //Debug.Log("point " + point);

        col.status = false;
        if ((rotatedRange - point).magnitude - circleHull.radius < 0 )
        {
            col.status = true;
            col.contacts[0] = con0;
        }
        return col;
    }
    
    public static HullCollision AABBAABBCollision(AABBHull boxHull1, AABBHull boxHull2)
    {
        Vector3 min0, max0, min1, max1;
        Vector3 b1Offset = boxHull1.offset;
        Vector3 b2Offset = boxHull2.offset;
        Particle2D A = boxHull1.GetComponent<Particle2D>();
        Particle2D B = boxHull2.GetComponent<Particle2D>();

        min0 = boxHull1.transform.position - new Vector3(boxHull1.halfX, boxHull1.halfY) + b1Offset;
        max0 = boxHull1.transform.position + new Vector3(boxHull1.halfX, boxHull1.halfY) + b1Offset;
        min1 = boxHull2.transform.position - new Vector3(boxHull2.halfX, boxHull2.halfY) + b2Offset;
        max1 = boxHull2.transform.position + new Vector3(boxHull2.halfX, boxHull2.halfY) + b2Offset;

        Vector3 range = (boxHull2.transform.position + b2Offset) - (boxHull1.transform.position + b1Offset); // make sure offsets arent screwing things up

        HullCollision col = new HullCollision();
        col.a = boxHull1;
        col.b = boxHull2;

        float xOverlap = boxHull1.halfX + boxHull2.halfX - Mathf.Abs(range.x);
        float yOverlap = boxHull1.halfY + boxHull2.halfY - Mathf.Abs(range.y);

        //TRANSPORTATION
        
        col.penetration = new Vector3(xOverlap, yOverlap);

        //Vector3 closingVel = A.velocity - B.velocity;
        Vector3 closingVel = B.velocity - A.velocity;
        col.closingVelocity = closingVel;

        HullCollision.Contact con0 = new HullCollision.Contact();
        con0.point = new Vector3(Mathf.Clamp(range.x, -boxHull1.halfX, boxHull1.halfX), Mathf.Clamp(range.y, -boxHull1.halfY, boxHull1.halfY));
        con0.restitution = Mathf.Min(boxHull1.restitution, boxHull2.restitution);
        
        if (max0.x >= min1.x && max1.x >= min0.x)
        {
            if (max0.y >= min1.y && max1.y >= min0.y)
            {
                Vector3 collisionNormal = new Vector3();

                if (con0.point.x == boxHull1.halfX)//added mathf
                    collisionNormal = new Vector3(1.0f, 0.0f);
                if (con0.point.x == -boxHull1.halfX)//added mathf
                    collisionNormal = new Vector3(-1.0f, 0.0f);
                if (con0.point.y == boxHull1.halfY)
                    collisionNormal = new Vector3(0.0f, 1.0f);
                if (con0.point.y == -boxHull1.halfY)
                    collisionNormal = new Vector3(0.0f, -1.0f);

                con0.normal = collisionNormal;

                col.status = true;
            }
        }    
        else col.status = false;
        col.contacts[0] = con0;
        return col;   
    }
    
    public static HullCollision AABBOBBCollision(AABBHull AABBHull, OBBHull OBBHull)
    {
        Particle2D A = AABBHull.GetComponent<Particle2D>();
        Particle2D B = OBBHull.GetComponent<Particle2D>();
        Vector3[] shape1Corners;
        Vector3[] shape2Corners;
        shape1Corners = new Vector3[4];
        shape2Corners = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        float[] shape1MinMax = new float[2];
        float[] shape2MinMax = new float[2];

        shape1Corners[0] = AABBHull.transform.position - new Vector3(AABBHull.halfX, AABBHull.halfY) + AABBHull.offset;
        shape1Corners[1] = AABBHull.transform.position - new Vector3(AABBHull.halfX, -AABBHull.halfY) + AABBHull.offset;
        shape1Corners[2] = AABBHull.transform.position + new Vector3(AABBHull.halfX, AABBHull.halfY) + AABBHull.offset;
        shape1Corners[3] = AABBHull.transform.position + new Vector3(AABBHull.halfX, -AABBHull.halfY) + AABBHull.offset;
        shape2Corners = getRotatedCorners(OBBHull);

        normals[0] = new Vector3(0.0f, 1.0f, 0.0f);
        normals[1] = new Vector3(1.0f, 0.0f, 0.0f);
        normals[2] = getUpNormal(-OBBHull.currentRotation);
        normals[3] = getRightNormal(-OBBHull.currentRotation);

        HullCollision col = new HullCollision();
        col.a = AABBHull;
        col.b = OBBHull;
        Vector3 range = (OBBHull.transform.position + OBBHull.offset) - (AABBHull.transform.position + AABBHull.offset);
        //float xOverlap = boxHull1.halfX + boxHull2.halfX - Mathf.Abs(range.x);
        //float yOverlap = boxHull1.halfY + boxHull2.halfY - Mathf.Abs(range.y);

        //TRANSPORTATION

        //col.penetration = new Vector3(xOverlap, yOverlap);

        //Vector3 closingVel = A.velocity - B.velocity;
        Vector3 closingVel = B.velocity - A.velocity;
        col.closingVelocity = closingVel;

        HullCollision.Contact con0 = new HullCollision.Contact();
        con0.point = new Vector3(Mathf.Clamp(range.x, -AABBHull.halfX, AABBHull.halfX), Mathf.Clamp(range.y, -AABBHull.halfY, AABBHull.halfY));
        con0.restitution = Mathf.Min(AABBHull.restitution, OBBHull.restitution);
        con0.normal = range.normalized;

        for (int i = 0; i < normals.Length; i++)
        {
            //Debug.Log("testing corner" + i);

            shape1MinMax = SatTest(normals[i], shape1Corners);
            shape2MinMax = SatTest(normals[i], shape2Corners);
            if (!Overlap(shape1MinMax[0], shape1MinMax[1], shape2MinMax[0], shape2MinMax[1]))
            {
                //Debug.Log("falure");
                col.status = false;
                return col;
            }
        }
        col.status = true;
        col.contacts[0] = con0;
        return col;
    }
    
    public static HullCollision OBBOBBCollision(OBBHull boxHull1, OBBHull boxHull2)
    {
        Particle2D A = boxHull1.GetComponent<Particle2D>();
        Particle2D B = boxHull2.GetComponent<Particle2D>();
        Vector3[] shape1Corners;
        Vector3[] shape2Corners;
        shape1Corners = new Vector3[4];
        shape2Corners = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        float[] shape1MinMax = new float[2];
        float[] shape2MinMax = new float[2];

        shape1Corners = getRotatedCorners(boxHull1);
        shape2Corners = getRotatedCorners(boxHull2);

        normals[0] = getUpNormal(-boxHull1.currentRotation);
        normals[1] = getRightNormal(-boxHull1.currentRotation);
        normals[2] = getUpNormal(-boxHull2.currentRotation);
        normals[3] = getRightNormal(-boxHull2.currentRotation);

        HullCollision col = new HullCollision();
        col.a = boxHull1;
        col.b = boxHull2;
        Vector3 range = (boxHull2.transform.position + boxHull2.offset) - (boxHull1.transform.position + boxHull1.offset);
        //float xOverlap = boxHull1.halfX + boxHull2.halfX - Mathf.Abs(range.x);
        //float yOverlap = boxHull1.halfY + boxHull2.halfY - Mathf.Abs(range.y);

        //TRANSPORTATION

        //col.penetration = new Vector3(xOverlap, yOverlap);

        //Vector3 closingVel = A.velocity - B.velocity;
        Vector3 closingVel = B.velocity - A.velocity;
        col.closingVelocity = closingVel;

        HullCollision.Contact con0 = new HullCollision.Contact();
        con0.point = new Vector3(Mathf.Clamp(range.x, -boxHull1.halfX, boxHull1.halfX), Mathf.Clamp(range.y, -boxHull1.halfY, boxHull1.halfY));
        con0.restitution = Mathf.Min(boxHull1.restitution, boxHull2.restitution);
        con0.normal = range.normalized;

        for (int i = 0; i < normals.Length; i ++ )
        {
            //Debug.Log("testing corner" + i);

            shape1MinMax = SatTest(normals[i], shape1Corners);
            shape2MinMax = SatTest(normals[i], shape2Corners);
            if (!Overlap(shape1MinMax[0], shape1MinMax[1], shape2MinMax[0], shape2MinMax[1]))
            {
                //Debug.Log("falure");
                col.status = false;
                return col;

            }
        }
        col.status = true;
        col.contacts[0] = con0;
        return col;
    }
    
    
    static Vector3 getUpNormal(float theta)
    {
        float rad = theta * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), -Mathf.Sin(rad));
    }

    static Vector3 getRightNormal(float theta)
    {
        float rad = theta * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), Mathf.Cos(rad));
    }

    static float[] SatTest(Vector3 axis, Vector3[] points)
    {
        float[] minMax = new float[2];
        float minAlong = 1000000; float maxAlong = -1000000;
        for(int i = 0; i < points.Length; i++)
        {
            float dotValue = Vector3.Dot(points[i], axis);
            if (dotValue < minAlong) minAlong = dotValue;
            if (dotValue > maxAlong) maxAlong = dotValue;
        }
        minMax[0] = minAlong;
        minMax[1] = maxAlong;
        //Debug.Log(minMax[0] + " " + minMax[1]);
        return minMax;
    }

    static Vector3[] getRotatedCorners(OBBHull newHull)
    {
        Vector3[] returnPoints = new Vector3[4];
        returnPoints[0] = getRotatedPoint(new Vector3(newHull.transform.position.x - newHull.halfX, newHull.transform.position.y - newHull.halfY), newHull.transform.position, newHull.currentRotation);
        returnPoints[1] = getRotatedPoint(new Vector3(newHull.transform.position.x - newHull.halfX, newHull.transform.position.y + newHull.halfY), newHull.transform.position, newHull.currentRotation);
        returnPoints[2] = getRotatedPoint(new Vector3(newHull.transform.position.x + newHull.halfX, newHull.transform.position.y - newHull.halfY), newHull.transform.position, newHull.currentRotation);
        returnPoints[3] = getRotatedPoint(new Vector3(newHull.transform.position.x + newHull.halfX, newHull.transform.position.y + newHull.halfY), newHull.transform.position, newHull.currentRotation);

        return returnPoints;
    }

    public static Vector3 getRotatedPoint(Vector3 cornerPos, Vector3 centerPos, float theta)
    {
        float rad = theta * Mathf.Deg2Rad;

        float xPos = cornerPos.x - centerPos.x;
        float yPos = cornerPos.y - centerPos.y;
        float xRot = (xPos * Mathf.Cos(rad)) - (yPos * Mathf.Sin(rad));
        float yRot = (xPos * Mathf.Sin(rad)) + (yPos * Mathf.Cos(rad));
     
        Vector3 returnVector = new Vector3(xRot, yRot);

        returnVector += centerPos;
       
        return returnVector;
    }

    static bool Overlap(float min1, float max1, float min2, float max2)
    {
        return IsBetweenOrdered(min2, min1, max1) || IsBetweenOrdered(min1, min2, max2);
    }
    static bool IsBetweenOrdered(float val, float lowerBound, float upperBound)
    {
        return lowerBound <= val && val <= upperBound;
    }
}
