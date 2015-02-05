#ifndef CopyToTexture_h
#define CopyToTexture_h

#include <vector>
#include <cstdio>

#define mpSafeRelease(obj) if(obj) { obj->Release(); obj=nullptr; }

inline int ceildiv(int v, int d)
{
    return v / d + (v%d == 0 ? 0 : 1);
}


enum DataConversion
{
    Float3ToFloat4,
    Float4ToFloat4,
};

struct float3
{
    float v[3];
};

struct float4
{
    float v[4];
};

class CopyToTextureBase
{
public:
    virtual ~CopyToTextureBase() {}
    virtual void copy(void *texptr, int width, int height, const void *data, int data_num, DataConversion conv)=0;

protected:
    const void* getDataPointer(const void *data, int data_num, DataConversion conv);

    std::vector<float4> m_buffer;
};

CopyToTextureBase* CreateCopyToTextureD3D11(void *device);
CopyToTextureBase* CreateCopyToTextureOpenGL(void *device);


#endif // 