#include "UnityPluginInterface.h"
#include "CopyToTexture.h"

#if SUPPORT_OPENGL
#if UNITY_WIN
#include <windows.h>
#include <gl/GL.h>
#else
#include <OpenGL/gl.h>
#include <OpenGL/glext.h>
#endif


class CopyToTextureOpenGL : public CopyToTextureBase
{
public:
    CopyToTextureOpenGL();
    ~CopyToTextureOpenGL();
    virtual void copy(void *tex, int width, int height, const void *data, int data_num, DataConversion conv);

private:
};

CopyToTextureBase* CreateCopyToTextureOpenGL(void *device) { return new CopyToTextureOpenGL(); }


CopyToTextureOpenGL::CopyToTextureOpenGL()
{
}

CopyToTextureOpenGL::~CopyToTextureOpenGL()
{
}

void CopyToTextureOpenGL::copy(void *tex, int width, int height, const void *dataptr, int data_num, DataConversion conv)
{
    int w = width;
    int h = ceildiv(data_num, width);
    dataptr = getDataPointer(dataptr, data_num, width*height, conv);

    glBindTexture(GL_TEXTURE_2D, (GLuint)(size_t)tex);
    glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, w, h, GL_RGBA, GL_FLOAT, dataptr);
    glBindTexture(GL_TEXTURE_2D, 0);
}

#endif // SUPPORT_OPENGL
