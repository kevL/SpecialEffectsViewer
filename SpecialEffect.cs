using System;
using System.IO;

using OEIShared.Effects;
using OEIShared.IO;


namespace specialeffectsviewer
{
	/// <summary>
	/// A static object that contains SEF-related pointers.
	/// </summary>
	/// <remarks><see cref="sevi"/> has only 1 effect selected at any time. This
	/// is that effect. Although used only by DoubleCharacter config
	/// <see cref="Sefgroup"/> needs to be kept updated no matter the current
	/// scene in case user then selects the DoubleCharacter scene.</remarks>
	static class SpecialEffect
	{
		#region Properties (static)
		/// <summary>
		/// Pointer to the current resource entry.
		/// </summary>
		internal static IResourceEntry Resent
		{ get; set; }

		/// <summary>
		/// Pointer to the current SEFGroup.
		/// </summary>
		/// <remarks>This is used to display the full effect.</remarks>
		internal static SEFGroup Sefgroup
		{ get; private set; }

		/// <summary>
		/// Pointer to the current alternate SEFGroup.
		/// </summary>
		/// <remarks>This is used to display selected events of the effect per
		/// the Events menu.</remarks>
		internal static SEFGroup Altgroup
		{ get; set; }

		/// <summary>
		/// Pointer to the current solo SEFGroup.
		/// </summary>
		/// <remarks>This is used to display a solo effect if [Shift] is
		/// depressed when clicking an event on the Events menu.</remarks>
		internal static SEFGroup Solgroup
		{ get; set; }
		#endregion Properties (static)


		#region Methods (static)
		/// <summary>
		/// Assigns an IResourceEntry and creates a SEFGroup for it.
		/// </summary>
		/// <param name="resent"></param>
		internal static void CreateSefgroup(IResourceEntry resent)
		{
			Resent = resent;

			Altgroup = null;
			Solgroup = null;

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
		#endregion Methods (static)
	}
}
