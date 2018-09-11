using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using FeralTic.DX11;
using FeralTic.DX11.Resources;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.DX11.Lib.Devices;

using SlimDX.Direct3D11;
using SlimDX.DXGI;

using Device = SlimDX.Direct3D11.Device;

namespace FeralTic.DX11.Resources
{
    public class DX11Texture3D_Own : DX11Texture3D
    {
        public bool isowner;
        protected Texture3DDescription desc;

        public DX11Texture3D_Own(DX11RenderContext context) : base(context)
        {
        }

        public static DX11Texture3D_Own FromDescription(DX11RenderContext context, Texture3DDescription desc)
        {
            DX11Texture3D_Own res = new DX11Texture3D_Own(context);
            res.context = context;
            res.Resource = new Texture3D(context.Device, desc);
            res.isowner = true;
            res.desc = desc;
            res.SRV = new ShaderResourceView(context.Device, res.Resource);

            return res;
        }
    }
}