using UnityEngine;
using UnityEngine.UI;
using System;

public class Main : MonoBehaviour
{
    [SerializeField] RawImage image;
    [SerializeField] ComputeShader computeShader;

    RenderTexture renderTexture;
    int size = 1024;
    int count;
    int kernelIndex;
    void Start()
    {
        renderTexture = new RenderTexture(size, size, 1, RenderTextureFormat.ARGB32);
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Point;//読み取り時に自動補間しない
        renderTexture.wrapMode = TextureWrapMode.Clamp;//境界条件みたいな
        renderTexture.Create();
        image.texture = renderTexture;

        if (SystemInfo.supportsComputeShaders)
        {
            kernelIndex = computeShader.FindKernel("CSMain");
            computeShader.SetTexture(kernelIndex, "Tex", renderTexture);
        }
        count = 0;
    }

    void Update()
    {
        if (SystemInfo.supportsComputeShaders)
        {
            computeShader.SetInt("time", count % 1024);
            computeShader.Dispatch(kernelIndex, size / 64, size, 1);
        }
        count++;
    }
}
