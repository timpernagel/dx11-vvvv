StructuredBuffer<float2> uvBuffer;
StructuredBuffer<float3> uvLevelBuffer;

RWStructuredBuffer<float4> OutputBuffer;

Texture2D inputTexture;

SamplerState inputSampler 
{
    Filter = MIN_MAG_MIP_LINEAR; 
    AddressU = CLAMP; 
    AddressV = CLAMP;
};

cbuffer cbData : register(b0)
{
    uint TotalCount = 1;
};


cbuffer cbSamplerData : register(b1)
{
    float MipLevel = 0;
};


[numthreads(64, 1, 1)]
void CS_ConstantLevel(uint3 DTid : SV_DispatchThreadID) 
{
    if (DTid.x >= TotalCount)
        return;

    float2 uv = uvBuffer[DTid.x];
    OutputBuffer[DTid.x] = inputTexture.SampleLevel(inputSampler, uv, MipLevel);
}

[numthreads(64, 1, 1)]
void CS_DynamicLevel(uint3 DTid : SV_DispatchThreadID)
{
    if (DTid.x >= TotalCount)
        return;

    float3 uv = uvLevelBuffer[DTid.x];
    OutputBuffer[DTid.x] = inputTexture.SampleLevel(inputSampler, uv.xy, uv.z);
}



technique11 ConstantLevel 
{ 
    pass P0 
    { 
        SetComputeShader(CompileShader(cs_5_0, CS_ConstantLevel())); 
    } 
}

technique11 DynamicLevel 
{ 
    pass P0 
    { 
        SetComputeShader(CompileShader(cs_5_0, CS_DynamicLevel())); 
    } 
}







