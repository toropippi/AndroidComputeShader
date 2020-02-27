using UnityEngine;
using UnityEngine.UI;
using System;

public class Main : MonoBehaviour
{
    [SerializeField] RawImage image;
    [SerializeField] ComputeShader computeShader;

    RenderTexture renderTexture;
    int size = 1024;
    public int count;
    int kernelIndex;
    int[] data;

    ComputeBuffer buffer;
    ComputeBuffer atomicbuffer;
    void Start()
    {
        data = new int[size * size];
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

            buffer = new ComputeBuffer(size*size, sizeof(int));
            computeShader.SetBuffer(kernelIndex, "A", buffer);

            atomicbuffer = new ComputeBuffer(1, sizeof(int));
            computeShader.SetBuffer(kernelIndex, "atmicBUF", atomicbuffer);
        }
        count = 0;
    }

    void Update()
    {
        if (SystemInfo.supportsComputeShaders)
        {
            computeShader.SetInt("time", count % 1024);
            computeShader.Dispatch(kernelIndex, size / 64, size, 1);
            if (count % 32 == 13) 
            {
                buffer.GetData(data);
                count += data[count % (1024 * 1024)];
                count %= (1024 * 1024);

                uint[] tmp=new uint[1];
                atomicbuffer.GetData(tmp);
                count += (int)Mathf.Abs(tmp[0]);
                if (count < 0) count += 2100000000;
                count %= (1024 * 1024);
                count += 1024 * 1024;
            }
            
        }
        count++;
    }
}
