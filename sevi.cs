using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Microsoft.DirectX;

using NWN2Toolset;
using NWN2Toolset.NWN2.Data.Blueprints;
using NWN2Toolset.NWN2.Data.Campaign;
using NWN2Toolset.NWN2.Data.Instances;
using NWN2Toolset.NWN2.Data.Templates;
using NWN2Toolset.NWN2.IO;
using NWN2Toolset.NWN2.NetDisplay;

using OEIShared.Effects;
using OEIShared.IO;
using OEIShared.NetDisplay;
using OEIShared.OEIMath;
using OEIShared.UI;
using OEIShared.UI.Input;
using OEIShared.Utils;


namespace SpecialEffectsViewer
{
	/// <summary>
	/// The SpecialEffectsViewer window along with all of its mechanics. Aka the
	/// UI.
	/// </summary>
	sealed partial class sevi
		: Form
	{
		#region enums
		/// <summary>
		/// pick a Scene, any scene
		/// </summary>
		enum Scene
		{
			none,				// 0 - not used.
			doublecharacter,	// 1
			singlecharacter,	// 2
			placedeffect		// 3
		}
		#endregion enums


		#region Fields (static)
		const string TITLE = "Special Effects Viewer";

		const string SEF = "sef";

		const string MODULES   = "modules";
		const string CAMPAIGNS = "campaigns";
		const string OVERRIDE  = "override";

		const int STALE_non      = 0x0;
		const int STALE_Module   = 0x1;
		const int STALE_Campaign = 0x2;

		const int ItemsReserved = 6; // count of standard its in the Events dropdown

		/// <summary>
		/// A default width for the Options popout.
		/// </summary>
		static int WidthOptions;
		#endregion Fields (static)


		#region Fields
		IResourceEntry _effect; SEFGroup _sefgroup, _altgroup;
		// TODO: Probably don't need both '_sefgroup' and '_altgroup'.

		ElectronPanel _panel = new ElectronPanel(); // i hate u

		MenuItem _itFxList_all;
		MenuItem _itFxList_stock;
		MenuItem _itFxList_module;
		MenuItem _itFxList_campaign;
		MenuItem _itFxList_override;

		MenuItem _itEvents;
		MenuItem _itEvents_Play;
		MenuItem _itEvents_Stop;
		MenuItem _itEvents_Enable;
		MenuItem _itEvents_Disable;

		MenuItem _itView_DoubleCharacter;
		MenuItem _itView_SingleCharacter;
		MenuItem _itView_PlacedEffect;
		MenuItem _itView_Ground;
		MenuItem _itView_Options;
		MenuItem _itView_ExtendedInfo;
		MenuItem _itView_StayOnTop;

		/// <summary>
		/// A bitwise int that can repopulate the effects-list when the toolset
		/// changes its currently loaded Module or Campaign.
		/// </summary>
		int _isListStale;

		string _filtr = String.Empty;

		/// <summary>
		/// The search-textbox is usually focused after the effects-list is
		/// populated; but don't do that if the filter-button is clicked. That
		/// is keep the filter-button focused so the list can be toggled/
		/// re-toggled.
		/// </summary>
		bool _bypassSearchFocus;

		/// <summary>
		/// Fullpath to the helpfile if it exists.
		/// </summary>
		string _pfe_helpfile;

		/// <summary>
		/// True when the RMB or MMB is down in the ElectronPanel. Is used to
		/// deter the current cursor.
		/// </summary>
		bool _Right;

		/// <summary>
		/// True when the Control-key is depressed in the ElectronPanel. Is used
		/// to deter the current cursor.
		/// </summary>
		bool _Ctrl;
		#endregion Fields


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		internal sevi()
		{
			Owner = NWN2ToolsetMainForm.App;

			InitializeComponent();

			WidthOptions = sc2_Options.SplitterDistance;

			// leave Options-panel open in the designer but close it here
			sc2_Options.Panel1Collapsed = true;

			// set unicode text on the up/down Search btns.
			bu_SearchD.Text = "\u25bc"; // down triangle
			bu_SearchU.Text = "\u25b2"; // up triangle

			CreateMainMenu();
			ConfigureElectronPanel();

			LoadPreferences();

			NWN2ToolsetMainForm.ModuleChanged                  += OnModuleChanged;
			NWN2CampaignManager.Instance.ActiveCampaignChanged += OnActiveCampaignChanged;

			mi_resrep_All(null, EventArgs.Empty);

			Show();
		}

		/// <summary>
		/// Instantiates the plugin's MainMenu.
		/// </summary>
		void CreateMainMenu()
		{
			Menu = new MainMenu();

			MenuItem it, it0;

// List ->
			it = Menu.MenuItems.Add("&Resrep"); // 0

			_itFxList_all      = it.MenuItems.Add("list &all effects", mi_resrep_All);
								 it.MenuItems.Add("-");
			_itFxList_stock    = it.MenuItems.Add("&stock only",       mi_resrep_Stock);
			_itFxList_module   = it.MenuItems.Add("&module only",      mi_resrep_Module);
			_itFxList_campaign = it.MenuItems.Add("&campaign only",    mi_resrep_Campaign);
			_itFxList_override = it.MenuItems.Add("&override only",    mi_resrep_Override);

			_itFxList_all     .Shortcut = Shortcut.Ctrl1;
			_itFxList_stock   .Shortcut = Shortcut.Ctrl2;
			_itFxList_module  .Shortcut = Shortcut.Ctrl3;
			_itFxList_campaign.Shortcut = Shortcut.Ctrl4;
			_itFxList_override.Shortcut = Shortcut.Ctrl5;

// Events ->
			_itEvents = Menu.MenuItems.Add("&Events"); // 1
			_itEvents.Popup += mi_events_popup;

			CreateBasicEvents();

// View ->
			 it = Menu.MenuItems.Add("&View"); // 2

			_itView_DoubleCharacter = it.MenuItems.Add("&Double character",     mi_view_DoubleCharacter);
			_itView_SingleCharacter = it.MenuItems.Add("&Single character",     mi_view_SingleCharacter);
			_itView_PlacedEffect    = it.MenuItems.Add("&placed effect object", mi_view_PlacedEffect);
									  it.MenuItems.Add("-");
			_itView_Ground          = it.MenuItems.Add("show &Ground",          mi_view_Ground);
									  it.MenuItems.Add("-");
			_itView_Options         = it.MenuItems.Add("show &Options panel",   mi_view_Options);
			_itView_ExtendedInfo    = it.MenuItems.Add("show extended &info",   mi_view_Extended);
									  it.MenuItems.Add("-");
			_itView_StayOnTop       = it.MenuItems.Add("stay on &top",          mi_view_StayOnTop);

			_itView_DoubleCharacter.Shortcut = Shortcut.F10;
			_itView_SingleCharacter.Shortcut = Shortcut.F11;
			_itView_PlacedEffect   .Shortcut = Shortcut.F12;
			_itView_Ground         .Shortcut = Shortcut.CtrlG;
			_itView_Options        .Shortcut = Shortcut.F9;
			_itView_ExtendedInfo   .Shortcut = Shortcut.CtrlI;
			_itView_StayOnTop      .Shortcut = Shortcut.CtrlT;

			_itView_DoubleCharacter.Checked =
			_itView_StayOnTop      .Checked = true;

// Help ->
			it = Menu.MenuItems.Add("&Help"); // 3

			_pfe_helpfile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			_pfe_helpfile = Path.Combine(_pfe_helpfile, "SpecialEffectsViewer.txt");
			if (File.Exists(_pfe_helpfile))
			{
				it0 = it.MenuItems.Add("&help", mi_help_Help);
				it0.Shortcut = Shortcut.F1;
			}

			it0 = it.MenuItems.Add("&about", mi_help_About);
			it0.Shortcut = Shortcut.F2;
		}

		/// <summary>
		/// Layout control for the ElectronPanel.
		/// </summary>
		void ConfigureElectronPanel()
		{
			_panel.Dock = DockStyle.Fill;
			_panel.BorderStyle = BorderStyle.FixedSingle;
			_panel.MousePanel.KeyDown   += lb_Effects_keydown;
			_panel.MousePanel.MouseDown += panel_mousedown;
			_panel.MousePanel.MouseUp   += panel_mouseup;
			_panel.MousePanel.KeyDown   += panel_keydown;
			_panel.MousePanel.KeyUp     += panel_keyup;

			sc2_Options.Panel2.Controls.Add(_panel);
		}

		/// <summary>
		/// Loads preferences.
		/// </summary>
		void LoadPreferences()
		{
			int x = SpecialEffectsViewerPreferences.that.x;
			if (x > -1)
			{
				int y = SpecialEffectsViewerPreferences.that.y;
				if (y > -1 && checklocation(x,y))
				{
					StartPosition = FormStartPosition.Manual;
					SetDesktopLocation(x,y);
				}
				ClientSize = new Size(SpecialEffectsViewerPreferences.that.w,
									  SpecialEffectsViewerPreferences.that.h);
			}

			if (SpecialEffectsViewerPreferences.that.OptionsPanel)
				_itView_Options.PerformClick();

			sc1_Effects.SplitterDistance = SpecialEffectsViewerPreferences.that.SplitterDistanceEffects;
			sc3_Events .SplitterDistance = SpecialEffectsViewerPreferences.that.SplitterDistanceEvents;

			if (!SpecialEffectsViewerPreferences.that.StayOnTop)
				_itView_StayOnTop.PerformClick();

			if (SpecialEffectsViewerPreferences.that.Maximized)
				WindowState = FormWindowState.Maximized;

			if (SpecialEffectsViewerPreferences.that.Scene != (int)Scene.doublecharacter)
			{
				_itView_DoubleCharacter.Checked = (rb_DoubleCharacter.Checked = false);

				switch ((Scene)SpecialEffectsViewerPreferences.that.Scene)
				{
					default: // safety.
						_itView_DoubleCharacter.Checked = (rb_DoubleCharacter.Checked = true);
						break;

					case Scene.singlecharacter:
						_itView_SingleCharacter.Checked = (rb_SingleCharacter.Checked = true);
						break;

					case Scene.placedeffect:
						_itView_PlacedEffect.Checked = (rb_PlacedEffect.Checked = true);
						break;
				}
			}

			_itView_Ground      .Checked = (cb_Ground.Checked = SpecialEffectsViewerPreferences.that.Ground);
			_itView_ExtendedInfo.Checked = SpecialEffectsViewerPreferences.that.ExtendedInfo;
		}
		#endregion cTor


		#region eventhandlers (override)
		/// <summary>
		/// Prepares the ElectronPanel.
		/// @note The camera-receiver goes borky if this is done in the cTor.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			if (_panel.Scene.DayNightCycleStages[(int)DayNightStageType.Default] != null)
			{
				_panel.Scene.DayNightCycleStages[(int)DayNightStageType.Default].SunMoonDirection = new Vector3(0.00f,-0.67f,-0.67f);
				_panel.Scene.DayNightCycleStages[(int)DayNightStageType.Default].ShadowIntensity  = 0f;
			}

			if (cb_Ground.Checked)
				NWN2NetDisplayManager.Instance.UpdateTerrainSize(_panel.Scene, new Size(4,4)); // note: 4x4 is min area (200x200)

			var receiver = new ModelViewerInputCameraReceiver(); // is null on Load
			_panel.CameraMovementReceiver = receiver;

			var state = (receiver.CameraState as ModelViewerInputCameraReceiverState);
			state.FocusPoint      = new Vector3(SpecialEffectsViewerPreferences.that.FocusPoint_x, // +x=left -x=right +y=closer -y=farther
												SpecialEffectsViewerPreferences.that.FocusPoint_y,
												SpecialEffectsViewerPreferences.that.FocusPoint_z);
			state.FocusPhi        = SpecialEffectsViewerPreferences.that.FocusPhi;
			state.FocusTheta      = SpecialEffectsViewerPreferences.that.FocusTheta;
			state.Distance        = SpecialEffectsViewerPreferences.that.Distance;
			state.PitchMin        = 0.0f;
			state.MouseWheelSpeed = 0.8f;

			receiver.UpdateCamera();
		}

		/// <summary>
		/// Stores preferences and unsubscribes from toolset-events.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			NWN2ToolsetMainForm.ModuleChanged                  -= OnModuleChanged;
			NWN2CampaignManager.Instance.ActiveCampaignChanged -= OnActiveCampaignChanged;


			if (WindowState == FormWindowState.Maximized)
				SpecialEffectsViewerPreferences.that.Maximized = true;
			else
				SpecialEffectsViewerPreferences.that.Maximized = false;

			WindowState = FormWindowState.Normal;

			SpecialEffectsViewerPreferences.that.x = DesktopLocation.X;
			SpecialEffectsViewerPreferences.that.y = DesktopLocation.Y;

			SpecialEffectsViewerPreferences.that.w = ClientSize.Width;
			SpecialEffectsViewerPreferences.that.h = ClientSize.Height;

			SpecialEffectsViewerPreferences.that.SplitterDistanceEffects = sc1_Effects.SplitterDistance;

			if (!sc2_Options.Panel1Collapsed)
				SpecialEffectsViewerPreferences.that.SplitterDistanceEvents = sc3_Events.SplitterDistance;

			StoreCameraState();
		}

		/// <summary>
		/// Repopulates the Fx-list when activated iff the Module or Campaign
		/// was changed in the toolset.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnActivated(EventArgs e)
		{
			if (_isListStale != STALE_non)
			{
				if (_itFxList_all.Checked)
				{
					_itFxList_all.Checked = false;
					mi_resrep_All(null, EventArgs.Empty);
				}
				else if (_itFxList_module.Checked)
				{
					if ((_isListStale & STALE_Module) != 0)
					{
						_itFxList_module.Checked = false;
						mi_resrep_Module(null, EventArgs.Empty);
					}
				}
				else if (_itFxList_campaign.Checked)
				{
					if ((_isListStale & STALE_Campaign) != 0)
					{
						_itFxList_campaign.Checked = false;
						mi_resrep_Campaign(null, EventArgs.Empty);
					}
				}
				_isListStale = STALE_non;
			}
		}

		/// <summary>
		/// [Esc] closes the plugin.
		/// @note Requires 'KeyPreview' true.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyData == Keys.Escape)
			{
				e.Handled = e.SuppressKeyPress = true;
				Close();
			}
			base.OnKeyDown(e);
		}
		#endregion eventhandlers (override)


		#region eventhandlers (toolset)
		/// <summary>
		/// Forces the Fx-list to repopulate when the module changes.
		/// </summary>
		/// <param name="oSender"></param>
		/// <param name="eArgs"></param>
		void OnModuleChanged(object oSender, ModuleChangedEventArgs eArgs)
		{
			_isListStale |= STALE_Module;
		}

		/// <summary>
		/// Forces the Fx-list to repopulate when the active campaign changes.
		/// </summary>
		/// <param name="cOldCampaign"></param>
		/// <param name="cNewCampaign"></param>
		void OnActiveCampaignChanged(NWN2Campaign cOldCampaign, NWN2Campaign cNewCampaign)
		{
			_isListStale |= STALE_Campaign;
		}
		#endregion eventhandlers (toolset)


		#region eventhandlers (resrep)
		/// <summary>
		/// Populates the effects-list with everything it can find.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_resrep_All(object sender, EventArgs e)
		{
			if (!_itFxList_all.Checked)
			{
				lb_Effects.BeginUpdate();

				ClearEffectsList();
				_itFxList_all.Checked = true;

				var entries = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF));
				foreach (IResourceEntry entry in entries)
				{
					if (_filtr == String.Empty || entry.ResRef.Value.ToLower().Contains(_filtr))
						lb_Effects.Items.Add(entry);
				}
				lb_Effects.EndUpdate();
			}

			if (!_bypassSearchFocus)
				ActiveControl = tb_Search;
		}

		/// <summary>
		/// Populates the effects-list from the stock data folder.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_resrep_Stock(object sender, EventArgs e)
		{
			if (!_itFxList_stock.Checked)
			{
				lb_Effects.BeginUpdate();

				ClearEffectsList();
				_itFxList_stock.Checked = true;

				var entries = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF));
				foreach (IResourceEntry entry in entries)
				{
					string label = entry.Repository.Name.ToLower();
					if (   !label.Contains(MODULES) // fake it ->
						&& !label.Contains(CAMPAIGNS)
						&& !label.Contains(OVERRIDE)
						&& (_filtr == String.Empty || entry.ResRef.Value.ToLower().Contains(_filtr)))
					{
						lb_Effects.Items.Add(entry);
					}
				}
				lb_Effects.EndUpdate();
			}

			if (!_bypassSearchFocus)
				ActiveControl = tb_Search;
		}

		/// <summary>
		/// Populates the effects-list from the current module folder.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_resrep_Module(object sender, EventArgs e)
		{
			if (!_itFxList_module.Checked)
			{
				lb_Effects.BeginUpdate();

				ClearEffectsList();
				_itFxList_module.Checked = true;

				var entries = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF));
				foreach (IResourceEntry entry in entries)
				{
					if (entry.Repository.Name.ToLower().Contains(MODULES) // fake it
						&& (_filtr == String.Empty || entry.ResRef.Value.ToLower().Contains(_filtr)))
					{
						lb_Effects.Items.Add(entry);
					}
				}
				lb_Effects.EndUpdate();
			}

			if (!_bypassSearchFocus)
				ActiveControl = tb_Search;
		}

		/// <summary>
		/// Populates the effects-list from the current campaign folder.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_resrep_Campaign(object sender, EventArgs e)
		{
			if (!_itFxList_campaign.Checked)
			{
				lb_Effects.BeginUpdate();

				ClearEffectsList();
				_itFxList_campaign.Checked = true;

				var entries = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF));
				foreach (IResourceEntry entry in entries)
				{
					if (entry.Repository.Name.ToLower().Contains(CAMPAIGNS) // fake it
						&& (_filtr == String.Empty || entry.ResRef.Value.ToLower().Contains(_filtr)))
					{
						lb_Effects.Items.Add(entry);
					}
				}
				lb_Effects.EndUpdate();
			}

			if (!_bypassSearchFocus)
				ActiveControl = tb_Search;
		}

		/// <summary>
		/// Populates the effects-list from the override folder.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_resrep_Override(object sender, EventArgs e)
		{
			if (!_itFxList_override.Checked)
			{
				lb_Effects.BeginUpdate();

				ClearEffectsList();
				_itFxList_override.Checked = true;

				var entries = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF));
				foreach (IResourceEntry entry in entries)
				{
					if (entry.Repository.Name.ToLower().Contains(OVERRIDE) // fake it
						&& (_filtr == String.Empty || entry.ResRef.Value.ToLower().Contains(_filtr)))
					{
						lb_Effects.Items.Add(entry);
					}
				}
				lb_Effects.EndUpdate();
			}

			if (!_bypassSearchFocus)
				ActiveControl = tb_Search;
		}
		#endregion eventhandlers (resrep)


		#region eventhandlers (events)
		/// <summary>
		/// Disables Play if there is not a selected effect.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_events_popup(object sender, EventArgs e)
		{
			_itEvents_Play.Enabled =
			_itEvents_Stop.Enabled = (lb_Effects.SelectedIndex != -1);
		}

		/// <summary>
		/// Plays the currently selected effect.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_events_Play(object sender, EventArgs e)
		{
			_panel.Scene.SpecialEffectsManager.EndUpdating();

			if (lb_Effects.SelectedIndex != -1)
			{
				if (rb_DoubleCharacter.Checked)
				{
					if (CanPlayEvents())
					{
						// clear the netdisplay
						// - recreate the scene in case a solo-event was played previously.
						// - the toolset code is not as cooperative as I'd like ...
						ClearScene();

						_altgroup = null;
						mi_events_Event(null, EventArgs.Empty);
					}
				}
				else
					_panel.Scene.SpecialEffectsManager.BeginUpdating();
			}
		}

		/// <summary>
		/// Stops the currently selected effect.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_events_Stop(object sender, EventArgs e)
		{
			_panel.Scene.SpecialEffectsManager.EndUpdating();
		}

		/// <summary>
		/// Alters the current effect in accord with the Events menu and then
		/// plays it.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_events_Event(object sender, EventArgs e)
		{
			if (lb_Effects.SelectedIndex != -1)
			{
				var it = sender as MenuItem;

				bool isDisable = false;
				bool isSolo    = false;

				if (sender != null) // if NOT cb_Ground_click() ie. is a real Events click ->
				{
					// set the items' check
					if (it == _itEvents_Enable) // enable all events
					{
						for (int i = ItemsReserved; i != _itEvents.MenuItems.Count; ++i)
							_itEvents.MenuItems[i].Checked = true;
					}
					else if (it == _itEvents_Disable) // disable all events
					{
						isDisable = true;
						for (int i = ItemsReserved; i != _itEvents.MenuItems.Count; ++i)
							_itEvents.MenuItems[i].Checked = false;
					}
					else if (!(isSolo = (ModifierKeys == Keys.Shift)))
						it.Checked = !it.Checked;

					// clear the netdisplay
					// - don't bother with this for a Ground-toggle since the ElectronPanel will be recreated for that.
					ClearScene();

					_altgroup = null;
				}

				if (_altgroup == null) // create or recreate '_altgroup' ->
				{
					// alter the sefgroup
					var effect_alt = CommonUtils.SerializationClone(_effect) as IResourceEntry;

					_altgroup = new SEFGroup();
					_altgroup.XmlUnserialize(effect_alt.GetStream(false));

					if (!isSolo)
					{
						for (int i = _altgroup.Events.Count - 1; i != -1; --i)
						{
							if (!_itEvents.MenuItems[i + ItemsReserved].Checked)
								_altgroup.Events.RemoveAt(i);
						}
					}
					else
					{
						for (int i = _altgroup.Events.Count - 1; i != -1; --i)
						{
							if (i != (int)it.Tag)
								_altgroup.Events.RemoveAt(i);
						}
					}
				}

				// play it
				LoadSefgroup(_altgroup);

				if (!isDisable)
					_panel.Scene.SpecialEffectsManager.BeginUpdating();
			}
		}
		#endregion eventhandlers (events)


		#region eventhandlers (view)
		/// <summary>
		/// Handles a click on View|Double character.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_view_DoubleCharacter(object sender, EventArgs e)
		{
			if (!_itView_DoubleCharacter.Checked)
			{
				rb_DoubleCharacter.Checked = true;
				rb_click(null, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Handles a click on View|Single character.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_view_SingleCharacter(object sender, EventArgs e)
		{
			if (!_itView_SingleCharacter.Checked)
			{
				rb_SingleCharacter.Checked = true;
				rb_click(null, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Handles a click on View|Placed effect.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_view_PlacedEffect(object sender, EventArgs e)
		{
			if (!_itView_PlacedEffect.Checked)
			{
				rb_PlacedEffect.Checked = true;
				rb_click(null, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Handles a click on View|show Ground.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_view_Ground(object sender, EventArgs e)
		{
			cb_Ground.Checked = !cb_Ground.Checked;
			cb_Ground_click(null, EventArgs.Empty);
		}

		/// <summary>
		/// Toggles display of the Options/Events sidepanel.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_view_Options(object sender, EventArgs e)
		{
			if (!sc2_Options.Panel1Collapsed)
				SpecialEffectsViewerPreferences.that.SplitterDistanceEvents = sc3_Events.SplitterDistance; // workaround.

			if (!(sc2_Options.Panel1Collapsed = !(SpecialEffectsViewerPreferences.that.OptionsPanel =
												 (_itView_Options.Checked = !_itView_Options.Checked))))
			{
				sc3_Events.SplitterDistance = SpecialEffectsViewerPreferences.that.SplitterDistanceEvents; // workaround.
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_view_Extended(object sender, EventArgs e)
		{
			SpecialEffectsViewerPreferences.that.ExtendedInfo =
			(_itView_ExtendedInfo.Checked = !_itView_ExtendedInfo.Checked);

			CreateBasicEvents();
			PrintEffectData();
		}

		/// <summary>
		/// Toggles toolset ownership of the plugin-window.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_view_StayOnTop(object sender, EventArgs e)
		{
			if (SpecialEffectsViewerPreferences.that.StayOnTop =
			   (_itView_StayOnTop.Checked = !_itView_StayOnTop.Checked))
			{
				Owner = NWN2ToolsetMainForm.App;
			}
			else
				Owner = null;
		}
		#endregion eventhandlers (view)


		#region eventhandlers (help)
		/// <summary>
		/// Opens 'SpecialEffectsViewer.txt' via Windows file-association.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_help_Help(object sender, EventArgs e)
		{
			if (File.Exists(_pfe_helpfile)) Process.Start(_pfe_helpfile);
		}

		/// <summary>
		/// Displays the About dialog.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_help_About(object sender, EventArgs e)
		{
			using (var f = new AboutF()) f.ShowDialog(this);
		}
		#endregion eventhandlers (help)


		#region eventhandlers (controls)
		/// <summary>
		/// This is NOT "selectedindexchanged" - it fires if the current
		/// selection is (re)clicked also.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void lb_Effects_selectedindexchanged(object sender, EventArgs e)
		{
			if (lb_Effects.SelectedIndex != -1)
			{
				_effect = lb_Effects.SelectedItem as IResourceEntry;
				_sefgroup = new SEFGroup();
				_sefgroup.XmlUnserialize(_effect.GetStream(false));

				ApplyEffect();

				Text = TITLE + " - " + _effect.Repository.Name;
			}
			else
			{
				Text = TITLE;
				_effect = null; _sefgroup = null; _altgroup = null;
				tb_SefData.Text = tb_EventData.Text = String.Empty;
			}
		}

		/// <summary>
		/// Replays the currently selected effect.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void lb_Effects_keydown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
			{
				e.Handled = e.SuppressKeyPress = true;
				mi_events_Play(null, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Sets the Scene-preference and reloads the scene itself.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void rb_click(object sender, EventArgs e)
		{
			if (rb_DoubleCharacter.Checked)
			{
				SpecialEffectsViewerPreferences.that.Scene = (int)Scene.doublecharacter;
				SetSceneType(Scene.doublecharacter);
			}
			else if (rb_SingleCharacter.Checked)
			{
				SpecialEffectsViewerPreferences.that.Scene = (int)Scene.singlecharacter;
				SetSceneType(Scene.singlecharacter);
			}
			else //if (rb_PlacedEffect.Checked)
			{
				SpecialEffectsViewerPreferences.that.Scene = (int)Scene.placedeffect;
				SetSceneType(Scene.placedeffect);
			}

			ApplyEffect();
		}

		/// <summary>
		/// Reloads the ElectronPanel w/ or w/out ground as detered by the
		/// checkstate.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void cb_Ground_click(object sender, EventArgs e)
		{
			SpecialEffectsViewerPreferences.that.Ground =
			(_itView_Ground.Checked = cb_Ground.Checked);

			StoreCameraState();

			_panel.Dispose();
			_panel = new ElectronPanel();

			ConfigureElectronPanel();
			OnLoad(EventArgs.Empty);

			if (rb_DoubleCharacter.Checked) // special case
			{
				mi_events_Event(null, EventArgs.Empty);
			}
			else
			{
				_altgroup = null;
				ApplyEffect();
			}
		}

		/// <summary>
		/// Search on keydown event when control has focus.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void tb_Search_keydown(object sender, KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case Keys.Enter:
					e.SuppressKeyPress = e.Handled = true;
					bu_Search_click(bu_SearchD, EventArgs.Empty);
					break;

				case Keys.Enter | Keys.Shift:
					e.SuppressKeyPress = e.Handled = true;
					bu_Search_click(bu_SearchU, EventArgs.Empty);
					break;
			}
		}

		/// <summary>
		/// Search the Fx-list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void bu_Search_click(object sender, EventArgs e)
		{
			int count = lb_Effects.Items.Count;
			if (count != 0)
			{
				string text = tb_Search.Text;
				if (!String.IsNullOrEmpty(text))
				{
					text = text.ToLower();

					int id;

					var bu = sender as Button;
					if (bu == bu_SearchD)
					{
						if (lb_Effects.SelectedIndex == count - 1)
						{
							id = 0;
						}
						else
							id = lb_Effects.SelectedIndex + 1;

						while (!lb_Effects.Items[id].ToString().ToLower().Contains(text))
						{
							if (id == lb_Effects.SelectedIndex) // not found.
							{
								System.Media.SystemSounds.Beep.Play();
								break;
							}

							if (++id == count) // wrap to first node
							{
								id = 0;
							}
						}
					}
					else //if (bu == bu_SearchU)
					{
						if (lb_Effects.SelectedIndex < 1)
						{
							id = count - 1;
						}
						else
							id = lb_Effects.SelectedIndex - 1;

						while (!lb_Effects.Items[id].ToString().ToLower().Contains(text))
						{
							if (id == lb_Effects.SelectedIndex) // not found.
							{
								System.Media.SystemSounds.Beep.Play();
								break;
							}

							if (--id == -1) // wrap to last node
							{
								id = count - 1;
							}
						}
					}

					lb_Effects.SelectedIndex = id;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void cb_Filter_click(object sender, EventArgs e)
		{
			if (cb_Filter.Checked)
			{
				if (String.IsNullOrEmpty(_filtr = tb_Search.Text.ToLower()))
					cb_Filter.Checked = false;
			}
			else
				_filtr = String.Empty;

			_bypassSearchFocus = true;
			if (_itFxList_all.Checked)
			{
				_itFxList_all.Checked = false;
				mi_resrep_All(null, EventArgs.Empty);
			}
			else if (_itFxList_stock.Checked)
			{
				_itFxList_stock.Checked = false;
				mi_resrep_Stock(null, EventArgs.Empty);
			}
			else if (_itFxList_module.Checked)
			{
				_itFxList_module.Checked = false;
				mi_resrep_Module(null, EventArgs.Empty);
			}
			else if (_itFxList_campaign.Checked)
			{
				_itFxList_campaign.Checked = false;
				mi_resrep_Campaign(null, EventArgs.Empty);
			}
			else //if (_itFxList_override.Checked)
			{
				_itFxList_override.Checked = false;
				mi_resrep_Override(null, EventArgs.Empty);
			}
			_bypassSearchFocus = false;
		}

		/// <summary>
		/// [Enter] toggles the search-filter.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void cb_Filter_keydown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
			{
				e.Handled = e.SuppressKeyPress = true;
				cb_Filter.Checked = !cb_Filter.Checked;
				cb_Filter_click(null, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Selects all text on [Ctrl+a].
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void tb_keydown(object sender, KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case Keys.Control | Keys.A:
					e.Handled = e.SuppressKeyPress = true;
					var tb = sender as TextBox;
					tb.SelectionStart = 0;
					tb.SelectionLength = tb.Text.Length;
					break;

				// NOTE: Don't bother trying to handle [Ctrl+c] (whether or not
				// the toolset is set as the plugin's Owner ...) since the
				// toolset will freeze.
			}
		}

		/// <summary>
		/// Changes the mouse-cursor appropriately on mousedown event in the
		/// ElectronPanel.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void panel_mousedown(object sender, MouseEventArgs e)
		{
			if (_panel != null)
			{
				switch (e.Button)
				{
					case MouseButtons.Right:
					case MouseButtons.Middle:
						_Right = true;

						if ((ModifierKeys & Keys.Control) != 0)
							Cursor.Current = Cursors.Cross;
						else
							Cursor.Current = Cursors.SizeAll;
						break;
				}
			}
		}

		/// <summary>
		/// Reverts the mouse-cursor to default on the mouseup event in the
		/// ElectronPanel.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void panel_mouseup(object sender, EventArgs e)
		{
			_Right = false;
			Cursor.Current = Cursors.Default;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void panel_keydown(object sender, KeyEventArgs e)
		{
			if (_Right && (e.KeyData & Keys.Control) != 0)
			{
				_Ctrl = true;
				Cursor.Current = Cursors.Cross;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void panel_keyup(object sender, KeyEventArgs e)
		{
			if (_Ctrl)
			{
				_Ctrl = false;
				if (_Right)
					Cursor.Current = Cursors.SizeAll;
			}
		}

		/// <summary>
		/// Copies the currently selected special-effect string to the clipboard.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void bu_Copy_click(object sender, EventArgs e)
		{
			if (lb_Effects.SelectedIndex != -1)
			{
				// TODO: Ensure the clipboard is not locked by another process.
				Clipboard.Clear();
				Clipboard.SetText(lb_Effects.SelectedItem.ToString());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void bu_Clear_click(object sender, EventArgs e)
		{
			ClearScene();
			CreateBasicEvents();

			lb_Effects.SelectedIndex = -1;
		}

		/// <summary>
		/// Closes the plugin.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void bu_Close_click(object sender, EventArgs e)
		{
			Close();
		}
		#endregion eventhandlers (controls)


		#region Methods
		/// <summary>
		/// Clears the scene, the events-list, and the effects-list, then
		/// unchecks the repo-list so things are ready for the effects in a repo
		/// to be listed.
		/// </summary>
		void ClearEffectsList()
		{
			ClearScene();
			CreateBasicEvents();

			lb_Effects.SelectedIndex = -1;
			lb_Effects.Items.Clear();

			_itFxList_all     .Checked =
			_itFxList_stock   .Checked =
			_itFxList_module  .Checked =
			_itFxList_campaign.Checked =
			_itFxList_override.Checked = false;
		}

		/// <summary>
		/// Clears the scene.
		/// </summary>
		void ClearScene()
		{
			if (_panel.Scene != null) // netdisplaywindow is null on launch
			{
				_panel.Scene.SpecialEffectsManager.EndUpdating();
				_panel.Scene.SpecialEffectsManager.Groups.Clear();

				var objects = new NetDisplayObjectCollection();
				foreach (NetDisplayObject @object in _panel.Scene.Objects)
					objects.Add(@object);

				NWN2NetDisplayManager.Instance.RemoveObjects(objects);
			}
		}

		/// <summary>
		/// Clears the events-list and re-creates Play and Stop.
		/// </summary>
		void CreateBasicEvents()
		{
			_itEvents.MenuItems.Clear();

			_itEvents_Play = _itEvents.MenuItems.Add("&Play", mi_events_Play);
			_itEvents_Stop = _itEvents.MenuItems.Add("&Stop", mi_events_Stop);

			_itEvents_Play.Shortcut = Shortcut.F5;
			_itEvents_Stop.Shortcut = Shortcut.F6;
		}

		/// <summary>
		/// Checks if any event is currently checked in the Events menu for
		/// DoubleCharacter active.
		/// </summary>
		/// <returns>true if any event is checked</returns>
		bool CanPlayEvents()
		{
			for (int i = ItemsReserved; i != _itEvents.MenuItems.Count; ++i)
			{
				if (_itEvents.MenuItems[i].Checked)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Toggles view-type checkboxes on the View menu and keeps them
		/// synchronized.
		/// </summary>
		/// <param name="scene"></param>
		void SetSceneType(Scene scene)
		{
			_itView_DoubleCharacter.Checked = (scene == Scene.doublecharacter);
			_itView_SingleCharacter.Checked = (scene == Scene.singlecharacter);
			_itView_PlacedEffect   .Checked = (scene == Scene.placedeffect);
		}

		/// <summary>
		/// Clears the scene and the Events menu then instantiates objects and
		/// applies the current effect.
		/// </summary>
		void ApplyEffect()
		{
			ClearScene();
			CreateBasicEvents();

			if (lb_Effects.SelectedIndex != -1)
			{
				if (rb_PlacedEffect.Checked)
				{
					var blueprint = new NWN2PlacedEffectBlueprint();
					var iinstance = NWN2GlobalBlueprintManager.CreateInstanceFromBlueprint(blueprint);
					(iinstance as NWN2PlacedEffectTemplate).Active = true;
					(iinstance as NWN2PlacedEffectTemplate).Effect = _effect;

					NetDisplayObject oPlacedEffect = NWN2NetDisplayManager.Instance.CreateNDOForInstance(iinstance, _panel.Scene, 0);

					oPlacedEffect.Position = new Vector3(100f,100f,0f);
					NWN2NetDisplayManager.Instance.MoveObjects(new NetDisplayObjectCollection(oPlacedEffect), ChangeType.Absolute, false, oPlacedEffect.Position);
				}
				else if (rb_SingleCharacter.Checked)
				{
					var iIdiot1 = new NWN2CreatureInstance();
					iIdiot1.AppearanceType.Row = 4; // half-elf source/target
					iIdiot1.AppearanceSEF = _effect;

					NetDisplayObject oIdiot1 = NWN2NetDisplayManager.Instance.CreateNDOForInstance(iIdiot1, _panel.Scene, 0);

					oIdiot1.Position = new Vector3(100f,100f,0f);
					NWN2NetDisplayManager.Instance.MoveObjects(new NetDisplayObjectCollection(oIdiot1), ChangeType.Absolute, false, oIdiot1.Position);
				}
				else //if (rb_DoubleCharacter.Checked)
					LoadSefgroup(_sefgroup);

				PrintEffectData();

				if (!rb_DoubleCharacter.Checked || CanPlayEvents())
					_panel.Scene.SpecialEffectsManager.BeginUpdating();
			}
		}

		/// <summary>
		/// Loads a SEFGroup. Only double-characters play a sefgroup. A single-
		/// character could play a sefgroup I suppose, but prefer to use single-
		/// character to apply 'AppearanceSEF'.
		/// </summary>
		/// <param name="sefgroup"></param>
		void LoadSefgroup(SEFGroup sefgroup)
		{
			var iIdiot1 = new NWN2CreatureInstance();
			var iIdiot2 = new NWN2CreatureInstance();
			iIdiot1.AppearanceType.Row = 5; // half-orc source
			iIdiot2.AppearanceType.Row = 2; // gnome target

			NetDisplayObject oIdiot1 = NWN2NetDisplayManager.Instance.CreateNDOForInstance(iIdiot1, _panel.Scene, 0);
			NetDisplayObject oIdiot2 = NWN2NetDisplayManager.Instance.CreateNDOForInstance(iIdiot2, _panel.Scene, 0);

			oIdiot1.Position = new Vector3(103f,100f,0f);
			oIdiot2.Position = new Vector3( 97f,100f,0f);
			oIdiot1.Orientation = RHQuaternion.RotationZ(-(float)Math.PI / 2f);
			oIdiot2.Orientation = RHQuaternion.RotationZ( (float)Math.PI / 2f);

			var col1 = new NetDisplayObjectCollection(oIdiot1);
			var col2 = new NetDisplayObjectCollection(oIdiot2);
			NWN2NetDisplayManager.Instance.MoveObjects(  col1, ChangeType.Absolute, false, oIdiot1.Position);
			NWN2NetDisplayManager.Instance.MoveObjects(  col2, ChangeType.Absolute, false, oIdiot2.Position);
			NWN2NetDisplayManager.Instance.RotateObjects(col1, ChangeType.Absolute,        oIdiot1.Orientation);
			NWN2NetDisplayManager.Instance.RotateObjects(col2, ChangeType.Absolute,        oIdiot2.Orientation);

			sefgroup.FirstObject  = oIdiot1;
			sefgroup.SecondObject = oIdiot2;
			_panel.Scene.SpecialEffectsManager.Groups.Add(sefgroup);
		}

		/// <summary>
		/// Prints the currently loaded effect-events to the left panel. Adds an
		/// item to the Events menu for each event.
		/// </summary>
		void PrintEffectData()
		{
			if (_sefgroup != null)
			{
				if (rb_DoubleCharacter.Checked)
				{
					_itEvents.MenuItems.Add("-");

					_itEvents_Enable  = _itEvents.MenuItems.Add("&Enable all events",  mi_events_Event);
					_itEvents_Disable = _itEvents.MenuItems.Add("&Disable all events", mi_events_Event);

					_itEvents_Enable .Shortcut = Shortcut.F7;
					_itEvents_Disable.Shortcut = Shortcut.F8;

					_itEvents.MenuItems.Add("-");
				}

				string text = String.Empty;
				string L = Environment.NewLine;

				text += "[" + _sefgroup.Name + "]"                       + L;
				text += "pos - " + GetPositionString(_sefgroup.Position) + L;
//				text += "1st - " + _sefgroup.FirstObject                 + L;
//				text += "2nd - " + _sefgroup.SecondObject                + L;
				text += "fog - " + _sefgroup.FogMultiplier               + L;
				text += "dur - " + _sefgroup.HasMaximumDuration          + L;
				text += "dur - " + _sefgroup.MaximumDuration             + L;
				text += _sefgroup.SpecialTargetPosition;

				tb_SefData.Text = text;
				int width = TextRenderer.MeasureText(text, Font).Width + 4;

				text = String.Empty;
				ISEFEvent sefevent;
				for (int i = 0; i != _sefgroup.Events.Count; ++i)
				{
					// NOTE: a line is 13 px high (+5 pad total)
					if (text != String.Empty) text += L + L;

					sefevent = _sefgroup.Events[i];

					text += i.ToString();
					if (!String.IsNullOrEmpty(sefevent.Name))
						text += " [" + sefevent.Name + "]";
					text += L;

					string file = GetFileLabel(sefevent);
					if (file != null) text += file + L;

					int p = 6;
					if (_itView_ExtendedInfo.Checked)
					{
						if      (sefevent as SEFGameModelEffect  != null) p = 7;
						else if (sefevent as SEFLight            != null
							  || sefevent as SEFProjectedTexture != null) p = 12;
					}

					text += BwResourceTypes.GetResourceTypeString(sefevent.ResourceType)  + L;
					text += sefevent.EffectType                                           + L;
					text += pad("pos",    p) + GetPositionString(sefevent.Position)       + L;
					text += pad("orient", p) + (sefevent as SEFEvent).UseOrientedPosition + L;
					text += pad("1st",    p) + sefevent.FirstAttachmentObject             + L;
					text += pad("1st",    p) + sefevent.FirstAttachment                   + L;
					text += pad("2nd",    p) + sefevent.SecondAttachmentObject            + L;
					text += pad("2nd",    p) + sefevent.SecondAttachment                  + L;
					text += pad("delay",  p) + sefevent.Delay                             + L;
					text += pad("dur",    p) + sefevent.HasMaximumDuration;
					if (sefevent.HasMaximumDuration)
						text += L + pad("dur", p) + sefevent.MaximumDuration;

//					if (sefevent.Parent != null)
//					{
//						text += L + "parent - " + sefevent.Parent;
//						text += L + "parent - " + GetPositionString(sefevent.ParentPosition);
//					}


					if (_itView_ExtendedInfo.Checked)
					{
//						if (sefevent as SEFBeam != null)
//						{
//							// none.
//						}
//						else if (sefevent as SEFBillboard != null)
//						{
//							// none.
//						}
//						else if (sefevent as SEFEvent != null)
//						{
//							// Can a SEFEvent be assigned to a SEFEvent.
//							// SEFEvents *are* SEFEvents ... do not enable this because
//							// it takes precedence over any following types
//						}
						if (sefevent as SEFGameModelEffect != null)
						{
							var modeleffect = (sefevent as SEFGameModelEffect);
							text += L + "type    - " + modeleffect.GameModelEffectType;

							string texture = modeleffect.TextureName.ToString();
							if (!String.IsNullOrEmpty(texture))
								text += L + "texture - " + texture;

							text += L + "alpha   - " + modeleffect.Alpha;
							text += L + "tint    - " + modeleffect.SkinTintColor;
							text += L + "lerpin  - " + modeleffect.LerpInTime;
							text += L + "lerpout - " + modeleffect.LerpOutTime;
						}
						else if (sefevent as SEFLight != null)
						{
							var light = (sefevent as SEFLight);
							text += L + "range        - " + light.LightRange;
							text += L + "fadein       - " + light.FadeInTime;

							text += L + "shadow       - " + light.CastsShadow;
							if (light.CastsShadow)
								text += L + "shadow       - " + light.ShadowIntensity;

							text += L + "flicker      - " + light.Flicker;
							if (light.Flicker)
							{
								text += L + "flicker      - " + light.FlickerType;
								text += L + "flicker_rate - " + light.FlickerRate;
								text += L + "flicker_vari - " + light.FlickerVariance;
							}
							text += L + "lerp         - " + light.Lerp;
							if (light.Lerp)
								text += L + "lerp         - " + light.LerpPeriod;

							text += L + "vision       - " + light.VisionEffect;
							text += L + "start        - " + SplitLip(light.StartLighting);
							text += L + "end          - " + SplitLip(light.EndLighting);
						}
//						else if (sefevent as SEFLightning != null)
//						{
//							// none.
//						}
//						else if (sefevent as SEFLineParticleSystem != null)
//						{
//							// none.
//						}
						else if (sefevent as SEFModel != null)
						{
							var model = (sefevent as SEFModel);
							text += L + "skel   - " + model.SkeletonFile;
							text += L + "ani    - " + model.AnimationToPlay;
							text += L + "loop   - " + model.Looping;
							text += L + "tint   - " + model.TintSet;

							string sef = model.SEFToPlayOnModel.ToString();
							if (!String.IsNullOrEmpty(sef))
								text += L + "sef    - " + sef;
//								+ "." + BwResourceTypes.GetResourceTypeString(model.SEFToPlayOnModel.ResourceType); // .sef
						}
						else if (sefevent as SEFParticleMesh != null)
						{
							var mesh = (sefevent as SEFParticleMesh);
							string parts = String.Empty;
							for (int j = 0; j != mesh.ModelParts.Count; ++j)
							{
								if (!String.IsNullOrEmpty(parts)) parts += L + "         ";
								parts += mesh.ModelParts[j].ToString();
							}

							if (parts != String.Empty)
								text += L + "parts  - " + parts;
						}
//						else if (sefevent as SEFParticleSystem != null)
//						{
//							// none.
//						}
						else if (sefevent as SEFProjectedTexture != null)
						{
							var texture = (sefevent as SEFProjectedTexture);
							text += L + "texture      - " + texture.Texture;
							text += L + "ground       - " + texture.GroundOnly;
							text += L + "fadein       - " + texture.FadeInTime;
							text += L + "projection   - " + texture.ProjectionType;
							text += L + "orientation  - " + GetPositionString(texture.Orientation);

							text += L + "height       - " + texture.Height;
							if (texture.HeightEnd != texture.Height)
								text += L + "heightend    - " + texture.HeightEnd;
							text += L + "width        - " + texture.Width;
							if (texture.WidthEnd != texture.Width)
								text += L + "widthend     - " + texture.WidthEnd;
							text += L + "length       - " + texture.Length;
							if (texture.LengthEnd != texture.Length)
								text += L + "lengthend    - " + texture.LengthEnd;

							text += L + "color        - " + texture.Color;
							if (texture.ColorEnd != texture.Color)
								text += L + "colorend     - " + texture.ColorEnd;

							text += L + "lerp         - " + texture.Lerp;
							if (texture.Lerp)
								text += L + "lerp_period  - " + texture.LerpPeriod;

							text += L + "rot          - " + texture.InitialRotation;
							text += L + "rot_veloc    - " + texture.RotationalVelocity;
							text += L + "rot_accel    - " + texture.RotationalAcceleration;

							text += L + "fov          - " + texture.FOV;
							if (texture.FOVEnd != texture.FOV)
								text += L + "fovend       - " + texture.FOVEnd;

							text += L + "blend        - " + texture.Blending;
							text += L + "blend_source - " + texture.SourceBlendMode;
							text += L + "blend_dest   - " + texture.DestBlendMode;
						}
						else if (sefevent as SEFSound != null)
						{
							var sound = (sefevent as SEFSound);
							text += L + "loop   - " + sound.SoundLoops;
						}
						else if (sefevent as SEFTrail != null)
						{
							var trail = (sefevent as SEFTrail);
							text += L + "width  - " + trail.TrailWidth;
						}
					}

					if (rb_DoubleCharacter.Checked)
					{
						var it = _itEvents.MenuItems.Add("event " + i, mi_events_Event);
						it.Tag = i;
						it.Checked = true;
					}
				}
				tb_EventData.Text = text;

				width = Math.Max(width, TextRenderer.MeasureText(text, Font).Width + 4
										+ SystemInformation.VerticalScrollBarWidth);
				sc2_Options.SplitterDistance = Math.Max(width, WidthOptions);
			}
		}

		/// <summary>
		/// Stores the current camera-state in Preferences.
		/// @note Ensure that the ElectronPanel (etc) is valid before call.
		/// </summary>
		void StoreCameraState()
		{
			var receiver = (_panel.CameraMovementReceiver as ModelViewerInputCameraReceiver);
			var state = (receiver.CameraState as ModelViewerInputCameraReceiverState);
			SpecialEffectsViewerPreferences.that.FocusPhi     = state.FocusPhi;
			SpecialEffectsViewerPreferences.that.FocusTheta   = state.FocusTheta;
			SpecialEffectsViewerPreferences.that.Distance     = state.Distance;
			SpecialEffectsViewerPreferences.that.FocusPoint_x = state.FocusPoint.X;
			SpecialEffectsViewerPreferences.that.FocusPoint_y = state.FocusPoint.Y;
			SpecialEffectsViewerPreferences.that.FocusPoint_z = state.FocusPoint.Z;
		}
		#endregion Methods


		#region Methods (static)
		/// <summary>
		/// Checks if the plugin's initial location is onscreen.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		static bool checklocation(int x, int y)
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

		/// <summary>
		/// Gets a string for a 3d-vector.
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		static string GetPositionString(Vector3 vec)
		{
			return vec.X + "," + vec.Y + "," + vec.Z;
		}

		/// <summary>
		/// Gets the label of a definition file for a SEFEvent if one exists.
		/// </summary>
		/// <param name="sefevent"></param>
		/// <returns>null if not found</returns>
		static string GetFileLabel(ISEFEvent sefevent)
		{
			if (   sefevent.DefinitionFile              != null
				&& sefevent.DefinitionFile.ResRef       != null
				&& sefevent.DefinitionFile.ResRef.Value != String.Empty)
			{
				return sefevent.DefinitionFile.ResRef.Value;
			}
			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="in"></param>
		/// <param name="len"></param>
		/// <returns></returns>
		static string pad(string @in, int len)
		{
			while (@in.Length != len)
				@in += " ";

			return @in + " - ";
		}

		/// <summary>
		/// Splits a LightIntensityPair.
		/// </summary>
		/// <param name="pair"></param>
		/// <returns></returns>
		static string SplitLip(object pair)
		{
			string diff = pair.ToString();

			int pos = diff.IndexOf("Intensity");
			string inte = diff.Substring(pos);

			diff = diff.Substring(0, diff.Length - inte.Length - 2);
			pos = diff.IndexOf("Ambient");
			string ambi = diff.Substring(pos);

			diff = diff.Substring(0, diff.Length - ambi.Length - 2);
			pos = diff.IndexOf("Specular");
			string spec = diff.Substring(pos);

			diff = diff.Substring(0, diff.Length - spec.Length - 2);

			return diff                     + Environment.NewLine
				 + "               " + spec + Environment.NewLine
				 + "               " + ambi + Environment.NewLine
				 + "               " + inte;
		}
		#endregion Methods (static)
	}
}
