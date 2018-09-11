﻿using System;
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

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "FrameDelay", Category = "DX11.Texture", Version = "2d",
        AutoEvaluate=true,      
        Author = "vux",
        Warnings="non spreadable")]
    public class FrameDelayTextureNode : IPluginEvaluate, IDX11ResourceHost, IDisposable, IDX11RenderStartPoint
    {
        [Input("Texture In", IsSingle = true,AutoValidate =false)]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureInput;

        [Input("Flush", IsSingle = true)]
        protected ISpread<bool> FInFlush;

        [Input("Enabled", IsSingle = true, DefaultValue =1)]
        protected ISpread<bool> FInEnabled;

        [Output("Texture Out", IsSingle = true, AllowFeedback=true)]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureOutput;

        private IHDEHost hde;
        private DX11Texture2D lasttexture = null;
        private ILogger logger;

        public DX11RenderContext RenderContext
        {
            get { return DX11GlobalDevice.DeviceManager.RenderContexts[0]; }
        }
        
        public bool Enabled
        {
            get { return this.FInEnabled.SliceCount > 0 ? this.FInEnabled[0] : false; }
        }

        [ImportingConstructor()]
        public FrameDelayTextureNode(IHDEHost hde, ILogger logger)
        {
            this.hde = hde;
            this.logger = logger;
        }

        public void Evaluate(int SpreadMax)
        {
            if (SpreadMax > 0 && this.FInEnabled[0])
            {
                this.FTextureInput.Sync();
            }

            if (this.FTextureOutput[0] == null)
            {
                this.FTextureOutput[0] = new DX11Resource<DX11Texture2D>();
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (this.lasttexture != null)
            {
                this.FTextureOutput[0][context] = this.lasttexture;
            }
            else
            {
                this.FTextureOutput[0].Dispose(context);
            }
        }

        public void Present()
        {
            DX11RenderContext context = this.RenderContext;


            //Rendering is finished, so should be ok to grab texture now
            if (this.FTextureInput.IsConnected && this.FTextureInput.SliceCount > 0 && this.FTextureInput[0].Contains(context))
            {
                DX11Texture2D texture = this.FTextureInput[0][context];

                if (texture != null)
                {
                    if (texture is DX11DepthStencil)
                    {
                        this.logger.Log(LogType.Warning, "FrameDelay for depth texture is not supported");
                        return;
                    }

                    if (this.lasttexture != null)
                    {
                        if (this.lasttexture.Description != texture.Description) { this.DisposeTexture(); }
                    }

                    if (this.lasttexture == null)
                    {
                        this.lasttexture = DX11Texture2D.FromDescription(context, texture.Description);
                    }

                    context.CurrentDeviceContext.CopyResource(texture.Resource, this.lasttexture.Resource);

                    if (this.FInFlush[0]) { context.CurrentDeviceContext.Flush(); }
                }
            }
            else
            {
                this.DisposeTexture();
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {

        }

        public void Dispose()
        {
            this.DisposeTexture();
        }

        private void DisposeTexture()
        {
            if (this.lasttexture != null) { this.lasttexture.Dispose(); this.lasttexture = null; }
        }

    }
}
