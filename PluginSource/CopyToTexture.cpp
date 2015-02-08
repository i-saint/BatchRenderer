#include "UnityPluginInterface.h"
#include "CopyToTexture.h"


// float -> half conversion.
// thanks to @rygorous https://gist.github.com/rygorous/2156668

typedef uint32_t uint;

union FP32
{
    uint u;
    float f;
    struct
    {
        uint Mantissa : 23;
        uint Exponent : 8;
        uint Sign : 1;
    };
};

union FP16
{
    unsigned short u;
    struct
    {
        uint Mantissa : 10;
        uint Exponent : 5;
        uint Sign : 1;
    };
};

static FP16 float_to_half_fast3(FP32 f)
{
    FP32 f32infty = { 255 << 23 };
    FP32 f16infty = { 31 << 23 };
    FP32 magic = { 15 << 23 };
    uint sign_mask = 0x80000000u;
    uint round_mask = ~0xfffu;
    FP16 o = { 0 };

    uint sign = f.u & sign_mask;
    f.u ^= sign;

    // NOTE all the integer compares in this function can be safely
    // compiled into signed compares since all operands are below
    // 0x80000000. Important if you want fast straight SSE2 code
    // (since there's no unsigned PCMPGTD).

    if (f.u >= f32infty.u) // Inf or NaN (all exponent bits set)
        o.u = (f.u > f32infty.u) ? 0x7e00 : 0x7c00; // NaN->qNaN and Inf->Inf
    else // (De)normalized number or zero
    {
        f.u &= round_mask;
        f.f *= magic.f;
        f.u -= round_mask;
        if (f.u > f16infty.u) f.u = f16infty.u; // Clamp to signed infinity if overflowed

        o.u = f.u >> 13; // Take the bits!
    }

    o.u |= sign >> 16;
    return o;
}

int CopyToTextureBase::getDataSize(int data_num, DataConversion conv)
{
    switch (conv) {
    case Float4ToFloat4:
    case Float3ToFloat4:
        return 16 * data_num;
        break;

    case Float4ToHalf4:
    case Float3ToHalf4:
        return 8 * data_num;
        break;

    }
    return 0;
}

const void* CopyToTextureBase::getDataPointer(const void *data, int data_num, DataConversion conv)
{
    if (conv == Float4ToFloat4) {
        return data;
    }
    else if (conv == Float3ToFloat4) {
        m_bufferf.resize(data_num);
        const float3 *src = (const float3*)data;
        for (int i = 0; i < data_num; ++i) {
            m_bufferf[i] = (float4&)src[i];
        }
        return &m_bufferf[0];
    }
    else if (conv == Float4ToHalf4) {
        m_bufferh.resize(data_num);
        const float4 *src = (const float4*)data;
        for (int i = 0; i < data_num; ++i) {
            (FP16&)m_bufferf[i][0] = float_to_half_fast3((const FP32&)src[i][0]);
            (FP16&)m_bufferf[i][1] = float_to_half_fast3((const FP32&)src[i][1]);
            (FP16&)m_bufferf[i][2] = float_to_half_fast3((const FP32&)src[i][2]);
            (FP16&)m_bufferf[i][3] = float_to_half_fast3((const FP32&)src[i][3]);
        }
        return &m_bufferh[0];
    }
    else if (conv == Float3ToHalf4) {
        m_bufferh.resize(data_num);
        const float3 *src = (const float3*)data;
        for (int i = 0; i < data_num; ++i) {
            (FP16&)m_bufferf[i][0] = float_to_half_fast3((const FP32&)src[i][0]);
            (FP16&)m_bufferf[i][1] = float_to_half_fast3((const FP32&)src[i][1]);
            (FP16&)m_bufferf[i][2] = float_to_half_fast3((const FP32&)src[i][2]);
        }
        return &m_bufferh[0];
    }
    return nullptr;
}
