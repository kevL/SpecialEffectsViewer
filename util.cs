using System;
using System.Drawing;

using Microsoft.DirectX;

using OEIShared.OEIMath;
using OEIShared.Utils;


namespace SpecialEffectsViewer
{
	static class util
	{
		internal static string L = Environment.NewLine;

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
			return "[" + color.A + "] " + color.R + "," + color.G + "," + color.B;
		}

		/// <summary>
		/// Splits a LightIntensityPair.
		/// </summary>
		/// <param name="pair"></param>
		/// <param name="pad"></param>
		/// <returns></returns>
		internal static string SplitLip(LightIntensityPair pair, string pad)
		{
			return      "Diffuse   " + GetColorString(pair.DiffuseColor)  + L
				+ pad + "Specular  " + GetColorString(pair.SpecularColor) + L
				+ pad + "Ambient   " + GetColorString(pair.AmbientColor)  + L
				+ pad + "Intensity " + pair.Intensity;
		}
	}
}
