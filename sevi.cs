﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
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
using OEIShared.IO.TwoDA;
using OEIShared.NetDisplay;
using OEIShared.OEIMath;
using OEIShared.UI;
using OEIShared.UI.Input;
using OEIShared.Utils;


namespace SpecialEffectsViewer
{
	/// <summary>
	/// The SpecialEffectsViewer window along with all of its mechanics - aka
	/// the UI.
	/// </summary>
	/// <remarks>The ElectronPanel 'Scene' can and shall be in one of three
	/// configurations - <see cref="Scene"/> - and stored in
	/// <see cref="SpecialEffectsViewerPreferences.Scene"/>. The requisite
	/// objects to play effects against shall always be instantiated. There are
	/// however problems with merely keeping the same objects instantiated and
	/// simply changing effects. The double-character config applies a SEFGroup
	/// and *could* keep the same objects except that the SEFGroupManager holds
	/// onto the SEF-objects which results in a memory leak (I haven't seen a
	/// way to dispose them). This is worked around by clearing and
	/// re-instantiating the objects and SEFGroups every time the effect plays.
	/// The single-character and placed-effect-object configs don't use
	/// SEFGroups at least not directly here. Effects are instead assigned to
	/// their NWN2 Instances which are then instantiated into the ElectronPanel
	/// Scene as NetDisplayObjects. The double-character objects similarly
	/// instantiate as NetDisplayObjects but effects are NOT assigned directly
	/// to their NWN2 Instances. So ... depending what operation the user is
	/// invoking it's possible to retain the Instances but recreate the
	/// NetDisplayObjects etc etc etc. Describing code-flow in detail is beyond
	/// my lifespan. Suffice to say there's a fundamental difference between
	/// keeping the scene displayed in its proper configuration and actually
	/// playing the effect in the scene. At present there are only two ways to
	/// play an effect: select it in the effects-list or click the Play button.
	/// But there are many operations that need to reinstantiate the scene such
	/// as changing the scene-config, changing the appearances of creatures in
	/// the scene, showing/hiding ground-tiles, changing the resource-repository,
	/// etc. Note that playing effects gets even more complicated when user
	/// plays selected events of an effect under the double-character Events
	/// menu ...
	/// </remarks>
	sealed partial class sevi
		: Form
	{
		#region enums
		/// <summary>
		/// pick a Scene, any scene
		/// </summary>
		internal enum Scene
		{
			non,				// 0 - not used.
			doublecharacter,	// 1
			singlecharacter,	// 2
			placedeffect		// 3
		}

		/// <summary>
		/// Bitwise flags for use by <see cref="_isListStale"/>.
		/// </summary>
		[Flags]
		enum Stale : byte
		{
			non      = 0x0,
			Module   = 0x1,
			Campaign = 0x2
		}
		#endregion enums


		#region Fields (static)
		const string TITLE     = "Special Effects";
		const string SEF       = "sef";
		const string MODULES   = "modules";
		const string CAMPAIGNS = "campaigns";
		const string OVERRIDE  = "override";

		/// <summary>
		/// The count of standard its in the Events dropdown.
		/// </summary>
		/// <remarks>play, stop, sep, disableall, enableall, sep</remarks>
		const int ItemsReserved = 6;

		/// <summary>
		/// Static pointer to this. Is used by
		/// <see cref="SpecialEffectsViewerPreferences.ValidatePreferences"/>.
		/// </summary>
		internal static sevi that;
		#endregion Fields (static)


		#region Fields
		internal ElectronPanel _panel; // i hate u
		SEFManager _sefer;

		NWN2CreatureInstance _iIdiot1;
		NWN2CreatureInstance _iIdiot2;
		NWN2CreatureInstance _iIdiot;
		NWN2PlacedEffectInstance _iPlacedEffect;

		NetDisplayObject _oIdiot1;
		NetDisplayObject _oIdiot2;
		NetDisplayObject _oIdiot;
		NetDisplayObject _oPlacedEffect;

		MenuItem _itResrepo_all;
		MenuItem _itResrepo_stock;
		MenuItem _itResrepo_module;
		MenuItem _itResrepo_campaign;
		MenuItem _itResrepo_override;

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
		MenuItem _itView_SceneData;
		MenuItem _itView_StayOnTop;
		MenuItem _itView_Refocus;

		string _filtr = String.Empty;

		/// <summary>
		/// A bitwise var that can repopulate the effects-list when the toolset
		/// changes its currently loaded Module or Campaign.
		/// </summary>
		Stale _isListStale;

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

		/// <summary>
		/// Set true to bypass handling events redundantly.
		/// </summary>
		bool _init;
		#endregion Fields


		#region Properties
		SceneData _sceneData;
		/// <summary>
		/// A dialog to print scene-data to.
		/// TODO: It currently has issues.
		/// </summary>
		internal SceneData SceneData
		{
			private get { return _sceneData; }
			set
			{
				_itView_SceneData.Checked = ((_sceneData = value) != null);
			}
		}

		/// <summary>
		/// A convenient getter for the user-selected scene in Preferences.
		/// </summary>
		internal Scene Scenary
		{
			get { return (Scene)SpecialEffectsViewerPreferences.that.Scene; }
		}
		#endregion Properties


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		internal sevi()
		{
			Owner = NWN2ToolsetMainForm.App;
			that = this;

			InitializeComponent();

			NWN2ToolsetMainForm.ModuleChanged                  += OnModuleChanged;
			NWN2CampaignManager.Instance.ActiveCampaignChanged += OnActiveCampaignChanged;

			// leave the options-panel open in the designer but close it here
			sc2_Options.Panel1Collapsed = true;

			// set unicode text on the up/down search-buttons
			bu_SearchD.Text = "\u25bc"; // down triangle
			bu_SearchU.Text = "\u25b2"; // up triangle

			CreateMainMenu();
			CreateElectronPanel();
			PopulateAppearanceDropdowns();

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
			it = Menu.MenuItems.Add("&Resrepo"); // 0

			_itResrepo_all      = it.MenuItems.Add("list &all effects", mi_resrepo_All);
								  it.MenuItems.Add("-");
			_itResrepo_stock    = it.MenuItems.Add("&stock only",       mi_resrepo_Stock);
			_itResrepo_module   = it.MenuItems.Add("&module only",      mi_resrepo_Module);
			_itResrepo_campaign = it.MenuItems.Add("&campaign only",    mi_resrepo_Campaign);
			_itResrepo_override = it.MenuItems.Add("&override only",    mi_resrepo_Override);

			_itResrepo_all     .Shortcut = Shortcut.Ctrl1;
			_itResrepo_stock   .Shortcut = Shortcut.Ctrl2;
			_itResrepo_module  .Shortcut = Shortcut.Ctrl3;
			_itResrepo_campaign.Shortcut = Shortcut.Ctrl4;
			_itResrepo_override.Shortcut = Shortcut.Ctrl5;

// Events ->
			_itEvents = Menu.MenuItems.Add("&Events"); // 1

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
			_itView_SceneData       = it.MenuItems.Add("sce&ne data",           mi_view_Scenedata);
									  it.MenuItems.Add("-");
			_itView_StayOnTop       = it.MenuItems.Add("stay on &top",          mi_view_StayOnTop);
									  it.MenuItems.Add("-");
			_itView_Refocus         = it.MenuItems.Add("&refocus",              mi_view_Refocus);

			_itView_DoubleCharacter.Shortcut = Shortcut.F10;
			_itView_SingleCharacter.Shortcut = Shortcut.F11;
			_itView_PlacedEffect   .Shortcut = Shortcut.F12;
			_itView_Ground         .Shortcut = Shortcut.CtrlG;
			_itView_Options        .Shortcut = Shortcut.F9;
			_itView_ExtendedInfo   .Shortcut = Shortcut.CtrlI;
			_itView_SceneData      .Shortcut = Shortcut.CtrlN;
			_itView_StayOnTop      .Shortcut = Shortcut.CtrlT;
			_itView_Refocus        .Shortcut = Shortcut.CtrlR;

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
		/// Clears the events-list and re/creates Play and Stop.
		/// </summary>
		void CreateBasicEvents()
		{
			_itEvents.MenuItems.Clear();

			_itEvents_Play = _itEvents.MenuItems.Add("&Play", mi_events_Play);
			_itEvents_Stop = _itEvents.MenuItems.Add("&Stop", mi_events_Stop);

			_itEvents_Play.Shortcut = Shortcut.F5;
			_itEvents_Stop.Shortcut = Shortcut.F6;

			_itEvents_Play.Enabled =
			_itEvents_Stop.Enabled = lb_Effects.SelectedIndex != -1;
		}

		/// <summary>
		/// Populates extended its on the Events menu to toggle the events of
		/// the current SEFGroup for a DoubleCharacter scene only.
		/// </summary>
		void CreateExtendedEvents()
		{
			_itEvents.MenuItems.Add("-");

			_itEvents_Enable  = _itEvents.MenuItems.Add("&Enable all events",  mi_events_Event);
			_itEvents_Disable = _itEvents.MenuItems.Add("&Disable all events", mi_events_Event);

			_itEvents_Enable .Shortcut = Shortcut.F7;
			_itEvents_Disable.Shortcut = Shortcut.F8;

			_itEvents.MenuItems.Add("-");

			for (int i = 0; i != SpecialEffect.Sefgroup.Events.Count; ++i)
			{
				var it = _itEvents.MenuItems.Add("event " + i, mi_events_Event);
				it.Tag = i;
				it.Checked = true;
			}
		}


		/// <summary>
		/// Creates the ElectronPanel and subscribes input-handlers to events.
		/// </summary>
		void CreateElectronPanel()
		{
			_panel = new ElectronPanel();

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
		/// Populates the Source and Target dropdowns in the options-panel.
		/// </summary>
		private void PopulateAppearanceDropdowns()
		{
			TwoDAFile appearances = TwoDAManager.Instance.Get("appearance");
			if (appearances != null)
			{
				TwoDAColumn labels = appearances.Columns["LABEL"];
				TwoDAColumn skels  = appearances.Columns["NWN2_Skeleton_File"];
				TwoDAColumn bodies = appearances.Columns["NWN2_Model_Body"]; // etc ...

				IResourceEntry resent;
				string skel, body;

				apr it;
				for (int i = 0; i != labels.Count; ++i)
				{
					skel = skels[i];
					if (!String.IsNullOrEmpty(skel))
					{
						//logger.log();
						//logger.log(i + "= " + "skel= " + skel);
						bool felaskel, felabody, andrskel, andrbody;

						if (skel.Contains("?"))
						{
							resent = NWN2ResourceManager.Instance.GetEntry(new OEIResRef(GetFela(skel)), 4003); // .gr2
							if (resent != null && !(resent is MissingResourceEntry)) // check that the female skeleton-file exists
								felaskel = true;
							else
								felaskel = false;

							//logger.log(". . felaskel= " + felaskel);

							resent = NWN2ResourceManager.Instance.GetEntry(new OEIResRef(GetMale(skel)), 4003); // .gr2 - use the Male skel
							if (resent != null && !(resent is MissingResourceEntry)) // check that the male skeleton-file exists
								andrskel = true;
							else
								andrskel = false;

							//logger.log(". . andrskel= " + andrskel);
						}
						else
						{
							resent = NWN2ResourceManager.Instance.GetEntry(new OEIResRef(skel), 4003); // .gr2
							felaskel = false;
							andrskel = true;

							//logger.log(". felaskel= " + felaskel);
							//logger.log(". andrskel= " + andrskel);
						}

						if (resent != null && !(resent is MissingResourceEntry)) // check that a skeleton-file exists
						{
							body = bodies[i];
							if (!String.IsNullOrEmpty(body))
							{
								//logger.log(i + "= " + "body= " + body);
								if (body.Contains("?"))
								{
									resent = NWN2ResourceManager.Instance.GetEntry(new OEIResRef(GetBody(GetFela(body))), 4000); // .mdb
									if (resent != null && !(resent is MissingResourceEntry)) // check that the female body-file exists
										felabody = true;
									else
										felabody = false;

									//logger.log(". . felabody= " + felabody);

									resent = NWN2ResourceManager.Instance.GetEntry(new OEIResRef(GetBody(GetMale(body))), 4000); // .mdb - use the Male body
									if (resent != null && !(resent is MissingResourceEntry)) // check that the male body-file exists
										andrbody = true;
									else
										andrbody = false;

									//logger.log(". . andrbody= " + andrbody);
								}
								else
								{
									resent = NWN2ResourceManager.Instance.GetEntry(new OEIResRef(GetBody(body)), 4000); // .mdb
									felabody = false;
									andrbody = true;

									//logger.log(". felabody= " + felabody);
									//logger.log(". andrbody= " + andrbody);
								}

								if (resent != null && !(resent is MissingResourceEntry)) // check that a body-file exists
								{
									it = new apr(labels[i], i, (felaskel || felabody), (andrskel || andrbody));
									co_Source.Items.Add(it);
									co_Target.Items.Add(it);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets the male resref.
		/// </summary>
		/// <param name="resref"></param>
		/// <returns></returns>
		private string GetMale(string resref)
		{
			return resref.Replace('?', 'M');
		}

		/// <summary>
		/// Gets the female resref.
		/// </summary>
		/// <param name="resref"></param>
		/// <returns></returns>
		private string GetFela(string resref)
		{
			return resref.Replace('?', 'F');
		}

		/// <summary>
		/// Gets the body resref.
		/// </summary>
		/// <param name="resref"></param>
		/// <returns></returns>
		private string GetBody(string resref)
		{
			return resref + "_cl_body01";
		}
		#endregion cTor


		#region eventhandlers (override)
		/// <summary>
		/// Prepares the ElectronPanel.
		/// </summary>
		/// <param name="e"></param>
		/// <remarks>The camera-receiver goes borky if this is done in the cTor.</remarks>
		protected override void OnLoad(EventArgs e)
		{
			LoadPreferences(); // -> finalize initialization
			InitializeReceiver();

			mi_resrepo_All(null, EventArgs.Empty);
		}

		/// <summary>
		/// Loads preferences.
		/// </summary>
		void LoadPreferences()
		{
			SpecialEffectsViewerPreferences.that.ValidatePreferences();

			int x = SpecialEffectsViewerPreferences.that.x;
			if (x > -1)
			{
				int y = SpecialEffectsViewerPreferences.that.y;
				if (y > -1 && util.checklocation(x,y))
				{
					StartPosition = FormStartPosition.Manual;
					SetDesktopLocation(x,y);
				}
				ClientSize = new Size(SpecialEffectsViewerPreferences.that.w,
									  SpecialEffectsViewerPreferences.that.h);
			}

			if (SpecialEffectsViewerPreferences.that.OptionsPanel)
			{
				_itView_Options.Checked = true;
				sc2_Options.Panel1Collapsed = false;
			}

			sc1_Effects.SplitterDistance = SpecialEffectsViewerPreferences.that.SplitterDistanceEffects;
			sc2_Options.SplitterDistance = SpecialEffectsViewerPreferences.that.SplitterDistanceOptions;
			sc3_Events .SplitterDistance = SpecialEffectsViewerPreferences.that.SplitterDistanceEvents;

			if (!SpecialEffectsViewerPreferences.that.StayOnTop)
			{
				_itView_StayOnTop.Checked = false;
				Owner = null;
			}

			if (SpecialEffectsViewerPreferences.that.Maximized)
				WindowState = FormWindowState.Maximized;

			if (SpecialEffectsViewerPreferences.that.Ground)
			{
				_itView_Ground.Checked =
				cb_Ground.Checked = true;
			}

			_itView_ExtendedInfo.Checked = SpecialEffectsViewerPreferences.that.ExtendedInfo;


			_init = true;

			int id = SpecialEffectsViewerPreferences.that.AppearanceSource; // select Source ->
			for (int i = 0; i != co_Source.Items.Count; ++i)
			if ((co_Source.Items[i] as apr).Id == id)
			{
				var it = co_Source.Items[i] as apr;
				co_Source.SelectedItem = it; // init

				cb_SourceF.Enabled = (it.Fela &&  it.Andr);
				cb_SourceF.Checked = (it.Fela && !it.Andr);
				break;
			}

			if (co_Source.SelectedIndex == -1) // failed ->
			{
				SpecialEffectsViewerPreferences.that.AppearanceSource = 0;
				if (co_Source.Items.Count != 0)
				{
					co_Source.SelectedIndex = 0;
					var it = co_Source.Items[0] as apr;
					cb_SourceF.Enabled = (it.Fela &&  it.Andr);
					cb_SourceF.Checked = (it.Fela && !it.Andr);
				}
				else
				{
					cb_SourceF.Enabled =
					cb_SourceF.Checked = false;
				}
			}

			id = SpecialEffectsViewerPreferences.that.AppearanceTarget; // select Target ->
			for (int i = 0; i != co_Target.Items.Count; ++i)
			if ((co_Target.Items[i] as apr).Id == id)
			{
				var it = co_Target.Items[i] as apr;
				co_Target.SelectedItem = it; // init

				cb_TargetF.Enabled = (it.Fela &&  it.Andr);
				cb_TargetF.Checked = (it.Fela && !it.Andr);
				break;
			}

			if (co_Target.SelectedIndex == -1) // failed ->
			{
				SpecialEffectsViewerPreferences.that.AppearanceTarget = 0;
				if (co_Target.Items.Count != 0)
				{
					co_Target.SelectedIndex = 0;
					var it = co_Target.Items[0] as apr;
					cb_TargetF.Enabled = (it.Fela &&  it.Andr);
					cb_TargetF.Checked = (it.Fela && !it.Andr);
				}
				else
				{
					cb_TargetF.Enabled =
					cb_TargetF.Checked = false;
				}
			}

			_init = false;


			switch (Scenary)
			{
				case Scene.doublecharacter:
					rb_DoubleCharacter.Checked = true;
					break;

				case Scene.singlecharacter:
					rb_SingleCharacter.Checked = true;
					break;

				case Scene.placedeffect:
					rb_PlacedEffect.Checked = true;
					break;
			}
		}

		/// <summary>
		/// Initializes the camera-receiver.
		/// </summary>
		void InitializeReceiver()
		{
			_sefer = _panel.Scene.SpecialEffectsManager;

			if (_panel.Scene.DayNightCycleStages[(int)DayNightStageType.Default] != null)
			{
				_panel.Scene.DayNightCycleStages[(int)DayNightStageType.Default].SunMoonDirection = new Vector3(0.00f,-0.67f,-0.67f);
				_panel.Scene.DayNightCycleStages[(int)DayNightStageType.Default].ShadowIntensity  = 0f;
			}

			if (SpecialEffectsViewerPreferences.that.Ground)
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
		/// Repopulates the Fx-list when activated iff the Module or Campaign
		/// was changed in the toolset.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnActivated(EventArgs e)
		{
			if (_isListStale != Stale.non)
			{
				if (_itResrepo_all.Checked)
				{
					_itResrepo_all.Checked = false;
					mi_resrepo_All(null, EventArgs.Empty);
				}
				else if (_itResrepo_module.Checked)
				{
					if ((_isListStale & Stale.Module) != 0)
					{
						_itResrepo_module.Checked = false;
						mi_resrepo_Module(null, EventArgs.Empty);
					}
				}
				else if (_itResrepo_campaign.Checked)
				{
					if ((_isListStale & Stale.Campaign) != 0)
					{
						_itResrepo_campaign.Checked = false;
						mi_resrepo_Campaign(null, EventArgs.Empty);
					}
				}
				_isListStale = Stale.non;
			}
		}

		/// <summary>
		/// [Esc] closes the plugin.
		/// </summary>
		/// <param name="e"></param>
		/// <remarks>Requires 'KeyPreview' true.</remarks>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			// TODO: universal [Enter] to play effect if valid
			// unless certain controls are focused

			if (e.KeyData == Keys.Escape)
			{
				e.Handled = e.SuppressKeyPress = true;
				Close();
			}
			base.OnKeyDown(e);
		}

		/// <summary>
		/// Stores preferences and unsubscribes from toolset-events.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			ClearScene();

			NWN2ToolsetMainForm.ModuleChanged                  -= OnModuleChanged;
			NWN2CampaignManager.Instance.ActiveCampaignChanged -= OnActiveCampaignChanged;

			if (SceneData != null)
				SceneData.Close();

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
			SpecialEffectsViewerPreferences.that.SplitterDistanceOptions = sc2_Options.SplitterDistance;

			if (!sc2_Options.Panel1Collapsed)
				SpecialEffectsViewerPreferences.that.SplitterDistanceEvents = sc3_Events.SplitterDistance;

			StoreCameraState();


			_panel.Dispose(); // not sure as usual

			while (Menu.MenuItems.Count != 0) // not sure as usual ->
			{
				while (Menu.MenuItems[0].MenuItems.Count != 0)
					   Menu.MenuItems[0].MenuItems[0].Dispose();

				Menu.MenuItems[0].Dispose();
			}
			Menu.Dispose();


			base.OnFormClosing(e);
		}

		/// <summary>
		/// Stores the current camera-state in Preferences.
		/// </summary>
		/// <remarks>Ensure that the ElectronPanel (etc) is valid before call.</remarks>
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
		#endregion eventhandlers (override)


		#region eventhandlers (toolset)
		/// <summary>
		/// Forces the Fx-list to repopulate when the module changes.
		/// </summary>
		/// <param name="oSender"></param>
		/// <param name="eArgs"></param>
		void OnModuleChanged(object oSender, ModuleChangedEventArgs eArgs)
		{
			_isListStale |= Stale.Module;
		}

		/// <summary>
		/// Forces the Fx-list to repopulate when the active campaign changes.
		/// </summary>
		/// <param name="cOldCampaign"></param>
		/// <param name="cNewCampaign"></param>
		void OnActiveCampaignChanged(NWN2Campaign cOldCampaign, NWN2Campaign cNewCampaign)
		{
			_isListStale |= Stale.Campaign;
		}
		#endregion eventhandlers (toolset)


		#region eventhandlers (resrepo)
		/// <summary>
		/// Populates the effects-list with everything it can find.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_resrepo_All(object sender, EventArgs e)
		{
			if (!_itResrepo_all.Checked)
			{
				rb_Scene_click(null, EventArgs.Empty);
				EnableControls(false);

				lb_Effects.BeginUpdate();

				ClearEffectsList();
				_itResrepo_all.Checked = true;

				bool bypassFilter = (_filtr == String.Empty);

				var resrefs = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF));
				foreach (IResourceEntry resref in resrefs)
				{
					if (resref != null && !(resref is MissingResourceEntry)
						&& (bypassFilter || resref.ResRef.Value.ToLower().Contains(_filtr)))
					{
						lb_Effects.Items.Add(resref);
					}
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
		void mi_resrepo_Stock(object sender, EventArgs e)
		{
			if (!_itResrepo_stock.Checked)
			{
				rb_Scene_click(null, EventArgs.Empty);
				EnableControls(false);

				lb_Effects.BeginUpdate();

				ClearEffectsList();
				_itResrepo_stock.Checked = true;

				bool bypassFilter = (_filtr == String.Empty);

				var resrefs = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF));
				foreach (IResourceEntry resref in resrefs)
				{
					if (resref != null && !(resref is MissingResourceEntry))
					{
						string label = resref.Repository.Name.ToLower();
						if (   !label.Contains(MODULES) // fake it ->
							&& !label.Contains(CAMPAIGNS)
							&& !label.Contains(OVERRIDE)
							&& (bypassFilter || resref.ResRef.Value.ToLower().Contains(_filtr)))
						{
							lb_Effects.Items.Add(resref);
						}
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
		void mi_resrepo_Module(object sender, EventArgs e)
		{
			if (!_itResrepo_module.Checked)
			{
				rb_Scene_click(null, EventArgs.Empty);
				EnableControls(false);

				lb_Effects.BeginUpdate();

				ClearEffectsList();
				_itResrepo_module.Checked = true;

				bool bypassFilter = (_filtr == String.Empty);

				var resrefs = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF));
				foreach (IResourceEntry resref in resrefs)
				{
					if (resref != null && !(resref is MissingResourceEntry)
						&& resref.Repository.Name.ToLower().Contains(MODULES) // fake it
						&& (bypassFilter || resref.ResRef.Value.ToLower().Contains(_filtr)))
					{
						lb_Effects.Items.Add(resref);
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
		void mi_resrepo_Campaign(object sender, EventArgs e)
		{
			if (!_itResrepo_campaign.Checked)
			{
				rb_Scene_click(null, EventArgs.Empty);
				EnableControls(false);

				lb_Effects.BeginUpdate();

				ClearEffectsList();
				_itResrepo_campaign.Checked = true;

				bool bypassFilter = (_filtr == String.Empty);

				var resrefs = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF));
				foreach (IResourceEntry resref in resrefs)
				{
					if (resref != null && !(resref is MissingResourceEntry)
						&& resref.Repository.Name.ToLower().Contains(CAMPAIGNS) // fake it
						&& (bypassFilter || resref.ResRef.Value.ToLower().Contains(_filtr)))
					{
						lb_Effects.Items.Add(resref);
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
		void mi_resrepo_Override(object sender, EventArgs e)
		{
			if (!_itResrepo_override.Checked)
			{
				rb_Scene_click(null, EventArgs.Empty);
				EnableControls(false);

				lb_Effects.BeginUpdate();

				ClearEffectsList();
				_itResrepo_override.Checked = true;

				bool bypassFilter = (_filtr == String.Empty);

				var resrefs = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF));
				foreach (IResourceEntry resref in resrefs)
				{
					if (resref != null && !(resref is MissingResourceEntry)
						&& resref.Repository.Name.ToLower().Contains(OVERRIDE) // fake it
						&& (bypassFilter || resref.ResRef.Value.ToLower().Contains(_filtr)))
					{
						lb_Effects.Items.Add(resref);
					}
				}
				lb_Effects.EndUpdate();
			}

			if (!_bypassSearchFocus)
				ActiveControl = tb_Search;
		}

		/// <summary>
		/// Clears the scene, the events-list, and the effects-list, then
		/// unchecks the repo-list so things are ready for the effects in a repo
		/// to be listed.
		/// </summary>
		void ClearEffectsList()
		{
			_init = true;
			lb_Effects.SelectedIndex = -1;
			_init = false;

			lb_Effects.Items.Clear();

			_itResrepo_all     .Checked =
			_itResrepo_stock   .Checked =
			_itResrepo_module  .Checked =
			_itResrepo_campaign.Checked =
			_itResrepo_override.Checked = false;
		}
		#endregion eventhandlers (resrepo)


		#region eventhandlers (events)
		/// <summary>
		/// Plays the currently selected effect.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_events_Play(object sender, EventArgs e)
		{
			Play();

			if (SceneData != null)
				SceneData.ResetDatatext();
		}

		/// <summary>
		/// Plays the current effect.
		/// </summary>
		/// <param name="clearScene"></param>
		void Play(bool clearScene = true)
		{
			if (clearScene) ClearScene();

			if (lb_Effects.SelectedIndex != -1)
			{
				switch (Scenary)
				{
					case Scene.doublecharacter:
						CreateDoubleCharacterObjects();
						ApplySefgroup();
						break;

					case Scene.singlecharacter:
						CreateSingleCharacterObject();
						break;

					case Scene.placedeffect:
						CreatePlacedEffectObject();
						break;
				}
				_sefer.BeginUpdating();
			}
		}

		/// <summary>
		/// Adds a SEFGroup to the SpecialEffectsManager.
		/// </summary>
		/// <remarks>Required by DoubleCharacter scene only.</remarks>
		void ApplySefgroup()
		{
			_sefer.Groups.Clear();
			_sefer.GroupsToRemove.Clear();

			SEFGroup sefgroup;
			if      (SpecialEffect.Solgroup != null) sefgroup = SpecialEffect.Solgroup;
			else if (SpecialEffect.Altgroup != null) sefgroup = SpecialEffect.Altgroup;
			else                                     sefgroup = SpecialEffect.Sefgroup; // SpecialEffect.Sefgroup != null

			sefgroup.FirstObject  = _oIdiot1;
			sefgroup.SecondObject = _oIdiot2;

			_sefer.Groups.Add(sefgroup);

			SpecialEffect.ClearSolgroup(); // 'SpecialEffect.Solgroup' plays once only.
		}

		/// <summary>
		/// Stops the currently selected effect.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_events_Stop(object sender, EventArgs e)
		{
			_sefer.EndUpdating();
		}


		/// <summary>
		/// Alters the current <see cref="SpecialEffect"/> in accord with the
		/// Events menu.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks>Event(s) operations are used by DoubleCharacter scene only.</remarks>
		void mi_events_Event(object sender, EventArgs e)
		{
			_sefer.EndUpdating();


			var it = sender as MenuItem;

			// set the it(s)' check
			if (it == _itEvents_Enable) // enable all events
			{
				SpecialEffect.ClearSubgroups();
				EnableEvents();
			}
			else if (it == _itEvents_Disable) // disable all events
			{
				SpecialEffect.CreateAltgroup();

				for (int i = ItemsReserved; i != _itEvents.MenuItems.Count; ++i)
					_itEvents.MenuItems[i].Checked = false;
			}
			else if (ModifierKeys != Keys.Shift)
			{
				it.Checked = !it.Checked;

				if (AllEventsEnabled())
				{
					SpecialEffect.ClearAltgroup(); // fallback on 'SpecialEffect.Sefgroup'
				}
				else
					SpecialEffect.CreateAltgroup();
			}
			else
				SpecialEffect.CreateSolgroup();


			if (SpecialEffect.Altgroup != null)
			{
				for (int i = SpecialEffect.Altgroup.Events.Count - 1; i != -1; --i)
				if (!_itEvents.MenuItems[i + ItemsReserved].Checked)
					SpecialEffect.Altgroup.Events.RemoveAt(i);
			}
			else if (SpecialEffect.Solgroup != null)
			{
				for (int i = SpecialEffect.Solgroup.Events.Count - 1; i != -1; --i)
				if (i != (int)it.Tag)
					SpecialEffect.Solgroup.Events.RemoveAt(i);

				Play();

				if (SceneData != null)
					SceneData.ResetDatatext();
			}
		}

		/// <summary>
		/// Enables all events on the Events menu.
		/// </summary>
		void EnableEvents()
		{
			for (int i = ItemsReserved; i != _itEvents.MenuItems.Count; ++i)
				_itEvents.MenuItems[i].Checked = true;
		}

		/// <summary>
		/// Checks if all events on the Events menu are enabled.
		/// </summary>
		/// <returns>true if all enabled</returns>
		bool AllEventsEnabled()
		{
			for (int i = ItemsReserved; i != _itEvents.MenuItems.Count; ++i)
			if (!_itEvents.MenuItems[i].Checked)
				return false;

			return true;
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
				rb_Scene_click(null, EventArgs.Empty);
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
				rb_Scene_click(null, EventArgs.Empty);
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
				rb_Scene_click(null, EventArgs.Empty);
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
		/// Toggles extended event-info in the Options panel.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_view_Extended(object sender, EventArgs e)
		{
			SpecialEffectsViewerPreferences.that.ExtendedInfo =
			(_itView_ExtendedInfo.Checked = !_itView_ExtendedInfo.Checked);

			PrintEffectData();
		}

		/// <summary>
		/// Toggles the SceneData dialog.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_view_Scenedata(object sender, EventArgs e)
		{
			if (SceneData == null)
			{
				SceneData = new SceneData(this);
			}
			else
				SceneData.Close();
		}

		/// <summary>
		/// Toggles toolset ownership of the plugin-window.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks>The plugin stays on top of the toolset only when the
		/// toolset is given ownership of the plugin.</remarks>
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

		
		/// <summary>
		/// Focuses the camera onto the center of the area.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_view_Refocus(object sender, EventArgs e)
		{
			if (_panel != null)
			{
				var receiver = _panel.CameraMovementReceiver as ModelViewerInputCameraReceiver; // is null on Load
				var state = (receiver.CameraState as ModelViewerInputCameraReceiverState);
				state.FocusPoint = new Vector3(100f, 100f, 1f);
				receiver.UpdateCamera();
			}
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
		/// Tracks the currently selected effects-list id.
		/// </summary>
		int _efid = -1;

		/// <summary>
		/// Creates the selected effect and plays it.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks>This is NOT "selectedindexchanged" - it fires if the
		/// current selection is (re)clicked also. So use <see cref="_efid"/> to
		/// track the currently selected effects-list id</remarks>
		void lb_Effects_selectedindexchanged(object sender, EventArgs e)
		{
			if (!_init)
			{
				_sefer.EndUpdating();

				if (lb_Effects.SelectedIndex != _efid)
				{
					_efid = lb_Effects.SelectedIndex;

					CreateBasicEvents();

					ClearScene();

					if (lb_Effects.SelectedIndex != -1)
					{
						EnableControls(true);

						SpecialEffect.CreateSefgroup(lb_Effects.SelectedItem as IResourceEntry);

						switch (Scenary)
						{
							case Scene.doublecharacter:
								CreateExtendedEvents();
								break;

							case Scene.singlecharacter:
								CreateSingleCharacterScene(false);
								break;

							case Scene.placedeffect:
								CreatePlacedEffectScene(false);
								break;
						}

						Play(false);
					}
					else
					{
						SpecialEffect.ClearEffect();
						EnableControls(false);
					}

					PrintEffectData();
				}
				else if (lb_Effects.SelectedIndex != -1)
				{
					if (Scenary == Scene.doublecharacter)
					{
						SpecialEffect.ClearAltgroup();
						EnableEvents();
					}
					Play();
				}

				if (SceneData != null)
					SceneData.ResetDatatext();
			}
		}

		/// <summary>
		/// Dis/enables play, stop, copy controls.
		/// </summary>
		/// <param name="enabled"></param>
		void EnableControls(bool enabled)
		{
			_itEvents_Play.Enabled =
			_itEvents_Stop.Enabled =

			bu_Play       .Enabled =
			bu_Stop       .Enabled =
			bu_Copy       .Enabled = enabled;
		}


		/// <summary>
		/// Prints the currently loaded effect-events to the Options panel. Also
		/// adds items to the Events menu iff scene is DoubleCharacter.
		/// </summary>
		void PrintEffectData()
		{
			SetTitle();

			if (SpecialEffect.Sefgroup != null)
			{
				var sb = new StringBuilder();

				sb.Append("[" + SpecialEffect.Sefgroup.Name + "]"                      + util.L);
				sb.Append("pos - " + util.Get3dString(SpecialEffect.Sefgroup.Position) + util.L);
//				sb.Append("1st - " + SpecialEffect.Sefgroup.FirstObject                + util.L);
//				sb.Append("2nd - " + SpecialEffect.Sefgroup.SecondObject               + util.L);
				sb.Append("fog - " + SpecialEffect.Sefgroup.FogMultiplier              + util.L);
				sb.Append("dur - " + SpecialEffect.Sefgroup.HasMaximumDuration         + util.L);
				sb.Append("dur - " + SpecialEffect.Sefgroup.MaximumDuration            + util.L);
				sb.Append(SpecialEffect.Sefgroup.SpecialTargetPosition);

				tb_SefData.Text = sb.ToString();

				sb.Length = 0;
				for (int i = 0; i != SpecialEffect.Sefgroup.Events.Count; ++i)
				{
					// NOTE: a line is 13 px high (+5 pad total)
					if (sb.Length != 0) sb.Append(util.L + util.L);

					sb.Append(EventData.GetEventData(SpecialEffect.Sefgroup.Events[i], i, _itView_ExtendedInfo.Checked));
				}
				tb_EventData.Text = sb.ToString();
			}
			else
			{
				tb_SefData  .Text =
				tb_EventData.Text = String.Empty;
			}
		}

		/// <summary>
		/// Sets text on the titlebar.
		/// </summary>
		void SetTitle()
		{
			string post;
			if (SpecialEffect.Resent != null)
				post = " - " + SpecialEffect.Resent.Repository.Name;
			else
				post = String.Empty;

			Text = TITLE + post;
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
		/// Creates objects in the NetDisplay according to the current
		/// scene-configuration.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks>This is also used to re/create the scene when the resrepo
		/// initializes or is changed.</remarks>
		void rb_Scene_click(object sender, EventArgs e)
		{
			ClearScene();

			CreateBasicEvents();

			if (rb_DoubleCharacter.Checked) // create entire scene ->
			{
				SetSceneType(Scene.doublecharacter);
				CreateDoubleCharacterScene();

				if (lb_Effects.SelectedIndex != -1)
					CreateExtendedEvents();
			}
			else if (rb_SingleCharacter.Checked)
			{
				SetSceneType(Scene.singlecharacter);
				CreateSingleCharacterScene();
			}
			else // rb_PlacedEffect.Checked
			{
				SetSceneType(Scene.placedeffect);
				CreatePlacedEffectScene();
			}

			if (SceneData != null)
				SceneData.ResetDatatext();
		}

		/// <summary>
		/// Toggles scene-checks on the View menu to keep them synchronized with
		/// the radio-buttons on the options-panel.
		/// </summary>
		/// <param name="scene"></param>
		void SetSceneType(Scene scene)
		{
			SpecialEffectsViewerPreferences.that.Scene = (int)scene;

			_itView_DoubleCharacter.Checked = (scene == Scene.doublecharacter);
			_itView_SingleCharacter.Checked = (scene == Scene.singlecharacter);
			_itView_PlacedEffect   .Checked = (scene == Scene.placedeffect);
		}


		/// <summary>
		/// Handles changing Source and Target appearance-types.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void co_Appearance_selectedindexchanged(object sender, EventArgs e)
		{
			if (!_init) // do not fire by LoadPreferences()
			{
				_init = true; // do not double-fire CreateScene()

				var co = sender as ComboBox;
				if (co == co_Source)
				{
					var it = co_Source.SelectedItem as apr;
					SpecialEffectsViewerPreferences.that.AppearanceSource = it.Id;

					cb_SourceF.Enabled = (it.Fela && it.Andr);
					cb_SourceF.Checked = (it.Fela && (cb_SourceF.Checked || !it.Andr));
				}
				else // co == co_Target
				{
					var it = co_Target.SelectedItem as apr;
					SpecialEffectsViewerPreferences.that.AppearanceTarget = it.Id;

					cb_TargetF.Enabled = (it.Fela && it.Andr);
					cb_TargetF.Checked = (it.Fela && (cb_TargetF.Checked || !it.Andr));
				}
				_init = false;

				if (Scenary != Scene.placedeffect)
					CreateScene_appearancechanged(co == co_Source);
			}
		}

		/// <summary>
		/// Handles the checkchanged event for the Fela checkboxes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void cb_Fela_checkedchanged(object sender, EventArgs e)
		{
			if (!_init && Scenary != Scene.placedeffect)
				CreateScene_appearancechanged(sender as CheckBox == cb_SourceF);
		}

		/// <summary>
		/// Instantiates a scene when character-appearance changes.
		/// </summary>
		/// <param name="source">true if the Source appearance or gender changed</param>
		void CreateScene_appearancechanged(bool source)
		{
			switch (Scenary)
			{
				case Scene.doublecharacter:
					ClearScene();
					CreateDoubleCharacterScene();
					break;

				case Scene.singlecharacter:
					if (source)
					{
						ClearScene();
						CreateSingleCharacterScene();
					}
					break;
			}

			if (SceneData != null)
				SceneData.ResetDatatext();
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
			_itView_Ground.Checked = cb_Ground.Checked;

			ClearScene();

			StoreCameraState();

			_panel.Dispose();
			CreateElectronPanel();
			InitializeReceiver();

			switch (Scenary) // create entire scene ->
			{
				case Scene.doublecharacter:
					CreateDoubleCharacterScene();
					break;

				case Scene.singlecharacter:
					CreateSingleCharacterScene();
					break;

				case Scene.placedeffect:
					CreatePlacedEffectScene();
					break;
			}

			if (SceneData != null)
				SceneData.ResetDatatext();
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
			int id = Search.SearchEffects(lb_Effects, tb_Search.Text, sender == bu_SearchD);
			if (id != -1)
				lb_Effects.SelectedIndex = id;
		}

		/// <summary>
		/// Applies the search-filter to the effects list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void cb_Filter_checkedchanged(object sender, EventArgs e)
		{
			if (cb_Filter.Checked)
			{
				if ((_filtr = tb_Search.Text.ToLower()) == String.Empty)
				{
					cb_Filter.Checked = false;	// recurse
					return;						// but don't run twice.
				}
			}
			else
				_filtr = String.Empty;

			_bypassSearchFocus = true;
			if (_itResrepo_all.Checked)
			{
				_itResrepo_all.Checked = false;
				mi_resrepo_All(null, EventArgs.Empty);
			}
			else if (_itResrepo_stock.Checked)
			{
				_itResrepo_stock.Checked = false;
				mi_resrepo_Stock(null, EventArgs.Empty);
			}
			else if (_itResrepo_module.Checked)
			{
				_itResrepo_module.Checked = false;
				mi_resrepo_Module(null, EventArgs.Empty);
			}
			else if (_itResrepo_campaign.Checked)
			{
				_itResrepo_campaign.Checked = false;
				mi_resrepo_Campaign(null, EventArgs.Empty);
			}
			else // _itFxList_override.Checked
			{
				_itResrepo_override.Checked = false;
				mi_resrepo_Override(null, EventArgs.Empty);
			}
			_bypassSearchFocus = false;
		}

		/// <summary>
		/// [Enter] toggles the search-filter.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks>Spacebar also toggles the filter on/off.</remarks>
		void cb_Filter_keydown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
			{
				e.Handled = e.SuppressKeyPress = true;
				cb_Filter.Checked = !cb_Filter.Checked;
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
		/// Deters the current state of the cursor.
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
		/// Deters the current state of the cursor.
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
		#endregion eventhandlers (controls)


		#region Methods
		/// <summary>
		/// Clears the scene. Stops playing the effect, clears the SEFGroups,
		/// and removes all objects from the NetDisplay scene.
		/// </summary>
		void ClearScene()
		{
			if (_sefer != null) // NetDisplay is null on launch
			{
				_sefer.EndUpdating();
				_sefer.Groups.Clear();
				_sefer.GroupsToRemove.Clear();

				for (int i = _panel.Scene.Objects.Count - 1; i != -1; --i)
					NWN2NetDisplayManager.Instance.RemoveObject(_panel.Scene.Objects[i]);
			}
		}


		/// <summary>
		/// Creates double-character Instances w/out effect.
		/// </summary>
		/// <remarks>Idiots can stand around looking dorky. Only
		/// double-characters play a SEFGroup. A single-character could play a
		/// sefgroup I suppose, but prefer to use single-character to assign the
		/// effect to 'AppearanceSEF'.</remarks>
		void CreateDoubleCharacterScene()
		{
			_iIdiot1 = new NWN2CreatureInstance();
			_iIdiot2 = new NWN2CreatureInstance();

			int id;

			if (co_Source.SelectedIndex != -1)
				id = (co_Source.SelectedItem as apr).Id;
			else
				id = 5; // half-orc source

			_iIdiot1.AppearanceType.Row = id;
			_iIdiot1.AppearanceSEF = null;

			if (cb_SourceF.Checked)
				_iIdiot1.Gender = CreatureGender.Female;
			else
				_iIdiot1.Gender = CreatureGender.Male;


			if (co_Target.SelectedIndex != -1)
				id = (co_Target.SelectedItem as apr).Id;
			else
				id = 2; // gnome target

			_iIdiot2.AppearanceType.Row = id;
			_iIdiot2.AppearanceSEF = null;

			if (cb_TargetF.Checked)
				_iIdiot2.Gender = CreatureGender.Female;
			else
				_iIdiot2.Gender = CreatureGender.Male;


			CreateDoubleCharacterObjects();
		}

		/// <summary>
		/// Creates double-character NetDisplayObjects.
		/// </summary>
		void CreateDoubleCharacterObjects()
		{
			// the Idiots are class-vars so that a SEFGroup can be applied later
			_oIdiot1 = NWN2NetDisplayManager.Instance.CreateNDOForInstance(_iIdiot1, _panel.Scene, 0);
			_oIdiot2 = NWN2NetDisplayManager.Instance.CreateNDOForInstance(_iIdiot2, _panel.Scene, 0);

			_oIdiot1.Position = new Vector3(103f,100f,0f);
			_oIdiot2.Position = new Vector3( 97f,100f,0f);
			_oIdiot1.Orientation = RHQuaternion.RotationZ(-util.pi_2);
			_oIdiot2.Orientation = RHQuaternion.RotationZ( util.pi_2);

			var col1 = new NetDisplayObjectCollection(_oIdiot1);
			var col2 = new NetDisplayObjectCollection(_oIdiot2);
			NWN2NetDisplayManager.Instance.MoveObjects(  col1, ChangeType.Absolute, false, _oIdiot1.Position);
			NWN2NetDisplayManager.Instance.MoveObjects(  col2, ChangeType.Absolute, false, _oIdiot2.Position);
			NWN2NetDisplayManager.Instance.RotateObjects(col1, ChangeType.Absolute,        _oIdiot1.Orientation);
			NWN2NetDisplayManager.Instance.RotateObjects(col2, ChangeType.Absolute,        _oIdiot2.Orientation);
		}


		/// <summary>
		/// Creates a single-character Instance w/ effect.
		/// </summary>
		/// <param name="createObject">true to also create the NetDisplayObject</param>
		/// <remarks>Idiot must be recreated from scratch to apply an effect.</remarks>
		void CreateSingleCharacterScene(bool createObject = true)
		{
			_iIdiot = new NWN2CreatureInstance();

			int id;
			if (co_Source.SelectedIndex != -1)
				id = (co_Source.SelectedItem as apr).Id;
			else
				id = 4; // half-elf character

			_iIdiot.AppearanceType.Row = id;
			_iIdiot.AppearanceSEF = SpecialEffect.Resent;

			if (cb_SourceF.Checked)
				_iIdiot.Gender = CreatureGender.Female;
			else
				_iIdiot.Gender = CreatureGender.Male;

			if (createObject)
				CreateSingleCharacterObject();
		}

		/// <summary>
		/// Creates a single-character NetDisplayObject.
		/// </summary>
		void CreateSingleCharacterObject()
		{
			_oIdiot = NWN2NetDisplayManager.Instance.CreateNDOForInstance(_iIdiot, _panel.Scene, 0);

			_oIdiot.Position = new Vector3(100f,100f,0f);
			NWN2NetDisplayManager.Instance.MoveObjects(new NetDisplayObjectCollection(_oIdiot),
													   ChangeType.Absolute,
													   false,
													   _oIdiot.Position);
		}


		/// <summary>
		/// Creates a placed-effect Instance w/ effect.
		/// </summary>
		/// <param name="createObject">true to also create the NetDisplayObject</param>
		/// <remarks>Object must be recreated from scratch to apply an effect.</remarks>
		void CreatePlacedEffectScene(bool createObject = true)
		{
			_iPlacedEffect = new NWN2PlacedEffectInstance();
			_iPlacedEffect.Effect = SpecialEffect.Resent;
			_iPlacedEffect.Active = true;

			if (createObject)
				CreatePlacedEffectObject();
		}

		/// <summary>
		/// Creates a placed-effect NetDisplayObject.
		/// </summary>
		void CreatePlacedEffectObject()
		{
			_oPlacedEffect = NWN2NetDisplayManager.Instance.CreateNDOForInstance(_iPlacedEffect, _panel.Scene, 0);

			_oPlacedEffect.Position = new Vector3(100f,100f,0f);
			NWN2NetDisplayManager.Instance.MoveObjects(new NetDisplayObjectCollection(_oPlacedEffect),
													   ChangeType.Absolute,
													   false,
													   _oPlacedEffect.Position);
		}
		#endregion Methods
	}
}
