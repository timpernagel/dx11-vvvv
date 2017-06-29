﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="DestroyConstraint",Category="Bullet",Author="vux",AutoEvaluate=true)]
	public class DestroyConstraintNode : IPluginEvaluate
	{
		[Input("Constraints", Order = 0)]
        protected Pin<TypedConstraint> FInput;

		[Input("Apply", IsBang = true, Order = 1000)]
        protected ISpread<bool> FApply;

		public void Evaluate(int SpreadMax)
		{
			if (FInput.PluginIO.IsConnected)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					if (FApply[i])
					{
						TypedConstraint cst = this.FInput[i];
						ConstraintCustomData cust = (ConstraintCustomData)cst.UserObject;
						cust.MarkedForDeletion = true;
					}
				}
			}			
		}
	}
}
