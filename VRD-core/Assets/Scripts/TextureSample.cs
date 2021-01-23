using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureSample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
         Texture2D sampleTextuew = new Texture2D(10, 10, TextureFormat.RGBA32, false);
        sampleTextuew.filterMode = FilterMode.Point;
        // Color[] samplePixelColors = new Color[]{
        //     new Color(1.0f, 0.0f, 0.0f, 1.0f),
        //     new Color(1.0f, 0.0f, 0.0f, 1.0f),
        //     new Color(1.0f, 1.0f, 0.0f, 1.0f),
        //     new Color(1.0f, 1.0f, 0.0f, 1.0f)
        //  };
        Color[] samplePixelColors = new Color[10*10];
        for(int i=0; i<10*10; i++){
            samplePixelColors[i] = new Color(1.0f, 1.0f, 0.0f, 1.0f);
        }
         sampleTextuew.SetPixels(samplePixelColors);
         sampleTextuew.Apply();
         GetComponent<Renderer>().material.SetTexture("_EmissionMap", sampleTextuew);
         //GetComponent<Renderer>().material.mainTexture = sampleTextuew;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
