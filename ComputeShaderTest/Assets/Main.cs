using UnityEngine;
using UnityEngine.UI;
using System;

public class Main : MonoBehaviour
{
    [SerializeField] RawImage image;
    [SerializeField] ComputeShader computeShader;
    public Text text;

    RenderTexture renderTexture;
    int size = 2048;
    int count;
    int kernelIndex;
    float mx = 0, my = 0;

    ComputeBuffer atomicbuffer;
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

            atomicbuffer = new ComputeBuffer(1, sizeof(int));
            computeShader.SetBuffer(kernelIndex, "atmicBUF", atomicbuffer);
        }
        count = 0;
    }

    void Update()
    {
        //計算
        if (SystemInfo.supportsComputeShaders)
        {
            computeShader.SetFloat("mx", mx);
            computeShader.SetFloat("my", my);
            computeShader.Dispatch(kernelIndex, size / 64, size, 1);
            
            uint[] atmcb=new uint[1];
            atomicbuffer.GetData(atmcb);
            text.text = "" + atmcb[0] + "";
            atmcb[0] = 0;
            atomicbuffer.SetData(atmcb);
        }

        //タッチ操作
        if (Input.touchCount > 0)
        {
            
            Touch touch = Input.GetTouch(0);
            mx += 0.1f*touch.deltaPosition.x;
            my += 0.1f*touch.deltaPosition.y;
            Debug.Log(touch.deltaPosition.y);

        }

        mx -= 0.007f * Mathf.Sin(0.05f*count);
        count++;
    }
}
