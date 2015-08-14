#include "UnityPluginInterface.h"
#include "CopyToTexture.h"

#if SUPPORT_D3D9

#include <windows.h>
#include <d3d9.h>
#include <map>



class CopyToTextureD3D9 : public CopyToTextureBase
{
public:
    CopyToTextureD3D9(void *dev);
    virtual ~CopyToTextureD3D9();
    virtual void copy(void *texptr, int width, int height, const void *data, int data_num, DataConversion conv);

private:
    void clearStagingTextures();
    IDirect3DSurface9* findOrCreateStagingTexture(int width, int height);

private:
    IDirect3DDevice9 *m_device;
    std::map<uint64_t, IDirect3DSurface9*> m_staging_textures;
};

CopyToTextureBase* CreateCopyToTextureD3D9(void *device) { return new CopyToTextureD3D9(device); }

CopyToTextureD3D9::CopyToTextureD3D9(void *dev)
    : m_device((IDirect3DDevice9*)dev)
{
}

CopyToTextureD3D9::~CopyToTextureD3D9()
{
    for (auto& pair : m_staging_textures)
    {
        pair.second->Release();
    }
    m_staging_textures.clear();
}


IDirect3DSurface9* CopyToTextureD3D9::findOrCreateStagingTexture(int width, int height)
{
    D3DFORMAT internal_format = D3DFMT_A32B32G32R32F;

    uint64_t hash = width + (height << 16);
    {
        auto it = m_staging_textures.find(hash);
        if (it != m_staging_textures.end())
        {
            return it->second;
        }
    }

    IDirect3DSurface9 *ret = nullptr;
    HRESULT hr = m_device->CreateOffscreenPlainSurface(width, height, internal_format, D3DPOOL_SYSTEMMEM, &ret, NULL);
    if (SUCCEEDED(hr))
    {
        m_staging_textures.insert(std::make_pair(hash, ret));
    }
    return ret;
}

void CopyToTextureD3D9::copy(void *texptr, int width, int height, const void *dataptr, int data_num, DataConversion conv)
{
    int psize = 16;
    int pitch = psize * width;
    int bufsize = data_num * psize;
    dataptr = getDataPointer(dataptr, data_num, width*height, conv, true);

    HRESULT hr;
    IDirect3DTexture9 *tex = (IDirect3DTexture9*)texptr;

    // D3D11 と違い、D3D9 では書き込みも staging texture を経由する必要がある。
    IDirect3DSurface9 *surf_src = findOrCreateStagingTexture(width, height);
    if (surf_src == nullptr) { return; }

    IDirect3DSurface9* surf_dst = nullptr;
    hr = tex->GetSurfaceLevel(0, &surf_dst);
    if (FAILED(hr)) { return; }

    bool ret = false;
    D3DLOCKED_RECT locked;
    hr = surf_src->LockRect(&locked, nullptr, D3DLOCK_DISCARD);
    if (SUCCEEDED(hr))
    {
        const char *rpixels = (const char*)dataptr;
        int rpitch = psize * width;
        char *wpixels = (char*)locked.pBits;
        int wpitch = locked.Pitch;

        memcpy(wpixels, rpixels, bufsize);
        surf_src->UnlockRect();

        hr = m_device->UpdateSurface(surf_src, nullptr, surf_dst, nullptr);
        if (SUCCEEDED(hr)) {
            ret = true;
        }
    }
    surf_dst->Release();
}


#endif // SUPPORT_D3D9
