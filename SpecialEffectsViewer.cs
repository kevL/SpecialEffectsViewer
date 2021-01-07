using System;

using NWN2Toolset.Plugins;

using TD.SandBar;


namespace SpecialEffectsViewer
{
	/// <summary>
	/// NwN2 Electron toolset plugin.
	/// </summary>
	public sealed class SpecialEffectsViewer
		: INWN2Plugin
	{
		#region INWN2Plugin (interface)
		public MenuButtonItem PluginMenuItem
		{ get; set; }

		public object Preferences
		{
			get
			{
				return SpecialEffectsViewerPreferences.that;
			}
			set
			{
				SpecialEffectsViewerPreferences.that = (SpecialEffectsViewerPreferences)value;
			}
		}

		/// <summary>
		/// Preferences will be stored in an XML-file w/ this label in
		/// C:\Users\User\AppData\Local\NWN2 Toolset\Plugins
		/// (or similar).
		/// </summary>
		public string Name
		{
			get { return "SpecialEffectsViewer"; }
		}

		/// <summary>
		/// The label of the operation on the toolset's "Plugins" menu.
		/// </summary>
		public string MenuName
		{
			get { return "Special Effects Viewer"; }
		}

		/// <summary>
		/// The caption on the titlebar of the plugin window.
		/// </summary>
		public string DisplayName
		{
			get { return "Special Effects Viewer"; }
		}


		public void Load(INWN2PluginHost host)
		{}

		public void Startup(INWN2PluginHost host)
		{
			PluginMenuItem = host.GetMenuForPlugin(this);
			PluginMenuItem.Activate += launch;
		}

		public void Shutdown(INWN2PluginHost host)
		{}

		public void Unload(INWN2PluginHost host)
		{}
		#endregion INWN2Plugin (interface)


		/// <summary>
		/// Handler that launches SpecialEffectsViewer from the toolset's
		/// Plugins menu.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void launch(object sender, EventArgs e)
		{
			logger.create();
			var f = new sevi();
		}
	}
}
