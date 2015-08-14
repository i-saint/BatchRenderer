#ifndef CopyToTexture_h
#define CopyToTexture_h

#include <vector>
#include <cstdio>
#include <cstdint>

#define mpSafeRelease(obj) if(obj) { obj->Release(); obj=nullptr; }

inline int ceildiv(int v, int d)
{
    return v / d + (v%d == 0 ? 0 : 1);
}

enum DataConversion
{
    Float3ToFloat4,
    Float4ToFloat4,
    Float3ToHalf4,
    Float4ToHalf4,
};

struct float3
{
    float v[3];
    float& operator[](int i) { return v[i]; }
    const float& operator[](int i) const { return v[i]; }
};
struct float4
{
    float v[4];
    float& operator[](int i) { return v[i]; }
    const float& operator[](int i) const { return v[i]; }
};
struct half4
{
    uint16_t v[4];
    uint16_t& operator[](int i) { return v[i]; }
    const uint16_t& operator[](int i) const { return v[i]; }
};



class CopyToTextureBase
{
public:
    virtual ~CopyToTextureBase() {}
    virtual void copy(void *texptr, int width, int height, const void *data, int data_num, DataConversion conv)=0;

protected:
    int getDataSize(int data_num, DataConversion conv);
    const void* getDataPointer(const void *data, int data_num, int reserve_size, DataConversion conv, bool float2half_conversion);

    std::vector<float4> m_bufferf;
    std::vector<half4> m_bufferh;
};

CopyToTextureBase* CreateCopyToTextureD3D9(void *device);
CopyToTextureBase* CreateCopyToTextureD3D11(void *device);
CopyToTextureBase* CreateCopyToTextureOpenGL(void *device);


#endif // 