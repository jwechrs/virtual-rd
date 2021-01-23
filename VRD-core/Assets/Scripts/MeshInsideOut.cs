using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshInsideOut : MonoBehaviour
{
    // Start is called before the first frame update
    void Start(){
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.triangles = mesh.triangles.Reverse().ToArray();
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale=2.0f;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
