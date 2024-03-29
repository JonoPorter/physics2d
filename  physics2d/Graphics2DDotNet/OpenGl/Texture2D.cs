#region MIT License
/*
 * Copyright (c) 2005-2008 Jonathan Mark Porter. http://physics2d.googlepages.com/
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal 
 * in the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
 * the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion

#if UseDouble
using Scalar = System.Double;
#else
using Scalar = System.Single;
#endif
using System;
using System.Runtime.InteropServices;
using AdvanceMath;
using Tao.OpenGl;
using SdlDotNet.Graphics;
namespace Graphics2DDotNet
{
    public sealed class Texture2D : IDisposable
    {
        Surface surface;
        bool flip;
        TextureOptions options;
        int refresh;
        int textureID;

        public Texture2D(Surface surface, bool flip, TextureOptions options)
        {
            if (surface == null) { throw new ArgumentNullException("surface"); }
            if (options == null) { throw new ArgumentNullException("options"); }
            this.surface = surface;
            this.flip = flip;
            this.options = options;
            this.refresh = -1;
            this.textureID = -1;
        }
        ~Texture2D()
        {
            Dispose(false);
        }

        public void Buffer(int refresh)
        {
            this.refresh = refresh;
            this.textureID = TextureHelper.LoadTexture2D(surface, flip, options);
        }

        public void Bind()
        {
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, textureID);
        }
        private void Dispose(bool disposing)
        {
            GlHelper.GlDeleteTextures(refresh, new int[] { textureID });
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}