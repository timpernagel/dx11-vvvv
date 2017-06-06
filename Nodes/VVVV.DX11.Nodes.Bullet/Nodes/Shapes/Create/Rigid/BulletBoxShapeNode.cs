﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;

using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;


namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="Box",Category="Bullet",Author="vux")]
	public class BulletBoxShapeNode : AbstractBulletRigidDynamicShapeNode
    {
		[Input("Size", DefaultValues = new double[] { 1,1,1 })]
        protected IDiffSpread<Vector3D> FSize;

		public override void Evaluate(int SpreadMax)
		{
			if (this.FSize.IsChanged ||
				this.BasePinsChanged)
			{
				this.FShapes.SliceCount = SpreadMax;

				for (int i = 0; i < SpreadMax; i++)
				{
					Vector3D size = this.FSize[i].Abs();

					BoxShapeDefinition box = new BoxShapeDefinition((float)size.x, (float)size.y, (float)size.z);
					
					box.Mass = this.FMass[i];
					this.SetBaseParams(box, i);

					this.FShapes[i] = box;
				}
			}			
		}
	}
}
