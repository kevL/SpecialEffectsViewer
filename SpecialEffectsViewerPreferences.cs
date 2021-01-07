using System;


namespace SpecialEffectsViewer
{
	/// <summary>
	/// Preferences per Electron toolset plugin interface.
	/// @note Prefs are stored at
	/// C:\Users\User\AppData\Local\NWN2 Toolset\Plugins\SpecialEffectsViewer.xml
	/// </summary>
	[Serializable]
	public class SpecialEffectsViewerPreferences
	{
		#region Properties (static)
		static SpecialEffectsViewerPreferences _that;
		public static SpecialEffectsViewerPreferences that
		{
			get
			{
				if (_that == null)
					_that = new SpecialEffectsViewerPreferences();

				return _that;
			}
			set { _that = value; }
		}
		#endregion Properties (static)


		#region Properties
		public int x
		{ get; set; }
		public int y
		{ get; set; }
		public int w
		{ get; set; }
		public int h
		{ get; set; }

		public bool Maximized
		{ get; set; }

		public int SplitterDistance
		{ get; set; }

		public bool StayOnTop
		{ get; set; }

		public int Scene
		{ get; set; }

		public float FocusPhi
		{ get; set; }

		public float FocusTheta
		{ get; set; }

		public float Distance
		{ get; set; }
		#endregion Properties


		#region cTor
		/// <summary>
		/// cTor. Sets default values for the SpecialEffectsViewer's preferences.
		/// @note sevi..cTor will quickly override these values with values
		/// in the user's prefs-file (if it exists).
		/// </summary>
		SpecialEffectsViewerPreferences()
		{
			x = y =
			w = h = Int32.MinValue;

			Maximized = false;

			SplitterDistance = 400;

			StayOnTop = true;

			Scene = 1;

			FocusPhi   = 0f;
			FocusTheta = -(float)Math.PI / 2f;
			Distance   = 10f;
		}
		#endregion cTor
	}
}
