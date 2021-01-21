using System;
using System.Drawing;

using Microsoft.DirectX;

using OEIShared.OEIMath;


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
			return "a" + color.A + " r" + color.R + " g" + color.G + " b" + color.B;
		}

		/// <summary>
		/// Splits a LightIntensityPair.
		/// </summary>
		/// <param name="pair"></param>
		/// <param name="pad"></param>
		/// <returns></returns>
		internal static string SplitLip(object pair, string pad)
		{
			string diff = pair.ToString();

			int pos = diff.IndexOf("Intensity", StringComparison.Ordinal);
			string inte = diff.Substring(pos);

			diff = diff.Substring(0, diff.Length - inte.Length - 2);
			pos = diff.IndexOf("Ambient", StringComparison.Ordinal);
			string ambi = diff.Substring(pos);

			diff = diff.Substring(0, diff.Length - ambi.Length - 2);
			pos = diff.IndexOf("Specular", StringComparison.Ordinal);
			string spec = diff.Substring(pos);

			diff = diff.Substring(0, diff.Length - spec.Length - 2);

			return diff       + L
				 + pad + spec + L
				 + pad + ambi + L
				 + pad + inte;
		}
	}
}
