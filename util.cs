using System;
using System.Drawing;

using Microsoft.DirectX;

using OEIShared.OEIMath;


namespace SpecialEffectsViewer
{
	static class util
	{
		/// <summary>
		/// Gets a string for a 2d-vector.
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		internal static string Get2dString(Vector2 vec)
		{
			return vec.X + "," + vec.Y;
		}

		/// <summary>
		/// Gets a string for a 3d-vector.
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		internal static string Get3dString(Vector3 vec)
		{
			return vec.X + "," + vec.Y + "," + vec.Z;
		}

		/// <summary>
		/// Gets a string for a quaternion.
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		internal static string Get4dString(RHQuaternion vec)
		{
			return vec.X + "," + vec.Y + "," + vec.Z + "," + vec.W;
		}

		internal static string GetColorString(Color color)
		{
			return "a" + color.A + " r" + color.R + " g" + color.G + " b" + color.B;
		}
	}
}
