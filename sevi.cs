using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
#if !__MonoCS__
using System.Runtime.InteropServices;
#endif
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


namespace specialeffectsviewer
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
	/// and *could* keep the same objects except that the SEFManager holds onto
	/// the SEF-objects which results in a memory leak (I haven't seen a way to
	/// dispose them). This is worked around by clearing and re-instantiating
	/// the objects and SEFGroups every time the effect plays. The
	/// single-character and placed-effect-object configs don't use SEFGroups at
	/// least not directly here. Effects are instead assigned to their NWN2
	/// Instances which are then instantiated into the ElectronPanel Scene as
	/// NetDisplayObjects. The double-character objects similarly instantiate as
	/// NetDisplayObjects but effects are NOT assigned directly to their NWN2
	/// Instances. So ... depending what operation the user is invoking it's
	/// possible to retain the Instances but recreate the NetDisplayObjects etc
	/// etc etc. Describing code-flow in detail is beyond my lifespan. Suffice
	/// to say there's a fundamental difference between keeping the scene
	/// displayed in its user-chosen configuration and actually playing the
	/// effect in the scene. At present there are only two ways to play an
	/// effect: select it in the effects-list or click the Play button. But
	/// there are many operations that need to reinstantiate the scene such as
	/// changing the scene-config, changing the appearances of creatures in the
	/// scene, showing/hiding ground-tiles, changing the resource-repository,
	/// etc. Note that playing effects gets even more complicated when user
	/// plays selected events of an effect under the double-character Events
	/// menu ...
	/// </remarks>
	sealed partial class sevi
		: Form
#if !__MonoCS__
		, IMessageFilter
#endif
	{
#if !__MonoCS__
		#region P/Invoke declarations
		[DllImport("user32.dll")]
		static extern IntPtr WindowFromPoint(Point pt);

		[DllImport("user32.dll")]
		static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
		#endregion P/Invoke declarations
#endif

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

		/// <summary>
		/// A bitwise var that can repopulate the effects-list when the toolset
		/// changes its currently loaded Module or Campaign.
		/// </summary>
		Stale _isListStale;

		/// <summary>
		/// Set true to bypass handling events redundantly.
		/// </summary>
		bool _init;

		/// <summary>
		/// Tracks the currently selected effects-list id.
		/// </summary>
		int _efid = -1;

		/// <summary>
		/// Tracks the label of the currently selected effect for search after
		/// a resrepo loads. The value will be stored in
		/// <see cref="SpecialEffectsViewerPreferences.LastEffect"/> when the
		/// plugin closes if appropriate.
		/// </summary>
		string _lastEffectLabel;

		/// <summary>
		/// Do not play an effect if it gets reselected after a resrepo changes.
		/// </summary>
		bool _bypassPlay;

		/// <summary>
		/// A string to filter the effects-list against.
		/// </summary>
		string _filtr = String.Empty;

		/// <summary>
		/// If the filter-button is checked when there is no text in the
		/// search-textbox AND the filter itself is already blank do not run
		/// the recursion that sets the filter blank - just toggle the
		/// filter-button off pronto.
		/// </summary>
		bool _bypassFiltrRecursion;

		/// <summary>
		/// The search-textbox is usually focused after the effects-list is
		/// populated; but don't do that if the filter-button is clicked. That
		/// is keep the filter-button focused so the list can be toggled/
		/// re-toggled.
		/// </summary>
		bool _bypassSearchFocus;

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
		/// Fullpath to the helpfile if it exists.
		/// </summary>
		string _pfe_helpfile;
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
		internal Scene Scenari
		{
			get { return (Scene)SpecialEffectsViewerPreferences.that.Scene; }
		}
		#endregion Properties

#if !__MonoCS__
		#region IMessageFilter
		/// <summary>
		/// Sends mousewheel messages to the control that the mouse-cursor is
		/// hovering over.
		/// </summary>
		/// <param name="m">the message</param>
		/// <returns>true if a mousewheel message was handled successfully</returns>
		/// <remarks>https://stackoverflow.com/questions/4769854/windows-forms-capturing-mousewheel#4769961</remarks>
		public bool PreFilterMessage(ref Message m)
		{
			if (m.Msg == 0x20a)
			{
				// WM_MOUSEWHEEL - find the control at screen position m.LParam
				var pos = new Point(m.LParam.ToInt32());

				IntPtr hWnd = WindowFromPoint(pos);
				if (hWnd != IntPtr.Zero && hWnd != m.HWnd && Control.FromHandle(hWnd) != null)
				{
					SendMessage(hWnd, m.Msg, m.WParam, m.LParam);
					return true;
				}
			}
			return false;
		}
		#endregion IMessageFilter
#endif

		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		internal sevi()
		{
			// Re. Logger - the logfile will preserve and append text if
			// create() is NOT called. create() is required only to overwrite
			// a previous logfile.
			//logger.create();

			Owner = NWN2ToolsetMainForm.App;
			that = this;

			InitializeComponent();
#if !__MonoCS__
			Application.AddMessageFilter(this);
#endif
			NWN2ToolsetMainForm.ModuleChanged                  += OnModuleChanged;
			NWN2CampaignManager.Instance.ActiveCampaignChanged += OnActiveCampaignChanged;

			// leave the options-panel open in the designer but close it here
			sc2_Options.Panel1Collapsed = true;

			// set unicode text on the up/down search-buttons
			bu_SearchUp.Text = "\u25b2"; // up triangle
			bu_SearchDn.Text = "\u25bc"; // down triangle

			CreateMainMenu();
			CreateElectronPanel();
			PopulateAppearanceDropdowns();
			LoadPreferences();

			Show();
		}

		/// <summary>
		/// Creates the plugin's MainMenu.
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

			_panel.Dock        = DockStyle.Fill;
			_panel.BorderStyle = BorderStyle.FixedSingle;

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
				int width = co_Source.DropDownWidth, widthtest;

				TwoDAColumn strrefs = appearances.Columns["STRING_REF"];
				TwoDAColumn skels   = appearances.Columns["NWN2_Skeleton_File"];
				TwoDAColumn bodies  = appearances.Columns["NWN2_Model_Body"]; // etc ...

				IResourceEntry resent;
				string skel, body;

				apr it;
				for (int i = 0; i != strrefs.Count; ++i)
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
									// 'strrefs[]' gets the Dialog.Tlk string auto. w/ fallthrough to "LABEL".
									it = new apr(strrefs[i], i, (felaskel || felabody), (andrskel || andrbody));

									if ((widthtest = TextRenderer.MeasureText(it.ToString(), co_Source.Font).Width) > width)
										width = widthtest;

									co_Source.Items.Add(it);
									co_Target.Items.Add(it);
								}
							}
						}
					}
				}

				if (width != co_Source.DropDownWidth)
				{
					co_Source.DropDownWidth =
					co_Target.DropDownWidth = width + 15;
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

		/// <summary>
		/// Loads preferences.
		/// </summary>
		void LoadPreferences()
		{
			SpecialEffectsViewerPreferences prefs = SpecialEffectsViewerPreferences.that;

			prefs.ValidatePreferences();

			if (prefs.x != Int32.MinValue)
			{
				if (prefs.y != Int32.MinValue && util.checklocation(prefs.x, prefs.y))
				{
					StartPosition = FormStartPosition.Manual;
					SetDesktopLocation(prefs.x, prefs.y);
				}
				ClientSize = new Size(prefs.w, prefs.h);
			}

			if (prefs.OptionsPanel)
			{
				_itView_Options.Checked = true;
				sc2_Options.Panel1Collapsed = false;
			}

			sc1_Effects.SplitterDistance = prefs.SplitterDistanceEffects;
			sc2_Options.SplitterDistance = prefs.SplitterDistanceOptions;
			sc3_Events .SplitterDistance = prefs.SplitterDistanceEvents;

			if (!prefs.StayOnTop)
			{
				_itView_StayOnTop.Checked = false;
				Owner = null;
			}

			if (prefs.Maximized)
				WindowState = FormWindowState.Maximized;

			if (prefs.Ground)
			{
				_itView_Ground.Checked =
				cb_Ground.Checked = true;
			}

			_itView_ExtendedInfo.Checked = prefs.ExtendedInfo;


			_init = true;

			int id = prefs.AppearanceSource; // select Source ->
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
				prefs.AppearanceSource = 0;
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

			id = prefs.AppearanceTarget; // select Target ->
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
				prefs.AppearanceTarget = 0;
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

			tb_Dist.Text = prefs.DoubleCharacterDistance.ToString();

			_init = false;


			switch (Scenari)
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

			_lastEffectLabel = prefs.LastEffect;

			if (prefs.search0 != String.Empty) co_Search.Items.Add(prefs.search0);
			if (prefs.search1 != String.Empty) co_Search.Items.Add(prefs.search1);
			if (prefs.search2 != String.Empty) co_Search.Items.Add(prefs.search2);
			if (prefs.search3 != String.Empty) co_Search.Items.Add(prefs.search3);
			if (prefs.search4 != String.Empty) co_Search.Items.Add(prefs.search4);
			if (prefs.search5 != String.Empty) co_Search.Items.Add(prefs.search5);
			if (prefs.search6 != String.Empty) co_Search.Items.Add(prefs.search6);
			if (prefs.search7 != String.Empty) co_Search.Items.Add(prefs.search7);
		}
		#endregion cTor


		#region eventhandlers (override)
		/// <summary>
		/// Prepares the ElectronPanel, and loads a resrepo to instantiate the
		/// initial scene.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			InitializeReceiver();

			mi_resrepo_All(null, EventArgs.Empty); // -> finalize initialization

			rb_Scene_click(null, EventArgs.Empty); // -> create a scene
		}

		/// <summary>
		/// Initializes the camera-receiver.
		/// </summary>
		/// <remarks>The camera-receiver goes borky if this is done in the cTor.</remarks>
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
		/// Stops effect when the plugin is minimized.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnResize(EventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
				mi_events_Stop(null, EventArgs.Empty);

			base.OnResize(e);
		}


		/// <summary>
		/// Handles keyboard events at the Form level.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="keyData"></param>
		/// <returns>true if the event gets handled</returns>
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Escape:
					Close();
					return true;

				case Keys.Enter:
					if (co_Search.Focused)
					{
						bu_Search_click(bu_SearchDn, EventArgs.Empty);
						lb_Effects.Select();
						return true;
					}

					if (cb_Filter.Focused)
					{
						cb_Filter.Checked = !cb_Filter.Checked;
						return true;
					}

					if (tb_Dist.Focused)
					{
						bu_SetDist_click(null, EventArgs.Empty);
						return true;
					}

					if (lb_Effects.SelectedIndex != -1 && isPlayControlFocused())
					{
						mi_events_Play(null, EventArgs.Empty);
						return true;
					}
					break;

				case Keys.Enter | Keys.Shift:
					if (co_Search.Focused)
					{
						bu_Search_click(bu_SearchUp, EventArgs.Empty);
						lb_Effects.Select();
						return true;
					}
					break;

				case Keys.F3:
					bu_Search_click(bu_SearchDn, EventArgs.Empty);
					return true;

				case Keys.F3 | Keys.Shift:
					bu_Search_click(bu_SearchUp, EventArgs.Empty);
					return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		/// <summary>
		/// The listed controls shall not attempt to play an effect on keydown
		/// [Enter].
		/// </summary>
		/// <returns>true if a valid play-control has current focus or no
		/// control has focus (which I don't believe is possible)</returns>
		/// <remarks>The events for these buttons could/should probably be
		/// handled by <see cref="ProcessCmdKey"/> directly.</remarks>
		bool isPlayControlFocused()
		{
			return !bu_SearchUp.Focused
				&& !bu_SearchDn.Focused
				&& !bu_Play    .Focused
				&& !bu_Stop    .Focused
				&& !bu_Copy    .Focused
				&& !bu_SetDist .Focused;
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

			SpecialEffectsViewerPreferences prefs = SpecialEffectsViewerPreferences.that;

			if (WindowState == FormWindowState.Maximized)
				prefs.Maximized = true;
			else
				prefs.Maximized = false;

			WindowState = FormWindowState.Normal;

			prefs.x = DesktopLocation.X;
			prefs.y = DesktopLocation.Y;
			prefs.w = ClientSize.Width;
			prefs.h = ClientSize.Height;

			prefs.SplitterDistanceEffects = sc1_Effects.SplitterDistance;
			prefs.SplitterDistanceOptions = sc2_Options.SplitterDistance;

			if (!sc2_Options.Panel1Collapsed)
				prefs.SplitterDistanceEvents = sc3_Events.SplitterDistance;

			if (prefs.LastEffect != String.Empty
				&& _lastEffectLabel.Trim() != String.Empty)
			{
				prefs.LastEffect = _lastEffectLabel;
			}

			for (int i = 0; i != co_Search.Items.Count; ++i)
			{
				string search = co_Search.Items[i].ToString();
				switch (i)
				{
					case 0: prefs.search0 = search; break;
					case 1: prefs.search1 = search; break;
					case 2: prefs.search2 = search; break;
					case 3: prefs.search3 = search; break;
					case 4: prefs.search4 = search; break;
					case 5: prefs.search5 = search; break;
					case 6: prefs.search6 = search; break;
					case 7: prefs.search7 = search; break;
				}
			}

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
			SpecialEffectsViewerPreferences prefs = SpecialEffectsViewerPreferences.that;

			var receiver = (_panel.CameraMovementReceiver as ModelViewerInputCameraReceiver);
			var state = (receiver.CameraState as ModelViewerInputCameraReceiverState);

			prefs.FocusPhi     = state.FocusPhi;
			prefs.FocusTheta   = state.FocusTheta;
			prefs.Distance     = state.Distance;
			prefs.FocusPoint_x = state.FocusPoint.X;
			prefs.FocusPoint_y = state.FocusPoint.Y;
			prefs.FocusPoint_z = state.FocusPoint.Z;
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

				SearchLastEffectLabel();
			}

			if (!_bypassSearchFocus)
				ActiveControl = co_Search;
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

				SearchLastEffectLabel();
			}

			if (!_bypassSearchFocus)
				ActiveControl = co_Search;
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

				SearchLastEffectLabel();
			}

			if (!_bypassSearchFocus)
				ActiveControl = co_Search;
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

				SearchLastEffectLabel();
			}

			if (!_bypassSearchFocus)
				ActiveControl = co_Search;
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

				SearchLastEffectLabel();
			}

			if (!_bypassSearchFocus)
				ActiveControl = co_Search;
		}

		/// <summary>
		/// Stops the effect, disables controls, resets the effects-list id and
		/// clears the list, then unchecks the all repo-its so that things are
		/// ready for the effects of another repo to be listed.
		/// </summary>
		/// <remarks>Stop the effect but leave the NWN2 Instances and NetDisplayObjects
		/// intact (w/ or w/out their current effects). Rely on user selecting
		/// an effect in the list - lb_Effects_selectedindexchanged() - to clear
		/// and recreate the scene with another effect. Note there are several
		/// other ways to create or recreate the Instances and Objects: rb_Scene_click()
		/// cb_Ground_click() CreateScene_appearancechanged().
		/// </remarks>
		void ClearEffectsList()
		{
			Stop();

			EnableControls(false);

			SpecialEffect.ClearEffect();

			_init = true;
			lb_Effects.SelectedIndex = _efid = -1;
			_init = false;

			lb_Effects.Items.Clear();

			_itResrepo_all     .Checked =
			_itResrepo_stock   .Checked =
			_itResrepo_module  .Checked =
			_itResrepo_campaign.Checked =
			_itResrepo_override.Checked = false;
		}

		/// <summary>
		/// Searches the effects-list for the previously selected effect after a
		/// resrepo loads.
		/// </summary>
		void SearchLastEffectLabel()
		{
			int id = Search.SearchEffects(lb_Effects, _lastEffectLabel, true, true);
			if (id != -1)
			{
				_bypassPlay = true;
				lb_Effects.SelectedIndex = id;
				_bypassPlay = false;
			}

			PrintEffectData();

			if (SceneData != null)
				SceneData.ResetDatatext();
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
		/// Instantiates NetDisplayObjects and plays the current effect.
		/// </summary>
		/// <param name="effectChanged">true to create extended events and
		/// SEFGroup if double-character or recreate NWN2 Instances if
		/// single-character or placed-effect</param>
		void Play(bool effectChanged = false)
		{
			ClearScene(); // clear and recreate all NetDisplay objects (so they don't leak each time they're played)

			switch (Scenari)
			{
				case Scene.doublecharacter:					// NWN2 Instances shall be valid
					if (effectChanged)
					{
						CreateExtendedEvents();
					}
					CreateDoubleCharacterObjects();			// instantiate NetDisplayObjects
					AddSefgroup();							// add the SEFGroup to the SEFManager
					break;

				case Scene.singlecharacter:
					if (effectChanged)
					{
						CreateSingleCharacterScene(false);	// recreate NWN2 Instance w/ effect
					}
					CreateSingleCharacterObject();			// instantiate NetDisplayObject
					break;

				case Scene.placedeffect:
					if (effectChanged)
					{
						CreatePlacedEffectScene(false);		// recreate NWN2 Instance w/ effect
					}
					CreatePlacedEffectObject();				// instantiate NetDisplayObject
					break;
			}
			_sefer.BeginUpdating();
		}

		/// <summary>
		/// Adds a SEFGroup to the SpecialEffectsManager.
		/// </summary>
		/// <remarks>Required by DoubleCharacter scene only.</remarks>
		void AddSefgroup()
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

			SpecialEffect.Solgroup = null; // 'SpecialEffect.Solgroup' plays once only.
		}

		/// <summary>
		/// Stops the currently selected effect.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mi_events_Stop(object sender, EventArgs e)
		{
			Stop();

			if (SceneData != null)
				SceneData.ResetDatatext();
		}

		/// <summary>
		/// Stops rendering the current effect including audio.
		/// </summary>
		/// <remarks>(ISEFEvent as SEFSound).Deactivate() does not deactivate
		/// non-looping sounds. To stop their audio, flag each as looping, then
		/// call _sefer.EndUpdating(). Lastly deflag their 'SoundLoops' booleans
		/// in case user replays the effect.</remarks>
		void Stop()
		{
			var reverts = new List<SEFSound>();

			SEFGroup sefgroup; SEFSound sefsound;

			for (int i = 0; i != _sefer.Groups.Count; ++i)
			{
				sefgroup = _sefer.Groups[i];
				for (int j = 0; j != sefgroup.Events.Count; ++j)
				{
					if ((sefsound = sefgroup.Events[j] as SEFSound) != null
						&& !sefsound.SoundLoops)
					{
						sefsound.SoundLoops = true;
						reverts.Add(sefsound);
					}
				}
			}

			_sefer.EndUpdating();

			foreach (SEFSound revert in reverts)	// WARNING: Don't try bypassing this even if Groups are going to
				revert.SoundLoops = false;			// be destroyed, since things are getting so convoluted it'll ef.
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
			Stop();

			var it = sender as MenuItem;
			if (it == _itEvents_Enable) // enable all events
			{
				SpecialEffect.Altgroup = null;
				EnableEvents();
			}
			else if (it == _itEvents_Disable) // disable all events
			{
				SpecialEffect.CreateAltgroup();
				EnableEvents(false);
			}
			else if (ModifierKeys != Keys.Shift)
			{
				it.Checked = !it.Checked;

				if (AllEventsEnabled())
					SpecialEffect.Altgroup = null; // fallback on 'SpecialEffect.Sefgroup'
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
		/// Dis/enables all events on the Events menu.
		/// </summary>
		/// <param name="enabled">true to enable</param>
		void EnableEvents(bool enabled = true)
		{
			for (int i = ItemsReserved; i != _itEvents.MenuItems.Count; ++i)
				_itEvents.MenuItems[i].Checked = enabled;
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

			PrintEffectData(false);
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
			var receiver = _panel.CameraMovementReceiver as ModelViewerInputCameraReceiver;
			var state = (receiver.CameraState as ModelViewerInputCameraReceiverState);
			state.FocusPoint = new Vector3(100f, 100f, 1f);
			receiver.UpdateCamera();
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


		#region eventhandlers (effects-list)
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
			if (!_init && lb_Effects.SelectedIndex != -1)
			{
				// NOTE: The id shall never be -1 here. The only way that id can
				// be set to -1 is by initialization or resrepo-load - incl/
				// filter - but those are conditioned by '_init'. -1 would need
				// to be handled here if that behavior is altered ...
				//
				// however, since selectedindexchanged fires whenever the list
				// is clicked - EVEN WHEN NO ITEM IS SELECTED OR CLICKED ON -
				// -1 needs to be bypassed.

				if (lb_Effects.SelectedIndex != _efid)
				{
					_efid = lb_Effects.SelectedIndex;

					CreateBasicEvents();

					_lastEffectLabel = lb_Effects.SelectedItem.ToString();

					EnableControls(true);
					SpecialEffect.CreateSefgroup(lb_Effects.SelectedItem as IResourceEntry);

					if (!_bypassPlay)
					{
						Play(true);
					}
					else if (Scenari == Scene.doublecharacter)
						CreateExtendedEvents();

					PrintEffectData();
				}
				else
				{
					if (Scenari == Scene.doublecharacter)
					{
						EnableEvents(); // ensure all events get re-enabled when the effects-list is clicked
						SpecialEffect.Altgroup = null;
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
		#endregion eventhandlers (effects-list)


		#region eventhandlers (scene-config)
		/// <summary>
		/// Creates objects in the Electron Panel according to the current
		/// scene-configuration.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks>This is also used to re/create the scene when any resrepo
		/// initializes.</remarks>
		void rb_Scene_click(object sender, EventArgs e)
		{
			var rb = sender as RadioButton;

			if (sender == null
				|| (rb == rb_DoubleCharacter && Scenari != Scene.doublecharacter)
				|| (rb == rb_SingleCharacter && Scenari != Scene.singlecharacter)
				|| (rb == rb_PlacedEffect    && Scenari != Scene.placedeffect))
			{
				SetSceneType();

				ClearScene();

				CreateBasicEvents();

				if (rb_DoubleCharacter.Checked) // create entire scene ->
				{
					SpecialEffectsViewerPreferences.that.Scene = (int)Scene.doublecharacter;
					CreateDoubleCharacterScene();

					if (lb_Effects.SelectedIndex != -1)
						CreateExtendedEvents();
				}
				else if (rb_SingleCharacter.Checked)
				{
					SpecialEffectsViewerPreferences.that.Scene = (int)Scene.singlecharacter;
					CreateSingleCharacterScene();
				}
				else // rb_PlacedEffect.Checked
				{
					SpecialEffectsViewerPreferences.that.Scene = (int)Scene.placedeffect;
					CreatePlacedEffectScene();
				}

				if (SceneData != null)
					SceneData.ResetDatatext();
			}
		}

		/// <summary>
		/// Toggles scene-checks on the View menu to keep them synchronized with
		/// the radio-buttons on the options-panel.
		/// </summary>
		void SetSceneType()
		{
			_itView_DoubleCharacter.Checked = rb_DoubleCharacter.Checked;
			_itView_SingleCharacter.Checked = rb_SingleCharacter.Checked;
			_itView_PlacedEffect   .Checked = rb_PlacedEffect   .Checked;
		}


		/// <summary>
		/// Ensures that DoubleCharacter distance is within limits.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void tb_Distance_textchanged(object sender, EventArgs e)
		{
			if (!_init)
			{
				int result;
				if (!Int32.TryParse(tb_Dist.Text, out result)
					|| result < SpecialEffectsViewerPreferences.DIST_Min
					|| result > SpecialEffectsViewerPreferences.DIST_Max)
				{
					tb_Dist.Text = SpecialEffectsViewerPreferences.that.DoubleCharacterDistance.ToString(); // recurse

					tb_Dist.SelectionLength = 0;
					tb_Dist.SelectionStart  = tb_Dist.Text.Length;
				}
				else
					bu_SetDist.Enabled = (Int32.Parse(tb_Dist.Text) != SpecialEffectsViewerPreferences.that.DoubleCharacterDistance);
			}
		}

		/// <summary>
		/// Applies DoubleCharacter distance to the scene.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks>Also handles [Enter] on <see cref="tb_Dist"/>.</remarks>
		void bu_SetDist_click(object sender, EventArgs e)
		{
			ActiveControl = co_Search;

			if (bu_SetDist.Enabled)
			{
				bu_SetDist.Enabled = false;

				SpecialEffectsViewerPreferences.that.DoubleCharacterDistance = Int32.Parse(tb_Dist.Text);

				if (Scenari == Scene.doublecharacter)
				{
					mi_events_Stop(null, EventArgs.Empty);

					float dist = SpecialEffectsViewerPreferences.that.DoubleCharacterDistance / 2f;

					_oIdiot1.Position = new Vector3(100f + dist, 100f, 0f);
					_oIdiot2.Position = new Vector3(100f - dist, 100f, 0f);

					var col1 = new NetDisplayObjectCollection(_oIdiot1);
					var col2 = new NetDisplayObjectCollection(_oIdiot2);
					NWN2NetDisplayManager.Instance.MoveObjects(col1, ChangeType.Absolute, false, _oIdiot1.Position);
					NWN2NetDisplayManager.Instance.MoveObjects(col2, ChangeType.Absolute, false, _oIdiot2.Position);
				}
			}
		}
		#endregion eventhandlers (scene-config)


		#region eventhandlers (appearance)
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

				if (Scenari != Scene.placedeffect)
					CreateScene_appearancechanged(sender == co_Source);
			}
		}

		/// <summary>
		/// Handles the checkchanged event for the Fela checkboxes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void cb_Fela_checkedchanged(object sender, EventArgs e)
		{
			if (!_init && Scenari != Scene.placedeffect)
				CreateScene_appearancechanged(sender == cb_SourceF);
		}

		/// <summary>
		/// Instantiates a scene when character-appearance changes.
		/// </summary>
		/// <param name="source">true if the Source appearance or gender changed</param>
		void CreateScene_appearancechanged(bool source)
		{
			switch (Scenari)
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
		#endregion eventhandlers (appearance)


		#region eventhandlers (ground)
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

			// NOTE: For whatever whacky reason rb_Scene_click() behaves
			// properly - by not playing the effect auto - but if instead
			// CreateSingleCharacterScene() or CreatePlacedEffectScene() are
			// called - since that's all that should be needed here - the
			// effect will play auto.
			//
			// CreateDoubleCharacterScene() does not play of course since it
			// does not apply an effect - unlike Sc or Pe.

			rb_Scene_click(null, EventArgs.Empty);

//			switch (Scenari) // create entire scene ->
//			{
//				case Scene.doublecharacter:
//					CreateDoubleCharacterScene();
//					break;
//
//				case Scene.singlecharacter:
//					CreateSingleCharacterScene();
//					break;
//
//				case Scene.placedeffect:
//					CreatePlacedEffectScene();
//					break;
//			}
//
//			if (SceneData != null)
//				SceneData.ResetDatatext();
		}
		#endregion eventhandlers (ground)


		#region eventhandlers (search/filter)
		/// <summary>
		/// Search the effects-list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void bu_Search_click(object sender, EventArgs e)
		{
			int id = Search.SearchEffects(lb_Effects, co_Search.Text, sender == bu_SearchDn);
			if (id != -1)
				lb_Effects.SelectedIndex = id;
			else
				SystemSounds.Beep.Play();

			UpdateSearchDropdown();
		}

		/// <summary>
		/// Adds the current string to the search-dropdown.
		/// </summary>
		void UpdateSearchDropdown()
		{
			if (   co_Search.Text != String.Empty
				&& co_Search.Text != co_Search.Items[0].ToString())
			{
				co_Search.Items.Insert(0, co_Search.Text);

				if (co_Search.Items.Count > 1)
				{
					for (int i = co_Search.Items.Count - 1; i != 0; --i)
					if (co_Search.Items[i].ToString() == co_Search.Text)
					{
						co_Search.Items.RemoveAt(i);
						break;
					}

					if (co_Search.Items.Count == 9)
						co_Search.Items.RemoveAt(8);
				}
				co_Search.SelectedIndex = 0; // else text gets cleared
			}
		}

		/// <summary>
		/// Applies the search-filter to the effects-list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void cb_Filter_checkedchanged(object sender, EventArgs e)
		{
			if (!_bypassFiltrRecursion)
			{
				if (cb_Filter.Checked)
				{
					if (co_Search.Text == String.Empty)
					{
						if (_filtr == String.Empty)
							_bypassFiltrRecursion = true;

						cb_Filter.Checked = false;	// recurse
						return;						// but don't run twice.
					}
					else
					{
						cb_Filter.Text = _filtr = co_Search.Text.ToLower();
						cb_Filter.BackColor = Color.SkyBlue; // <- win10 workaround.
					}
				}
				else
				{
					_filtr = String.Empty;
					cb_Filter.Text = "filtr";
					cb_Filter.BackColor = SystemColors.Control;
				}

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
			else
				_bypassFiltrRecursion = false;
		}
		#endregion eventhandlers (search/filter)


		#region eventhandlers (electron panel)
		/// <summary>
		/// Changes the mouse-cursor appropriately on mousedown event in the
		/// ElectronPanel.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void panel_mousedown(object sender, MouseEventArgs e)
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
		#endregion eventhandlers (electron panel)


		#region eventhandlers (buttons)
		/// <summary>
		/// Copies the currently selected special-effect string to the clipboard.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks>The Copy button shall not be enabled unless an effect is
		/// currently selected.</remarks>
		void bu_Copy_click(object sender, EventArgs e)
		{
			if (lb_Effects.SelectedIndex != -1) // safety.
				ClipboardAssistant.SetText(lb_Effects.SelectedItem.ToString());
		}
		#endregion eventhandlers (buttons)


		#region eventhandlers (general)
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
		/// Focuses the event-data textbox when the Events page is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void tc_click(object sender, EventArgs e)
		{
			if (tc_Options.SelectedIndex == 1) // Events page
			{
				tb_EventData.Focus();
				tb_EventData.SelectionStart =
				tb_EventData.SelectionLength = 0;
			}
		}
		#endregion eventhandlers (general)


		#region Methods
		/// <summary>
		/// Stops playing the effect, clears the SEFGroups, and removes all
		/// objects from the NetDisplay scene.
		/// </summary>
		void ClearScene()
		{
			if (_sefer != null) // NetDisplay is null on launch
			{
				Stop();
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

			float dist = SpecialEffectsViewerPreferences.that.DoubleCharacterDistance / 2f;

			_oIdiot1.Position = new Vector3(100f + dist, 100f, 0f);
			_oIdiot2.Position = new Vector3(100f - dist, 100f, 0f);
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
			NetDisplayObject idiot = NWN2NetDisplayManager.Instance.CreateNDOForInstance(_iIdiot, _panel.Scene, 0);

			idiot.Position = new Vector3(100f,100f,0f);
			NWN2NetDisplayManager.Instance.MoveObjects(new NetDisplayObjectCollection(idiot),
													   ChangeType.Absolute,
													   false,
													   idiot.Position);
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
			NetDisplayObject placedeffect = NWN2NetDisplayManager.Instance.CreateNDOForInstance(_iPlacedEffect, _panel.Scene, 0);

			placedeffect.Position = new Vector3(100f,100f,0f);
			NWN2NetDisplayManager.Instance.MoveObjects(new NetDisplayObjectCollection(placedeffect),
													   ChangeType.Absolute,
													   false,
													   placedeffect.Position);
		}

		/// <summary>
		/// Prints the currently loaded effect-events to the Options panel.
		/// </summary>
		/// <param name="setTitle"></param>"
		void PrintEffectData(bool setTitle = true)
		{
			if (setTitle) SetTitle();

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
		#endregion Methods
	}
}
