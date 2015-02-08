#include "UnityPluginInterface.h"
#include "CopyToTexture.h"

#if SUPPORT_D3D11

#include <windows.h>
#include <d3d11.h>



class CopyToTextureD3D11 : public CopyToTextureBase
{
public:
    CopyToTextureD3D11(void *dev);
    virtual ~CopyToTextureD3D11();
    virtual void copy(void *texptr, int width, int height, const void *data, int data_num, DataConversion conv);

private:
    ID3D11Device        *m_pDevice;
    ID3D11DeviceContext *m_pImmediateContext;
};

CopyToTextureBase* CreateCopyToTextureD3D11(void *device) { return new CopyToTextureD3D11(device); }

CopyToTextureD3D11::CopyToTextureD3D11(void *dev)
: m_pDevice(nullptr)
, m_pImmediateContext(nullptr)
{
    m_pDevice = (ID3D11Device*)dev;
    m_pDevice->GetImmediateContext(&m_pImmediateContext);
}

CopyToTextureD3D11::~CopyToTextureD3D11()
{
    mpSafeRelease(m_pImmediateContext);
}


void CopyToTextureD3D11::copy(void *texptr, int width, int height, const void *dataptr, int data_num, DataConversion conv)
{
    ID3D11Texture2D *tex = (ID3D11Texture2D*)texptr;
    dataptr = getDataPointer(dataptr, data_num, width*height, conv);

    D3D11_BOX box;
    box.left = 0;
    box.right = width;
    box.top = 0;
    box.bottom = ceildiv(data_num, width);
    box.front = 0;
    box.back = 1;
    m_pImmediateContext->UpdateSubresource(tex, 0, &box, dataptr, getDataSize(width, conv), 0);
}


#endif // SUPPORT_D3D11
