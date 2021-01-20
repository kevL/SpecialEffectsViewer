using System;
using System.ComponentModel;


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
		[Category("Window")]
		[Description("The x-position of the plugin window on your desktop.")]
		[DefaultValue(Int32.MinValue)]
		public int x
		{ get; set; }

		[Category("Window")]
		[Description("The y-position of the plugin window on your desktop.")]
		[DefaultValue(Int32.MinValue)]
		public int y
		{ get; set; }

		[Category("Window")]
		[Description("The width of the plugin window on your desktop.")]
		[DefaultValue(Int32.MinValue)]
		public int w
		{ get; set; }

		[Category("Window")]
		[Description("The height of the plugin window on your desktop.")]
		[DefaultValue(Int32.MinValue)]
		public int h
		{ get; set; }

		[Category("Window")]
		[Description("If true the plugin will start with its window maximized.")]
		[DefaultValue(false)]
		public bool Maximized
		{ get; set; }

		[Category("Window")]
		[Description("The distance of the effects-list from the left border of the window.")]
		[DefaultValue(375)]
		public int SplitterDistanceEffects
		{ get; set; }

		[Category("Options")]
		[Description("The distance of event-data in the Options panel from the top of its split-container.")]
		[DefaultValue(83)]
		public int SplitterDistanceEvents
		{ get; set; }

		[Category("Options")]
		[Description("If true the plugin will stay on top of the toolset.")]
		[DefaultValue(true)]
		public bool StayOnTop
		{ get; set; }

		[Category("Options")]
		[Description("If true the plugin will start with its Options panel displayed.")]
		[DefaultValue(false)]
		public bool OptionsPanel
		{ get; set; }

		[Category("Options")]
		[Description("The scene that the plugin defaults to: 1 DoubleCharacter,"
				   + " 2 SingleCharacter, 3 PlacedEffectObject. The default is 1.")]
		[DefaultValue(1)]
		public int Scene
		{ get; set; }

		[Category("Options")]
		[Description("If true the scene will render with ground.")]
		[DefaultValue(true)]
		public bool Ground
		{ get; set; }

		[Category("Options")]
		[Description("If true the event-data in the Options panel will show extended info.")]
		[DefaultValue(false)]
		public bool ExtendedInfo
		{ get; set; }

		[Category("Camera")]
		[Description("The degree in radians that the camera will start on the"
				   + " x/y-plane clockwise from behind the orc (east).")]
//		[DefaultValue((float)Math.PI * 3f / 2f)]
		public float FocusTheta
		{ get; set; }

		[Category("Camera")]
		[Description("The degree of elevation in radians that the camera will start.")]
//		[DefaultValue(0f)]
		public float FocusPhi
		{ get; set; }

		[Category("Camera")]
		[Description("The distance from its focal point that the camera will start.")]
		[DefaultValue(10f)]
		public float Distance
		{ get; set; }

		[Category("Camera")]
		[Description("The camera's focal point on the x-axis. The default is 100; the"
				   + " area is 200x200.")]
		[DefaultValue(100f)]
		public float FocusPoint_x
		{ get; set; }

		[Category("Camera")]
		[Description("The camera's focal point on the y-axis. The default is 100; the"
				   + " area is 200x200.")]
		[DefaultValue(100f)]
		public float FocusPoint_y
		{ get; set; }

		[Category("Camera")]
		[Description("The camera's focal point on the z-axis. The default is 1.")]
		[DefaultValue(1f)]
		public float FocusPoint_z
		{ get; set; }

		[Category("SceneData dialog")]
		[Description("The delay in milliseconds between selecting an effect and"
				   + " showing information about the scene in the SceneData dialog."
				   + " If you get an InvalidOperationException when using the SceneData"
				   + " dialog \"Collection was modified; enumeration operation may not"
				   + " execute\" try increasing this delay. The default is 350. This is"
				   + " the only property of the SpecialEffectsViewer that is not"
				   + " tracked auto; its value can be increased or decreased here only"
				   + " (or directly in the Viewer's XML preferences file).")]
		[DefaultValue(350)]
		public int SceneDataDelay
		{ get; set; }

		[Category("SceneData dialog")]
		[Description("The x-position of the SceneData dialog on your desktop.")]
		[DefaultValue(Int32.MinValue)]
		public int SceneData_x
		{ get; set; }

		[Category("SceneData dialog")]
		[Description("The y-position of the SceneData dialog on your desktop.")]
		[DefaultValue(Int32.MinValue)]
		public int SceneData_y
		{ get; set; }

		[Category("SceneData dialog")]
		[Description("The width of the SceneData dialog on your desktop.")]
		[DefaultValue(Int32.MinValue)]
		public int SceneData_w
		{ get; set; }

		[Category("SceneData dialog")]
		[Description("The height of the SceneData dialog on your desktop.")]
		[DefaultValue(Int32.MinValue)]
		public int SceneData_h
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

			SplitterDistanceEffects = 375;
			SplitterDistanceEvents  =  83;

			StayOnTop    = true;
			OptionsPanel = false;

			Scene = 1;

			FocusTheta   = -(float)Math.PI / 2f;
			FocusPhi     = 0f;
			Distance     = 10f;
			FocusPoint_x = 100f;
			FocusPoint_y = 100f;
			FocusPoint_z = 1f;

			Ground       = true;
			ExtendedInfo = false;

			SceneDataDelay = 350;

			SceneData_x = SceneData_y =
			SceneData_w = SceneData_h = Int32.MinValue;
		}
		#endregion cTor


		#region Methods
		internal void ValidatePreferences()
		{
			if (x != Int32.MinValue && x < 0) x = 0;
			if (y != Int32.MinValue && y < 0) y = 0;
			if (w != Int32.MinValue && w < 0) w = 0;
			if (h != Int32.MinValue && h < 0) h = 0;

			if (SplitterDistanceEffects < 0) SplitterDistanceEffects = 0;
			if (SplitterDistanceEvents  < 0) SplitterDistanceEvents  = 0;

			if (Scene < 1 || Scene > 3) Scene = 1;

			while (FocusTheta < -(float)Math.PI * 2f) FocusTheta += (float)Math.PI * 2f;
			while (FocusTheta >  (float)Math.PI * 2f) FocusTheta -= (float)Math.PI * 2f;

			if      (FocusPhi <  0f)                 FocusPhi =  0f;
			else if (FocusPhi > (float)Math.PI / 2f) FocusPhi = (float)Math.PI / 2f;

			if      (Distance <   1f) Distance =   1f;
			else if (Distance > 100f) Distance = 100f;

			if (FocusPoint_x < 0f || FocusPoint_x > 200f) FocusPoint_x = 100f;
			if (FocusPoint_y < 0f || FocusPoint_y > 200f) FocusPoint_y = 100f;
			if (FocusPoint_z < 0f || FocusPoint_z >  10f) FocusPoint_z =   1f;

			if (SceneDataDelay < 15) SceneDataDelay = 15;

			if (SceneData_x != Int32.MinValue && SceneData_x < 0) SceneData_x = 0;
			if (SceneData_y != Int32.MinValue && SceneData_y < 0) SceneData_y = 0;
			if (SceneData_w != Int32.MinValue && SceneData_w < 0) SceneData_w = 0;
			if (SceneData_h != Int32.MinValue && SceneData_h < 0) SceneData_h = 0;
		}
		#endregion Methods
	}
}
