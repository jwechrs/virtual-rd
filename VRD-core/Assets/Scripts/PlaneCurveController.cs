using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneCurveController : MeshControllerMonoBehaviour
{
    public Material planeMaterial;
    public Material planeMaterialForegroundBackground;
     private float curvedPlaneWidth = 7.0f;
    private float curvedPlaneHeight = 3.5f;
    private float curvedPlaneRadius = 20.0f;
    private float curvedPlaneCenterY = 2.0f;
    private float curvedPlaneZ = 2.0f;
    public LineRenderer OVRSelectionVisualizer;
    public GameObject screenPointerObject;
    public SettingsPlaneController settingsPlaneController;
    public bool useForegroundBackgroundTexture = false;
    // Start is called before the first frame update
    void Start(){
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale=2.0f;
        // float curvedPlaneWidth = 7.0f;
        // float curvedPlaneHeight = 3.5f;
        // float curvedPlaneRadius = 10.0f;
        // Vector3 curvedPlaneCenter = new Vector3(
        //     0.0f, 2.0f-curvedPlaneHeight/2.0f, curvedPlaneZ);
        // Mesh curvedPlaneMesh = CurvedGeometry.getCurvedPlaneMesh(
        //     curvedPlaneCenter,
        //     curvedPlaneWidth, curvedPlaneHeight, curvedPlaneRadius, 20);
        //     GetComponent<MeshFilter>().sharedMesh = curvedPlaneMesh;

        updateCurvatureShape();
        if (useForegroundBackgroundTexture){
            GetComponent<MeshRenderer>().material = planeMaterialForegroundBackground;
        } else {
            GetComponent<MeshRenderer>().material = planeMaterial;
        }

        Texture2D sampleTextuew = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        sampleTextuew.filterMode = FilterMode.Point;
        Color initialPlainColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        Color[] samplePixelColors = new Color[]{
            initialPlainColor,
            initialPlainColor,
            initialPlainColor,
            initialPlainColor
         };
         sampleTextuew.SetPixels(samplePixelColors);
         sampleTextuew.Apply();
         if (useForegroundBackgroundTexture){
             GetComponent<MeshRenderer>().material.SetTexture("_MainTex", sampleTextuew);
         } else {
             GetComponent<MeshRenderer>().material.SetTexture("_EmissionMap", sampleTextuew);
         }
        //TNDebug.Log("mesh done");
        //Debug.Log(curvedPlaneMesh.vertices[1]);
        // CurvedGeometry.updateCurvatureRadius( 
        //     curvedPlaneMesh, curvedPlaneCenter,
        //     2.0f, 1.0f, 
        //     20, 5.0f
        // );
        //Debug.Log(curvedPlaneMesh.vertices[1]);
        //Debug.Log(GetComponent<MeshFilter>().sharedMesh.vertices[0]);
        changeCurvatureRadius(10.0f);

        // Vector2 pointerPosition = getPlane2DPosition(new Vector3[]{
        //     new Vector3(0, 0, 0),
        //     new Vector3(1, 1, 1)
        // });
        //Debug.Log("pointer position=" + pointerPosition);
    }
    private void updateCurvatureShape(){
        Vector3 curvedPlaneCenter = new Vector3(
            0.0f, 
            curvedPlaneCenterY-curvedPlaneHeight/2.0f, curvedPlaneZ);
        Mesh curvedPlaneMesh = CurvedGeometry.getCurvedPlaneMesh(
            curvedPlaneCenter,
            curvedPlaneWidth, curvedPlaneHeight, curvedPlaneRadius, 20);
        GetComponent<MeshFilter>().sharedMesh = curvedPlaneMesh;
        //GetComponent<MeshRenderer>().material = planeMaterial;
        // Vector2 pointerPosition = getPlane2DPosition(new Vector3[]{
        //     new Vector3(0, 0, 0),
        //     new Vector3(1, 1, 1)
        // });
    }

    // Update is called once per frame
    void Update()
    {
        //getPointerPosition();
    }
    new public void initMeshWithSize(float width, float height){
        // keep height
        curvedPlaneWidth = curvedPlaneHeight * width/height;
        // TNDebug.Log("width=" + width.ToString());
        // TNDebug.Log("height=" + height.ToString());
        // TNDebug.Log("new screen width=" + curvedPlaneWidth.ToString());
        updateCurvatureShape();
        // Vector3 curvedPlaneCenter = new Vector3(
        //     0.0f, 2.0f-height/2.0f, 2.0f);
        // Mesh curvedPlaneMesh = CurvedGeometry.getCurvedPlaneMesh(
        //     curvedPlaneCenter,
        //     curvedPlaneWidth, 
        //     curvedPlaneHeight, 
        //     curvedPlaneRadius, 20);
        // GetComponent<MeshFilter>().sharedMesh = curvedPlaneMesh;
        //GetComponent<MeshRenderer>().material = planeMaterial;
    }
    public void changeCurvatureRadius(float newRadius){
        if (newRadius == curvedPlaneRadius){
            return;
        }
        curvedPlaneRadius = newRadius;
        updateCurvatureShape();
    }
    public void changeCurvatureZ(float newZ){
        if (curvedPlaneZ == newZ){
            return;
        }
        curvedPlaneZ = newZ;
        updateCurvatureShape();
    }
    new public void setTexture(Texture2D displayTexture){
        TNDebug.Log("setTexture @ PlaneCurveController");
        if (useForegroundBackgroundTexture){
            GetComponent<MeshRenderer>().material.SetTexture("_MainTex", displayTexture);
        } else {
            GetComponent<MeshRenderer>().material.SetTexture("_EmissionMap", displayTexture);
        }
    }
    public void setTextureForegroundBackground(Texture2D foregroundTexture, Texture2D backgroundTexture){
        // Hextileで背景に指定されているものはbackground textureとして処理
        // その上からforeground textureを描写
        // main alpha=0のときのみbackgroundが表示
        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", foregroundTexture);
        GetComponent<MeshRenderer>().material.SetTexture("_SubTex", backgroundTexture);
    }
    public Vector2 getPointerPosition(){
        //TNDebug.Log("getPointerPosition");
        Vector3[] selectionVisualizerPositions = new Vector3[OVRSelectionVisualizer.positionCount];
        OVRSelectionVisualizer.GetPositions(selectionVisualizerPositions);
        return getPlane2DPosition(selectionVisualizerPositions);
    }

    public bool focusOnScreen(){
        if (settingsPlaneController.isFocused){
            screenPointerObject.SetActive(false);
            //TNDebug.Log("settingsPlane is active");
            return false;
        }
        //TNDebug.Log("screen focus");
        screenPointerObject.SetActive(true);
        return true;
    }
    private Vector2 getPlane2DPosition(Vector3[] selectionVisualizerPositions){
        // Returns relative position (x, y)
        // (0, 0) --> topleft
        // (1, 1) --> bottom right
        Vector3 lineDirection = selectionVisualizerPositions[1]-selectionVisualizerPositions[0];
        Vector2 xzLineDirection = new Vector2(lineDirection.x, lineDirection.z);
        float quad_eq_a = lineDirection.x *lineDirection.x + lineDirection.z * lineDirection.z;//Mathf.Pow(lineDirection.x +lineDirection.z, 2.0f);
        float deltaX = selectionVisualizerPositions[0].x;
        float deltaZ = selectionVisualizerPositions[0].z - (curvedPlaneZ-curvedPlaneRadius);
        float quad_eq_b = 2 * (deltaX * lineDirection.x + deltaZ * lineDirection.z);
        float quad_eq_c = Mathf.Pow(deltaX, 2.0f)+deltaZ*deltaZ-curvedPlaneRadius*curvedPlaneRadius;
        float k = (-quad_eq_b+Mathf.Sqrt(quad_eq_b*quad_eq_b-4 * quad_eq_a*quad_eq_c))/(2.0f * quad_eq_a);
        Vector3 pointer3DPosition = new Vector3(
            selectionVisualizerPositions[0].x + k * lineDirection.x,
            selectionVisualizerPositions[0].y + k * lineDirection.y,
            selectionVisualizerPositions[0].z + k * lineDirection.z
        );
        screenPointerObject.transform.position = pointer3DPosition;
        float pointerAngle = Mathf.Atan2(
            pointer3DPosition.x,
            pointer3DPosition.z-(curvedPlaneZ-curvedPlaneRadius)
        );
        //screenPointerObject.transform.rotation = Quaternion.Identity;
        screenPointerObject.transform.rotation = Quaternion.Euler(90.0f,Mathf.Rad2Deg * pointerAngle, 0.0f);
        //screenPointerObject.transform.LookAt(new Vector3(0.0f, pointer3DPosition.y, curvedPlaneZ-curvedPlaneRadius));
        float relativeX = curvedPlaneRadius * pointerAngle/curvedPlaneWidth + 0.50f;
        float relativeY = 1.0f - (pointer3DPosition.y-curvedPlaneCenterY)/curvedPlaneHeight - 0.5f;
        if (relativeX <= 1 && relativeX >= 0 && relativeY <= 1 && relativeY >= 0){
            //TNDebug.Log("pointer inside screen");
            screenPointerObject.SetActive(true);
        } else {
            //TNDebug.Log("pointer out of screen");
            screenPointerObject.SetActive(false);
        }
        return new Vector2(relativeX, relativeY);
        //return new Vector2(0.0f, 0.0f);
    }
}
