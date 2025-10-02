using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallScript : MonoBehaviour
{
    public Material wallMaterial;

    [Range(0f, 1f)]
    public float alpha = 1f;

    [Range(1f, 10f)]
    public float fadeSpeed = 1f;

    public bool fading;

    [SerializeField] Color myColor;

    private void Start()
    {
        wallMaterial = GetComponent<MeshRenderer>().material;
        wallMaterial.color = myColor;
        fading = false;
    }

    private void Update()
    {
        myColor.a = alpha;
        wallMaterial.color = myColor;
        
        if(fading && alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
        }

        if(!fading && alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
        }
    }

}
