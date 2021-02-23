using System;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;

using OEIShared.OEIMath;
using OEIShared.Utils;


namespace SpecialEffectsViewer
{
	/// <summary>
	/// Static class of general utilities.
	/// </summary>
	static class util
	{
		#region Fields (static)
		internal const float pi2  = (float)Math.PI * 2f;
		internal const float pi_2 = (float)Math.PI / 2f;

		internal static string L = Environment.NewLine;
		#endregion Fields (static)


		#region Methods (static)
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

		/// <summary>
		/// Checks if an x/y location is onscreen.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		internal static bool checklocation(int x, int y)
		{
			x += 100; y += 50;

			Screen[] screens = Screen.AllScreens;
			foreach (var screen in screens)
			{
				if (screen.WorkingArea.Contains(x,y))
					return true;
			}
			return false;
		}
		#endregion Methods (static)
	}


	/// <summary>
	/// Decrypts obfuscated strings in the toolset DLLs.
	/// </summary>
	static class StringDecryptor
	{
		internal static string Decrypt(string st)
		{
			char[] array0 = st.ToCharArray();
			char[] array1 = array0;

			int len0;
			int len1 = array1.Length;
			while (len1 != 0)
			{
				len0 = len1 - 1;
				array1[len0] = (char)(array0[len0] - 5225);

				array1 = array0;
				len1 = len0;
			}

			return String.Intern(new string(array1));
		}
	}
}
