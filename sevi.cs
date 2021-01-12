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
		enum Scene
		{
			none,				// 0
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

		const int ItemsReserved = 5;

		const int MI_EFFECTS = 0;
		const int MI_EVENTS  = 1;
		const int MI_VIEW    = 2;
		const int MI_HELP    = 3;

		const int MI_EVENTS_PLAY    = 0;
		const int MI_EVENTS_DISABLE = 2;
		const int MI_EVENTS_ENABLE  = 3;

//		const int MI_VIEW_TOP = 2;
		#endregion Fields (static)


		#region Fields
		IResourceEntry _effect; SEFGroup _sefgroup, _altgroup;

		ElectronPanel _panel = new ElectronPanel(); // i hate u

		MenuItem _itFxList_all;
		MenuItem _itFxList_stock;
		MenuItem _itFxList_module;
		MenuItem _itFxList_campaign;
		MenuItem _itFxList_override;

		MenuItem _itStayOnTop;
		MenuItem _itOptions;

		int _isListStale;

		string _filtr = String.Empty;
		bool _bypassActivateSearchControl;

		bool _bypassEventsClear;

		string _pfe_helpfile;
		#endregion Fields


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		internal sevi()
		{
			Owner = NWN2ToolsetMainForm.App;

			InitializeComponent();

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

			effectsAll_click(null, EventArgs.Empty);

//			logger.log(StringDecryptor.Decrypt("ᒼᒮᒯ"));

			Show();
		}

		/// <summary>
		/// Instantiates the plugin's MainMenu.
		/// </summary>
		void CreateMainMenu()
		{
			Menu = new MainMenu();

			MenuItem it;

			Menu.MenuItems.Add("E&ffects");		// 0
			it = Menu.MenuItems.Add("&Events");	// 1
			it.Popup += events_popout;
			Menu.MenuItems.Add("&View");		// 2
			Menu.MenuItems.Add("&Help");		// 3

// List ->
			_itFxList_all      = Menu.MenuItems[MI_EFFECTS].MenuItems.Add("list &all effects", effectsAll_click);
								 Menu.MenuItems[MI_EFFECTS].MenuItems.Add("-");
			_itFxList_stock    = Menu.MenuItems[MI_EFFECTS].MenuItems.Add("&stock only",       effectsStock_click);
			_itFxList_module   = Menu.MenuItems[MI_EFFECTS].MenuItems.Add("&module only",      effectsModule_click);
			_itFxList_campaign = Menu.MenuItems[MI_EFFECTS].MenuItems.Add("&campaign only",    effectsCampaign_click);
			_itFxList_override = Menu.MenuItems[MI_EFFECTS].MenuItems.Add("&override only",    effectsOverride_click);

			_itFxList_all     .Shortcut = Shortcut.Ctrl1;
			_itFxList_stock   .Shortcut = Shortcut.Ctrl2;
			_itFxList_module  .Shortcut = Shortcut.Ctrl3;
			_itFxList_campaign.Shortcut = Shortcut.Ctrl4;
			_itFxList_override.Shortcut = Shortcut.Ctrl5;

// Events ->
			it = Menu.MenuItems[MI_EVENTS].MenuItems.Add("&Play", eventsPlay_click);
			it.Shortcut = Shortcut.F5;

// View ->
			_itOptions = Menu.MenuItems[MI_VIEW].MenuItems.Add("show &Options panel", viewOptions_click);
			_itOptions.Shortcut = Shortcut.F8;

			Menu.MenuItems[MI_VIEW].MenuItems.Add("-");

			_itStayOnTop = Menu.MenuItems[MI_VIEW].MenuItems.Add("stay on &top", viewStayOnTop_click);
			_itStayOnTop.Shortcut = Shortcut.CtrlT;
			_itStayOnTop.Checked = true;

// Help ->
			_pfe_helpfile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			_pfe_helpfile = Path.Combine(_pfe_helpfile, "SpecialEffectsViewer.txt");
			if (File.Exists(_pfe_helpfile))
			{
				it = Menu.MenuItems[MI_HELP].MenuItems.Add("&help",  helpHelp_click);
				it.Shortcut = Shortcut.F1;
			}
			it = Menu.MenuItems[MI_HELP].MenuItems.Add("&about", helpAbout_click);
			it.Shortcut = Shortcut.F2;
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
				_itOptions.PerformClick();

			sc1_Effects.SplitterDistance = SpecialEffectsViewerPreferences.that.SplitterDistanceEffects;
			sc3_Events .SplitterDistance = SpecialEffectsViewerPreferences.that.SplitterDistanceEvents;

			if (!SpecialEffectsViewerPreferences.that.StayOnTop)
				_itStayOnTop.PerformClick();

			if (SpecialEffectsViewerPreferences.that.Maximized)
				WindowState = FormWindowState.Maximized;

			if (SpecialEffectsViewerPreferences.that.Scene != (int)Scene.doublecharacter)
			{
				rb_DoubleCharacter.Checked = false;

				switch ((Scene)SpecialEffectsViewerPreferences.that.Scene)
				{
					default:                    rb_DoubleCharacter.Checked = true; break;
					case Scene.singlecharacter: rb_SingleCharacter.Checked = true; break;
					case Scene.placedeffect:    rb_PlacedEffect   .Checked = true; break;
				}
			}

			cb_Ground.Checked = SpecialEffectsViewerPreferences.that.Ground;
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
				NWN2NetDisplayManager.Instance.UpdateTerrainSize(_panel.NDWindow.Scene, new Size(4,4)); // note: 4x4 is min area (200x200)

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
					effectsAll_click(null, EventArgs.Empty);
				}
				else if (_itFxList_module.Checked)
				{
					if ((_isListStale & STALE_Module) != 0)
					{
						_itFxList_module.Checked = false;
						effectsModule_click(null, EventArgs.Empty);
					}
				}
				else if (_itFxList_campaign.Checked)
				{
					if ((_isListStale & STALE_Campaign) != 0)
					{
						_itFxList_campaign.Checked = false;
						effectsCampaign_click(null, EventArgs.Empty);
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


		#region eventhandlers (effects)
		/// <summary>
		/// Populates the Fx-list with everything it can find.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void effectsAll_click(object sender, EventArgs e)
		{
			if (!_itFxList_all.Checked)
			{
				lb_Effects.BeginUpdate();

				ClearFxList();
				_itFxList_all.Checked = true;

				var entries = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF));
				foreach (IResourceEntry entry in entries)
				{
					if (_filtr == String.Empty || entry.ResRef.Value.ToLower().Contains(_filtr))
						lb_Effects.Items.Add(entry);
				}
				lb_Effects.EndUpdate();
			}

			if (!_bypassActivateSearchControl)
				ActiveControl = tb_Search;
		}

		/// <summary>
		/// Populates the Fx-list from the stock data folder.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void effectsStock_click(object sender, EventArgs e)
		{
			if (!_itFxList_stock.Checked)
			{
				lb_Effects.BeginUpdate();

				ClearFxList();
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

			if (!_bypassActivateSearchControl)
				ActiveControl = tb_Search;
		}

		/// <summary>
		/// Populates the Fx-list from the current module folder.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void effectsModule_click(object sender, EventArgs e)
		{
			if (!_itFxList_module.Checked)
			{
				lb_Effects.BeginUpdate();

				ClearFxList();
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

			if (!_bypassActivateSearchControl)
				ActiveControl = tb_Search;
		}

		/// <summary>
		/// Populates the Fx-list from the current campaign folder.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void effectsCampaign_click(object sender, EventArgs e)
		{
			if (!_itFxList_campaign.Checked)
			{
				lb_Effects.BeginUpdate();

				ClearFxList();
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

			if (!_bypassActivateSearchControl)
				ActiveControl = tb_Search;
		}

		/// <summary>
		/// Populates the Fx-list from the override folder.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void effectsOverride_click(object sender, EventArgs e)
		{
			if (!_itFxList_override.Checked)
			{
				lb_Effects.BeginUpdate();

				ClearFxList();
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

			if (!_bypassActivateSearchControl)
				ActiveControl = tb_Search;
		}

		/// <summary>
		/// Clears the Fx-list and unchecks the List menuitems.
		/// </summary>
		void ClearFxList()
		{
			bu_Clear_click(null, EventArgs.Empty);

			lb_Effects.SelectedIndex = -1;
			lb_Effects.Items.Clear();

			_itFxList_all     .Checked =
			_itFxList_stock   .Checked =
			_itFxList_module  .Checked =
			_itFxList_campaign.Checked =
			_itFxList_override.Checked = false;
		}
		#endregion eventhandlers (effects)


		#region eventhandlers (events)
		/// <summary>
		/// Disables Play if there is not a selected effect.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void events_popout(object sender, EventArgs e)
		{
			Menu.MenuItems[MI_EVENTS].MenuItems[MI_EVENTS_PLAY].Enabled = (lb_Effects.SelectedIndex != -1);
		}

		/// <summary>
		/// Plays the currently selected effect.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void eventsPlay_click(object sender, EventArgs e)
		{
			_panel.NDWindow.Scene.SpecialEffectsManager.EndUpdating();

			if (lb_Effects.SelectedIndex != -1)
			{
				if (rb_DoubleCharacter.Checked && CanPlayEvents())
				{
					// clear the netdisplay
					// - recreate the scene in case a solo-event was played previously.
					// - the toolset code is not as cooperative as I'd like ...
					_bypassEventsClear = true;
					bu_Clear_click(null, EventArgs.Empty);
					_bypassEventsClear = false;

					_altgroup = null;

					Events_click(null, EventArgs.Empty);
				}
				else
					_panel.NDWindow.Scene.SpecialEffectsManager.BeginUpdating();
			}
		}

		/// <summary>
		/// Checks if any event is currently checked in the Events menu for
		/// DoubleCharacter active.
		/// </summary>
		/// <returns>true if any event is checked</returns>
		bool CanPlayEvents()
		{
			for (int i = ItemsReserved; i != Menu.MenuItems[MI_EVENTS].MenuItems.Count; ++i)
			{
				if (Menu.MenuItems[MI_EVENTS].MenuItems[i].Checked)
					return true;
			}
			return false;
		}
		#endregion eventhandlers (events)


		#region eventhandlers (view)
		/// <summary>
		/// Toggles display of the Options/Events sidepanel.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void viewOptions_click(object sender, EventArgs e)
		{
			if (!sc2_Options.Panel1Collapsed)
				SpecialEffectsViewerPreferences.that.SplitterDistanceEvents = sc3_Events.SplitterDistance; // workaround.

			if (!(sc2_Options.Panel1Collapsed = !(SpecialEffectsViewerPreferences.that.OptionsPanel =
												 (_itOptions.Checked = !_itOptions.Checked))))
			{
				sc3_Events.SplitterDistance = SpecialEffectsViewerPreferences.that.SplitterDistanceEvents; // workaround.
			}
		}

		/// <summary>
		/// Toggles toolset ownership of the plugin-window.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void viewStayOnTop_click(object sender, EventArgs e)
		{
			if (SpecialEffectsViewerPreferences.that.StayOnTop =
			   (_itStayOnTop.Checked = !_itStayOnTop.Checked))
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
		void helpHelp_click(object sender, EventArgs e)
		{
			if (File.Exists(_pfe_helpfile)) Process.Start(_pfe_helpfile);
		}

		/// <summary>
		/// Displays the About dialog.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void helpAbout_click(object sender, EventArgs e)
		{
			using (var f = new AboutF())
				f.ShowDialog(this);
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
				_effect = null; _sefgroup = null; _altgroup = null;
				Text = TITLE;
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
				eventsPlay_click(null, EventArgs.Empty);
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
			}
			else if (rb_SingleCharacter.Checked)
			{
				SpecialEffectsViewerPreferences.that.Scene = (int)Scene.singlecharacter;
			}
			else //if (rb_PlacedEffect.Checked)
			{
				SpecialEffectsViewerPreferences.that.Scene = (int)Scene.placedeffect;
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
			SpecialEffectsViewerPreferences.that.Ground = cb_Ground.Checked;
			StoreCameraState();

			_panel.Dispose();
			_panel = new ElectronPanel();

			ConfigureElectronPanel();
			OnLoad(EventArgs.Empty);

			if (rb_DoubleCharacter.Checked) // special case
			{
				Events_click(null, EventArgs.Empty);
			}
			else
			{
				_altgroup = null;
				ApplyEffect();
			}
		}

		/// <summary>
		/// Clears the scene and the Events menu then instantiates objects and
		/// applies the current effect.
		/// </summary>
		void ApplyEffect()
		{
			bu_Clear_click(null, EventArgs.Empty);

			if (lb_Effects.SelectedIndex != -1)
			{
				if (rb_PlacedEffect.Checked)
				{
					var blueprint = new NWN2PlacedEffectBlueprint();
					var iinstance = NWN2GlobalBlueprintManager.CreateInstanceFromBlueprint(blueprint);
					(iinstance as NWN2PlacedEffectTemplate).Active = true;
					(iinstance as NWN2PlacedEffectTemplate).Effect = _effect;

					NetDisplayObject oPlacedEffect = NWN2NetDisplayManager.Instance.CreateNDOForInstance(iinstance, _panel.NDWindow.Scene, 0);

					oPlacedEffect.Position = new Vector3(100f,100f,0f);
					NWN2NetDisplayManager.Instance.MoveObjects(new NetDisplayObjectCollection(oPlacedEffect), ChangeType.Absolute, false, oPlacedEffect.Position);
				}
				else if (rb_SingleCharacter.Checked)
				{
					var iIdiot1 = new NWN2CreatureInstance();
					iIdiot1.AppearanceType.Row = 4; // half-elf source/target
					iIdiot1.AppearanceSEF = _effect;

					NetDisplayObject oIdiot1 = NWN2NetDisplayManager.Instance.CreateNDOForInstance(iIdiot1, _panel.NDWindow.Scene, 0);

					oIdiot1.Position = new Vector3(100f,100f,0f);
					NWN2NetDisplayManager.Instance.MoveObjects(new NetDisplayObjectCollection(oIdiot1), ChangeType.Absolute, false, oIdiot1.Position);
				}
				else //if (rb_DoubleCharacter.Checked)
				{
					LoadSefgroup(_sefgroup);
				}

				PrintSefData(_sefgroup);
				_panel.NDWindow.Scene.SpecialEffectsManager.BeginUpdating();
			}
		}

		/// <summary>
		/// Alters the current effect in accord with the Events menu and then
		/// plays it.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Events_click(object sender, EventArgs e)
		{
			if (lb_Effects.SelectedIndex != -1)
			{
				var it = sender as MenuItem;

				bool isDisable = false;
				bool isSolo    = false;

				if (sender != null) // if NOT cb_Ground_click() ie. is a real Events click ->
				{
					// set the items' check
					if (it == Menu.MenuItems[MI_EVENTS].MenuItems[MI_EVENTS_DISABLE]) // disable all events
					{
						isDisable = true;
						for (int i = ItemsReserved; i != Menu.MenuItems[MI_EVENTS].MenuItems.Count; ++i)
							Menu.MenuItems[MI_EVENTS].MenuItems[i].Checked = false;
					}
					else if (it == Menu.MenuItems[MI_EVENTS].MenuItems[MI_EVENTS_ENABLE]) // enable all events
					{
						for (int i = ItemsReserved; i != Menu.MenuItems[MI_EVENTS].MenuItems.Count; ++i)
							Menu.MenuItems[MI_EVENTS].MenuItems[i].Checked = true;
					}
					else if (!(isSolo = (ModifierKeys == Keys.Shift)))
						it.Checked = !it.Checked;

					// clear the netdisplay
					// - don't bother with this for a Ground-toggle since the ElectronPanel will be recreated for that.
					_bypassEventsClear = true;
					bu_Clear_click(null, EventArgs.Empty);
					_bypassEventsClear = false;

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
							if (!Menu.MenuItems[MI_EVENTS].MenuItems[i + ItemsReserved].Checked)
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
					_panel.NDWindow.Scene.SpecialEffectsManager.BeginUpdating();
			}
		}

		/// <summary>
		/// Instantiates a SEFGroup. Only double-characters play a sefgroup. A
		/// single-character could play a sefgroup I suppose, but prefer to use
		/// single-character to apply 'AppearanceSEF'.
		/// </summary>
		/// <param name="sefgroup"></param>
		void LoadSefgroup(SEFGroup sefgroup)
		{
			var iIdiot1 = new NWN2CreatureInstance();
			var iIdiot2 = new NWN2CreatureInstance();
			iIdiot1.AppearanceType.Row = 5; // half-orc source
			iIdiot2.AppearanceType.Row = 2; // gnome target

			NetDisplayObject oIdiot1 = NWN2NetDisplayManager.Instance.CreateNDOForInstance(iIdiot1, _panel.NDWindow.Scene, 0);
			NetDisplayObject oIdiot2 = NWN2NetDisplayManager.Instance.CreateNDOForInstance(iIdiot2, _panel.NDWindow.Scene, 0);

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
			_panel.NDWindow.Scene.SpecialEffectsManager.Groups.Add(sefgroup);
		}

		/// <summary>
		/// Clears the scene.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void bu_Clear_click(object sender, EventArgs e)
		{
			if (_panel.NDWindow != null) // netdisplaywindow is null on launch
			{
				_panel.NDWindow.Scene.SpecialEffectsManager.EndUpdating();
				_panel.NDWindow.Scene.SpecialEffectsManager.Groups.Clear();

				var objects = new NetDisplayObjectCollection();
				foreach (NetDisplayObject @object in _panel.Scene.Objects)
					objects.Add(@object);

				NWN2NetDisplayManager.Instance.RemoveObjects(objects);

				if (!_bypassEventsClear)
				{
					Menu.MenuItems[MI_EVENTS].MenuItems.Clear();

					MenuItem it = Menu.MenuItems[MI_EVENTS].MenuItems.Add("&Play", eventsPlay_click);
					it.Shortcut = Shortcut.F5;
				}
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
		/// Closes the plugin.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void bu_Close_click(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Search on keydown event when control has focus.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void tb_Search_keydown(object sender, KeyEventArgs e)
		{
			//logger.log("tb_Search_keydown()");
			switch (e.KeyData)
			{
				case Keys.Enter:
					//logger.log("Keys.Enter");
					e.SuppressKeyPress = e.Handled = true;
					bu_Search_click(bu_SearchD, EventArgs.Empty);
					break;

				case Keys.Enter | Keys.Shift:
					//logger.log("Keys.Enter | Keys.Shift");
					e.SuppressKeyPress = e.Handled = true;
					bu_Search_click(bu_SearchU, EventArgs.Empty);
					break;

//				case Keys.Control | Keys.A: // <- works okay don't know why
//				case Keys.Control | Keys.C:
//					//logger.log("Keys.Control | Keys.C"); // toolset freezes -> see tb_keydown()
//					tb_keydown(sender, e);
//					break;
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

			_bypassActivateSearchControl = true;
			if (_itFxList_all.Checked)
			{
				_itFxList_all.Checked = false;
				effectsAll_click(null, EventArgs.Empty);
			}
			else if (_itFxList_stock.Checked)
			{
				_itFxList_stock.Checked = false;
				effectsStock_click(null, EventArgs.Empty);
			}
			else if (_itFxList_module.Checked)
			{
				_itFxList_module.Checked = false;
				effectsModule_click(null, EventArgs.Empty);
			}
			else if (_itFxList_campaign.Checked)
			{
				_itFxList_campaign.Checked = false;
				effectsCampaign_click(null, EventArgs.Empty);
			}
			else //if (_itFxList_override.Checked)
			{
				_itFxList_override.Checked = false;
				effectsOverride_click(null, EventArgs.Empty);
			}
			_bypassActivateSearchControl = false;
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
			//logger.log("tb_keydown()");
			switch (e.KeyData)
			{
				case Keys.Control | Keys.A:
					//logger.log(". Keys.Control | Keys.C");
					e.Handled = e.SuppressKeyPress = true;
					var tb = sender as TextBox;
					tb.SelectionStart = 0;
					tb.SelectionLength = tb.Text.Length;
					break;

//				case Keys.Control | Keys.C: // toolset freezes ->
//					//logger.log(". Keys.Control | Keys.C");
//					// the toolset handles [Ctrl+c] if it's the Owner of this plugin
//					// so it needs to be re-handled here
//					if (Menu.MenuItems[MI_VIEW].MenuItems[MI_VIEW_TOP].Checked)
//					{
//						e.Handled = e.SuppressKeyPress = true;
//						Clipboard.Clear();
//						Clipboard.SetText((sender as TextBox).SelectedText); // doesn't work anyway.
//					}
//					break;
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
							if ((ModifierKeys & Keys.Control) != 0)
							{
								Cursor.Current = Cursors.Cross;
							}
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
			Cursor.Current = Cursors.Default;
		}
		#endregion eventhandlers (controls)


		#region Methods (static)
		/// <summary>
		/// Checks if the plugin's initial location is onscreen.
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


		#region Methods
		/// <summary>
		/// Prints the currently loaded Sef-events to the left panel. Adds an
		/// item to the Events menu for each event.
		/// </summary>
		/// <param name="sefgroup"></param>
		void PrintSefData(SEFGroup sefgroup)
		{
			string text = String.Empty;
			string L = Environment.NewLine;

			text += "[" + sefgroup.Name + "]" + L;
			text += sefgroup.Position.X + "," + sefgroup.Position.Y + "," + sefgroup.Position.Z + L;
			text += "1st - " + sefgroup.FirstObject + L;
			text += "2nd - " + sefgroup.SecondObject + L;
			text += "fog - " + sefgroup.FogMultiplier + L;
			text += "dur - " + sefgroup.HasMaximumDuration + L;
			text += "dur - " + sefgroup.MaximumDuration + L;
			text += sefgroup.SpecialTargetPosition;

			tb_SefData.Text = text;


			if (rb_DoubleCharacter.Checked)
			{
				Menu.MenuItems[MI_EVENTS].MenuItems.Add("-");
				Menu.MenuItems[MI_EVENTS].MenuItems.Add("&Disable all events", Events_click);
				Menu.MenuItems[MI_EVENTS].MenuItems.Add("&Enable all events",  Events_click);
				Menu.MenuItems[MI_EVENTS].MenuItems.Add("-");
			}

			text = String.Empty;
			ISEFEvent sefevent;
			for (int i = 0; i != sefgroup.Events.Count; ++i)
			{
				if (text != String.Empty) text += L + L;

				sefevent = sefgroup.Events[i];

				text += i + " [" + sefevent.Name + "]" + L;

				string file = GetFileLabel(sefevent);
				if (file != null) text += file + L;

				text += BwResourceTypes.GetResourceTypeString(sefevent.ResourceType) + L;

				text += sefevent.EffectType + L;
				text += sefevent.Position.X + "," + sefevent.Position.Y + "," + sefevent.Position.Z + L;

				text += "1st - "   + sefevent.FirstAttachmentObject + L;
				text += "1st - "   + sefevent.FirstAttachment + L;
				text += "2nd - "   + sefevent.SecondAttachmentObject + L;
				text += "2nd - "   + sefevent.SecondAttachment + L;
				text += "dur - "   + sefevent.HasMaximumDuration + L;
				text += "dur - "   + sefevent.MaximumDuration + L;
				text += "delay - " + sefevent.Delay;

				// NOTE: switch() possible here ->

				if (sefevent as SEFBeam != null)
				{
					// none.
				}
				else if (sefevent as SEFBillboard != null)
				{
					// none.
				}
//				else if (sefevent as SEFEvent != null)
//				{
//					// Can a SEFEvent be assigned to a SEFEvent.
//					// SEFEvents *are* SEFEvents ...
//				}
				else if (sefevent as SEFGameModelEffect != null)
				{
					var modeleffect = (sefevent as SEFGameModelEffect);
					text += L + "type - "    + modeleffect.GameModelEffectType;
					text += L + "texture - " + modeleffect.TextureName;
					text += L + "alpha - "   + modeleffect.Alpha;
					// TODO: plus a few other vars
				}
				else if (sefevent as SEFLight != null)
				{
					var light = (sefevent as SEFLight);
					text += L + "shadow - "  + light.CastsShadow;
					text += L + "shadow - "  + light.ShadowIntensity;
					text += L + "flicker - " + light.Flicker;
					text += L + "flicker - " + light.FlickerType;
					text += L + "lerp - "    + light.Lerp;
					text += L + "lerp - "    + light.LerpPeriod;
					// TODO: plus a lot of other vars
				}
				else if (sefevent as SEFLightning != null)
				{
					// none.
				}
				else if (sefevent as SEFLineParticleSystem != null)
				{
					// none.
				}
				else if (sefevent as SEFModel != null)
				{
					var model = (sefevent as SEFModel);
					text += L + "skel - " + model.SkeletonFile;
//					text += L + "tint - " + model.TintSet;
					text += L + "ani - "  + model.AnimationToPlay;
					text += L + "loop - " + model.Looping;
					text += L + "sef - "  + model.SEFToPlayOnModel;
				}
				else if (sefevent as SEFParticleMesh != null)
				{
//					var mesh = (sefevent as SEFParticleMesh);
//					text += L + " - " + mesh.ModelParts;
				}
				else if (sefevent as SEFParticleSystem != null)
				{
					// none.
				}
				else if (sefevent as SEFProjectedTexture != null)
				{
					var texture = (sefevent as SEFProjectedTexture);
					text += L + "texture - " + texture.Texture;
					// TODO: plus a lot of other vars
				}
				else if (sefevent as SEFSound != null)
				{
					var sound = (sefevent as SEFSound);
					text += L + "loop - " + sound.SoundLoops;
				}
				else if (sefevent as SEFTrail != null)
				{
					var trail = (sefevent as SEFTrail);
					text += L + "width - " + trail.TrailWidth;
				}

				if (rb_DoubleCharacter.Checked)
				{
					var it = Menu.MenuItems[MI_EVENTS].MenuItems.Add("event " + i, Events_click);
					it.Tag = i;
					it.Checked = true;
				}
			}
			tb_EventData.Text = text;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sefevent"></param>
		/// <returns></returns>
		string GetFileLabel(ISEFEvent sefevent)
		{
			if (   sefevent.DefinitionFile              != null
				&& sefevent.DefinitionFile.ResRef       != null
				&& sefevent.DefinitionFile.ResRef.Value != String.Empty)
			{
				return sefevent.DefinitionFile.ResRef.Value;
			}
			return null;
		}

/*		/// <summary>
		/// Concocts a label for a specified event.
		/// </summary>
		/// <param name="sefevent"></param>
		/// <returns></returns>
		string GetEventLabel(ISEFEvent sefevent)
		{
			string label;
			if (sefevent.Name != null)
				label = sefevent.Name;
			else
				label = String.Empty;

			string file;
			if (   sefevent.DefinitionFile              != null
				&& sefevent.DefinitionFile.ResRef       != null
				&& sefevent.DefinitionFile.ResRef.Value != null)
			{
				file = sefevent.DefinitionFile.ResRef.Value;
			}
			else
				file = String.Empty;

			string separator;
			if (label != String.Empty && file != String.Empty)
				separator = " - ";
			else
				separator = String.Empty;

			return "[" + label + separator + file + "]";
		} */

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
	}
}
