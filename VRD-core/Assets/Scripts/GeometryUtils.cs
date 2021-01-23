using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CurvedGeometry{
    public static Mesh updateCurvatureRadius(
        Mesh mesh,
        Vector3 centerPosition,
        float width, float height,
        int numberOfDeltaPlanes, float radius){
        float maxAngle = width/(radius*2);
        float minAngle = -maxAngle;
        float originZ = -radius;//-3.0f;
         float deltaAngle = (maxAngle-minAngle)/(float)numberOfDeltaPlanes;
        //Debug.Log("number of delta planes =" + numberOfDeltaPlanes);
         Vector3[] newVertices = new Vector3[mesh.vertices.Length];
        for(int deltaPlaneIndex=0; deltaPlaneIndex<=numberOfDeltaPlanes; deltaPlaneIndex++){
            float angle = deltaAngle * deltaPlaneIndex + minAngle;
            float x = radius * Mathf.Sin(angle) + centerPosition.x;
            // Debug.Log("angle=" + angle.ToString());
            // Debug.Log("x=" + x.ToString());
            //Debug.Log(minAngle);
            //loat y = radius * Mathf.Sin(angle);
            //Debug.Log("new height=" + height.ToString());
            float z = radius * Mathf.Cos(angle) + originZ + centerPosition.z;
            newVertices[2 * deltaPlaneIndex] = new Vector3(x, centerPosition.y, z);
            newVertices[2 * deltaPlaneIndex+1] = new Vector3(x, height +centerPosition.y, z);
            // float deltaIndexRel = deltaPlaneIndex/(float)numberOfDeltaPlanes;
            // Debug.Log(deltaIndexRel);
            // meshUVs[2 * deltaPlaneIndex = new Vector2(
            //     deltaIndexRel, 0
            // );
            // meshUVs[2*deltaPlaneIndex+1] = new Vector2(deltaIndexRel, 1);
        }
        mesh.vertices = newVertices;
        return mesh;
    }

    public static Mesh getCurvedPlaneMesh(
        Vector3 centerPosition,
        float width, float height, float radius, int numberOfDeltaPlanes){
        Mesh mesh = new Mesh();
        //int numberOfDeltaPlanes = 20;

        Vector3[] meshVertices = new Vector3[2 * (numberOfDeltaPlanes+1)];
        Vector2[] meshUVs = new Vector2[2 * (numberOfDeltaPlanes+1)];
        // mesh.vertices = new Vector3[]{
        //     new Vector3(-1.0f, -1.0f,0.0f),
        //     new Vector3(-1.0f, 1.0f,0),
        //     new Vector3(1.0f, -1.0f, 0.0f),
        //     new Vector3(1.0f, 1.0f, 1.0f)
        // };
        // mesh.uv = new Vector2[]{
        //     new Vector2(0, 0),
        //     new  Vector2(0, 1),
        //     new Vector2(1,0),
        //     new Vector2(1,1)
        // };
        //float pi = 3.1415926f;
        // float minAngle = -pi/3.0f;
        // float maxAngle = pi/3.0f;
        float maxAngle = width/(radius*2);
        float minAngle = -maxAngle;
        float originZ = -radius;//-3.0f;
        //float radius = -originZ;
        float deltaAngle = (maxAngle-minAngle)/(float)numberOfDeltaPlanes;
        for(int deltaPlaneIndex=0; deltaPlaneIndex<=numberOfDeltaPlanes; deltaPlaneIndex++){
            float angle = deltaAngle * deltaPlaneIndex + minAngle;
            float x = radius * Mathf.Sin(angle) + centerPosition.x;
           // Debug.Log("angle=" + angle.ToString());
            //Debug.Log("x=" + x.ToString());
            //Debug.Log(minAngle);
            //loat y = radius * Mathf.Sin(angle);
            float z = radius * Mathf.Cos(angle) + originZ + centerPosition.z;
            meshVertices[2 * deltaPlaneIndex] = new Vector3(x, centerPosition.y, z);
            meshVertices[2 * deltaPlaneIndex+1] = new Vector3(x, height +centerPosition.y, z);
            float deltaIndexRel = deltaPlaneIndex/(float)numberOfDeltaPlanes;
            //Debug.Log(deltaIndexRel);
            meshUVs[2 * deltaPlaneIndex] = new Vector2(1-deltaIndexRel, 1);
            meshUVs[2*deltaPlaneIndex+1] = new Vector2(1-deltaIndexRel, 0);
        }
        int[] meshTriangles = new int[2 * numberOfDeltaPlanes * 3];
        for(int i=0; i<numberOfDeltaPlanes; i++){
            meshTriangles[6 * i] = 2 *i;
            meshTriangles[6*i+1] = 2*i+1;
            meshTriangles[6*i+2] = 2*i+2;
            meshTriangles[6*i+3] = 2*i+1;
            meshTriangles[6*i+4] = 2*i+3;
            meshTriangles[6*i+5] = 2*i+2;
        }

        //mesh.triangles = new int[]{0, 1, 2, 1, 3, 2};
        mesh.vertices = meshVertices;
        mesh.uv = meshUVs;
        mesh.triangles = meshTriangles;
        return mesh;
    }
}