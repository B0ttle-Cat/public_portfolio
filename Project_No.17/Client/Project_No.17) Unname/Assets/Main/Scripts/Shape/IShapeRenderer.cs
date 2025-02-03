using System;

using BC.ODCC;

using Shapes;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.Rendering;

using ScaleMode = Shapes.ScaleMode;

namespace BC.Shape
{
	public interface IShapeRenderer : IOdccComponent
	{
		public void OnDrawShapes(Camera cam);
		public void IOnDrawShapes(Camera cam)
		{
			if((cam.cullingMask & (1 << GameObject.layer)) == 0)
				return; // scene & game view cameras should respect culling layer settings if you tell them to

			Draw.Push();
			OnDrawShapes(cam);
			Draw.Pop();
		}

		[Serializable]
		public struct DrawStyle
		{
			[Title("DrawStyle")]
			[ColorUsage(true, true)]
			public Color color;
			public ShapesBlendMode blendMode; // technically a render state rather than property, but we swap shaders here instead
			public ScaleMode scaleMode;
			public DetailLevel detailLevel;

			[Title("RenderState")]
			[Header("ColorMask"),EnumToggleButtons]
			public ColorWriteMask colorMask;

			[Header("ZTest")]
			public CompareFunction zTest;
			public float zOffsetFactor;
			public int zOffsetUnits;

			[Header("Stencil")]
			public CompareFunction stencilComp;
			public StencilOp stencilOpPass;
			public byte stencilRefID;
			public ByteMask stencilReadMask;
			public ByteMask stencilWriteMask;

			[Flags]
			public enum ByteMask : byte
			{
				None = 0,

				A = 1<<0,
				B = 1<<1,
				C = 1<<2,
				D = 1<<3,

				E = 1<<4,
				F = 1<<5,
				G = 1<<6,
				H = 1<<7,

				All = 255,
			}
			public DrawStyle(Color color)
			{
				this.color = color;
				blendMode = ShapesBlendMode.Transparent;
				scaleMode = Shapes.ScaleMode.Uniform;
				detailLevel = DetailLevel.Extreme;

				zTest = CompareFunction.LessEqual;
				zOffsetFactor = 0;
				zOffsetUnits = 0;
				colorMask = ColorWriteMask.All;
				stencilComp = CompareFunction.Always;
				stencilOpPass = StencilOp.Keep;
				stencilRefID = 0;
				stencilReadMask = ByteMask.All;
				stencilWriteMask = ByteMask.All;
			}

			[Button("Reset Value")]
			public void ResetValue()
			{
				blendMode = ShapesBlendMode.Transparent;
				scaleMode = ScaleMode.Uniform;
				detailLevel = DetailLevel.Extreme;

				zTest = CompareFunction.LessEqual;
				zOffsetFactor = 0;
				zOffsetUnits = 0;
				colorMask = ColorWriteMask.All;
				stencilComp = CompareFunction.Always;
				stencilOpPass = StencilOp.Keep;
				stencilRefID = 0;
				stencilReadMask = ByteMask.All;
				stencilWriteMask = ByteMask.All;
			}
			public void SetDrawStyle()
			{
				Draw.ResetStyle();
				Draw.Color = color;
				Draw.BlendMode = blendMode;
				Draw.ScaleMode = scaleMode;
				Draw.DetailLevel = detailLevel;

				Draw.ZTest = zTest;
				Draw.ZOffsetFactor = zOffsetFactor;
				Draw.ZOffsetUnits = zOffsetUnits;
				Draw.ColorMask = colorMask;
				Draw.StencilComp  = stencilComp;
				Draw.StencilOpPass = stencilOpPass;
				Draw.StencilRefID = stencilRefID;
				Draw.StencilReadMask = (byte)stencilReadMask;
				Draw.StencilWriteMask = (byte)stencilWriteMask;
			}
		}
	}
}
