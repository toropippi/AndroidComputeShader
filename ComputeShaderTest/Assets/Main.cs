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
    float mx, my;
    float backDist, scale, lastscale;

    ComputeBuffer atomicbuffer;
    void Start()
    {
        //RenderTexture関連
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
        mx = 0;
        my = 0;
        scale = 1f;
        backDist = 0f;
    }

    void Update()
    {
        //計算
        if (SystemInfo.supportsComputeShaders)
        {
            computeShader.SetFloat("mx", mx);
            computeShader.SetFloat("my", my);
            computeShader.SetFloat("sc", scale);
            computeShader.Dispatch(kernelIndex, size / 64, size, 1);
            //毎フレームGPUからデータをとってきてCPUから文字をprint
            uint[] atmcb=new uint[1];
            atomicbuffer.GetData(atmcb);
            text.text = "" + atmcb[0] + "";
            atmcb[0] = 0;
            atomicbuffer.SetData(atmcb);
        }

        //タッチ操作
        UITouch();

        mx -= 0.007f * Mathf.Sin(0.05f * count) * scale;
        count++;
    }


    void UITouch() 
    {

        //タッチ操作
        if (Input.touchCount == 1)
        {
            Touch touch1 = Input.GetTouch(0);
            mx -= 0.001f * scale * touch1.deltaPosition.x;
            my -= 0.001f * scale * touch1.deltaPosition.y;
        }

        //タッチ操作
        if (Input.touchCount == 2)
        {

            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            // 2点タッチ開始時の距離を記憶
            if (touch2.phase == TouchPhase.Began)
            {
                backDist = Vector2.Distance(touch1.position, touch2.position);
            }
            else if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
            {
                // タッチ位置の移動後、長さを再測し、前回の距離からの相対値を取る。
                float newDist = Vector2.Distance(touch1.position, touch2.position);
                scale = lastscale + (newDist - backDist) / 1024.0f;
            }
        }
        else 
        {
            lastscale = scale;
        }

        return;
    }
}
