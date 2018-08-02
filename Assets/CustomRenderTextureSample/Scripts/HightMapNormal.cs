using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HightMapNormal : MonoBehaviour
{
    [SerializeField]
    private RenderTexture _renderTexture;

    [SerializeField]
    private Material _material;

    private int _width;
    private int _height;
    private float _invWidth;
    private float _invHeight;

    private void Start()
    {
        _width = _renderTexture.width;
        _height = _renderTexture.height;
        _invWidth = 1f / _width;
        _invHeight = 1f / _height;
    }

    private void Update()
    {
        _material.SetFloatArray("_ParallaxMap_TexelSize", new[] { _invWidth, _invHeight });
    }
}
