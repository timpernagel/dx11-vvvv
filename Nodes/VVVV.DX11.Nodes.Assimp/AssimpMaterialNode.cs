﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using AssimpNet;
using SlimDX;
using VVVV.Utils.VColor;

namespace VVVV.DX11.Nodes.AssetImport
{
    [PluginInfo(Name = "Material", Category = "Assimp", Author = "vux,flateric")]
    public class AssimpMaterialNode : IPluginEvaluate
    {
        [Input("Scene", IsSingle = true)]
        protected IDiffSpread<AssimpScene> FInScene;

        [Output("Ambient Color")]
        protected ISpread<RGBAColor> FOutAmbient;

        [Output("Diffuse Color")]
        protected ISpread<RGBAColor> FOutDiffuse;

        [Output("Specular Color")]
        protected ISpread<RGBAColor> FOutSpecular;

        [Output("Specular Power")]
        protected ISpread<float> FOutPower;

        [Output("Texture Type")]
        protected ISpread<eAssimpTextureType> FOutTexType;

        [Output("Texture Path",BinName="Texture Count",BinOrder=30)]
        protected ISpread<ISpread<string>> FOutTexPath;

        [Output("Texture Operation")]
        protected ISpread<eAssimpTextureOp> FOutTexOp;

        [Output("Texture Map Mode")]
        protected ISpread<eAssimpTextureMapMode> FOutTexMapMode;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInScene.IsChanged)
            {
                if (this.FInScene[0] != null)
                {
                    int matcount = this.FInScene[0].Materials.Count;

                    this.FOutTexPath.SliceCount = matcount;
                    this.FOutAmbient.SliceCount = matcount;
                    this.FOutSpecular.SliceCount = matcount;
                    this.FOutDiffuse.SliceCount = matcount;
                    this.FOutPower.SliceCount = matcount;

                    List<eAssimpTextureType> types = new List<eAssimpTextureType>();
                    List<eAssimpTextureMapMode> mapmodes = new List<eAssimpTextureMapMode>();
                    List<eAssimpTextureOp> ops = new List<eAssimpTextureOp>();
                    
                    for (int i = 0; i < this.FInScene[0].Materials.Count; i++)
                    {
                        AssimpMaterial mat = this.FInScene[0].Materials[i];

                        this.FOutAmbient[i] = new RGBAColor(mat.AmbientColor.Red, mat.AmbientColor.Green, mat.AmbientColor.Blue, mat.AmbientColor.Alpha);
                        this.FOutDiffuse[i] = new RGBAColor(mat.DiffuseColor.Red, mat.DiffuseColor.Green, mat.DiffuseColor.Blue, mat.DiffuseColor.Alpha);
                        this.FOutSpecular[i] = new RGBAColor(mat.SpecularColor.Red, mat.SpecularColor.Green, mat.SpecularColor.Blue, mat.SpecularColor.Alpha);
                        this.FOutPower[i] = mat.SpecularPower;

                        this.FOutTexPath[i].SliceCount = mat.TexturePath.Count;
                        
                        types.AddRange(mat.TextureType);
                        mapmodes.AddRange(mat.TextureMapMode);
                        ops.AddRange(mat.TextureOperation);
                        for (int j = 0; j < mat.TexturePath.Count; j++)
                        {
                            this.FOutTexPath[i][j] = mat.TexturePath[j];
                        }
                    }
                    this.FOutTexType.AssignFrom(types);
                    this.FOutTexMapMode.AssignFrom(mapmodes);
                    this.FOutTexOp.AssignFrom(ops);
                }
                else
                {
                    this.FOutTexPath.SliceCount = 0;
                    this.FOutTexType.SliceCount = 0;
                    this.FOutTexMapMode.SliceCount = 0;
                    this.FOutTexOp.SliceCount = 0;
                    this.FOutAmbient.SliceCount = 0;
                    this.FOutDiffuse.SliceCount = 0;
                    this.FOutPower.SliceCount = 0;
                    this.FOutSpecular.SliceCount = 0;
                }
                

            }
        }

    }
}
