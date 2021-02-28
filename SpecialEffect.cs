using System;
using System.IO;

using OEIShared.Effects;
using OEIShared.IO;


namespace SpecialEffectsViewer
{
	/// <summary>
	/// A static object that contains SEF-related pointers.
	/// </summary>
	static class SpecialEffect
	{
		#region Properties (static)
		/// <summary>
		/// Pointer to the current resource entry.
		/// </summary>
		internal static IResourceEntry Resent
		{ get; private set; }

		/// <summary>
		/// Pointer to the current SEFGroup.
		/// </summary>
		internal static SEFGroup Sefgroup
		{ get; private set; }

		/// <summary>
		/// Pointer to the current alternate SEFGroup.
		/// </summary>
		internal static SEFGroup Altgroup
		{ get; private set; }

		/// <summary>
		/// Pointer to the current solo SEFGroup.
		/// </summary>
		internal static SEFGroup Solgroup
		{ get; private set; }
		#endregion Properties (static)


		#region Methods (static)
		/// <summary>
		/// Clears this SpecialEffect.
		/// </summary>
		internal static void ClearEffect()
		{
			Resent   = null;
			Sefgroup =
			Altgroup =
			Solgroup = null;
		}

		/// <summary>
		/// Clears the sub-SEFGroups of this SpecialEffect.
		/// </summary>
		internal static void ClearSubgroups()
		{
			Altgroup =
			Solgroup = null;
		}

		/// <summary>
		/// Sets the resource entry and creates a SEFGroup for it.
		/// </summary>
		/// <param name="resent"></param>
		internal static void CreateSefgroup(IResourceEntry resent)
		{
			Altgroup = null;
			Solgroup = null;

			Resent = resent;

			Sefgroup = new SEFGroup();

			using (Stream bin = Resent.GetStream(false))
				Sefgroup.XmlUnserialize(bin);

			Resent.Release();
		}

		/// <summary>
		/// Creates an alternate SEFGroup for the current resource entry.
		/// </summary>
		internal static void CreateAltgroup()
		{
			Solgroup = null;

			Altgroup = new SEFGroup();

			using (Stream bin = Resent.GetStream(false))
				Altgroup.XmlUnserialize(bin);

			Resent.Release();
		}

		/// <summary>
		/// Creates a solo SEFGroup for the current resource entry.
		/// </summary>
		internal static void CreateSolgroup()
		{
			Altgroup = null;

			Solgroup = new SEFGroup();

			using (Stream bin = Resent.GetStream(false))
				Solgroup.XmlUnserialize(bin);

			Resent.Release();
		}
		#endregion Methods (static)
	}
}
