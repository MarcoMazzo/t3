﻿using SharpDX.Direct3D11;

namespace T3.Core.DataTypes
{
    public class Texture3d
    {
        public SharpDX.Direct3D11.Texture3D Texture;
        public SharpDX.Direct3D11.ShaderResourceView Srv;
        public SharpDX.Direct3D11.UnorderedAccessView Uav;
        public SharpDX.Direct3D11.RenderTargetView Rtv;
    }
}