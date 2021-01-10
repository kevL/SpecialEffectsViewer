using System;
using System.Drawing;
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
			placedeffect,		// 1
			singlecharacter,	// 2
			doublecharacter		// 3
		}
		#endregion enums


		#region Fields (static)
		const string TITLE = "Special Effects Viewer";

		const string SEF_EXT = "sef";

		const string MODULES   = "modules";
		const string CAMPAIGNS = "campaigns";
		const string OVERRIDE  = "override";

		const int STALE_non      = 0x0;
		const int STALE_Module   = 0x1;
		const int STALE_Campaign = 0x2;

		static int WidthLeftPanel;
		#endregion Fields (static)


		#region Fields
		ElectronPanel _panel = new ElectronPanel(); // i hate u

		MenuItem _itFxList_all;
		MenuItem _itFxList_stock;
		MenuItem _itFxList_module;
		MenuItem _itFxList_campaign;
		MenuItem _itFxList_override;

		MenuItem _itLeft;
		MenuItem _itStayOnTop;

		int _isListStale;

		string _filtr = String.Empty;
		bool _bypassActivateSearchControl;

		bool _bypassEventsClear;
		#endregion Fields


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		internal sevi()
		{
			Owner = NWN2ToolsetMainForm.App;

			InitializeComponent();

			WidthLeftPanel = sc_left.SplitterDistance;

			// set unicode text on the up/down Search btns.
			bu_SearchD.Text = "\u25bc"; // down triangle
			bu_SearchU.Text = "\u25b2"; // up triangle

			CreateMainMenu();
			ConfigureElectronPanel();

			LoadPreferences();

			NWN2ToolsetMainForm.ModuleChanged                  += OnModuleChanged;
			NWN2CampaignManager.Instance.ActiveCampaignChanged += OnActiveCampaignChanged;

			listclick_All(null, EventArgs.Empty);

//			logger.log(StringDecryptor.Decrypt("ᒼᒮᒯ"));

			Show();
		}

		/// <summary>
		/// Instantiates the plugin's MainMenu.
		/// </summary>
		void CreateMainMenu()
		{
			Menu = new MainMenu();

			Menu.MenuItems.Add("&List");	// 0
			Menu.MenuItems.Add("&Events");	// 1
			Menu.MenuItems.Add("&View");	// 2
			Menu.MenuItems.Add("&Help");	// 3

			_itFxList_all      = Menu.MenuItems[0].MenuItems.Add("list &all fx",        listclick_All);
			_itFxList_stock    = Menu.MenuItems[0].MenuItems.Add("list &stock only",    listclick_Stock);
			_itFxList_module   = Menu.MenuItems[0].MenuItems.Add("list &module only",   listclick_Module);
			_itFxList_campaign = Menu.MenuItems[0].MenuItems.Add("list &campaign only", listclick_Campaign);
			_itFxList_override = Menu.MenuItems[0].MenuItems.Add("list &override only", listclick_Override);

			Menu.MenuItems[1].Enabled = false;

			_itLeft = Menu.MenuItems[2].MenuItems.Add("show &left panel", viewclick_Left);
			_itLeft.Shortcut = Shortcut.F8;
			_itLeft.Checked = true;

			Menu.MenuItems[2].MenuItems.Add("-");

			_itStayOnTop = Menu.MenuItems[2].MenuItems.Add("stay on &top", viewclick_StayOnTop);
			_itStayOnTop.Shortcut = Shortcut.CtrlT;
			_itStayOnTop.Checked = true;

			Menu.MenuItems[3].MenuItems.Add("&about", helpclick_About);
		}

		/// <summary>
		/// Layout control for the ElectronPanel.
		/// </summary>
		void ConfigureElectronPanel()
		{
			_panel.Dock = DockStyle.Fill;
			_panel.BorderStyle = BorderStyle.FixedSingle;
			_panel.MousePanel.KeyDown += Search_keydown;

			sc_left.Panel2.Controls.Add(_panel);
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

			sc.SplitterDistance = SpecialEffectsViewerPreferences.that.SplitterDistance;

			if (!SpecialEffectsViewerPreferences.that.ShowLeftPanel)
				_itLeft.PerformClick();

			if (!SpecialEffectsViewerPreferences.that.StayOnTop)
				_itStayOnTop.PerformClick();

			if (SpecialEffectsViewerPreferences.that.Maximized)
				WindowState = FormWindowState.Maximized;

			if ((Scene)SpecialEffectsViewerPreferences.that.Scene != Scene.placedeffect)
			{
				rb_PlacedEffect.Checked = false;

				switch ((Scene)SpecialEffectsViewerPreferences.that.Scene)
				{
					default:
						rb_PlacedEffect.Checked = true;
						break;

					case Scene.singlecharacter:
						rb_SingleCharacter.Checked = true;
						break;

					case Scene.doublecharacter:
						rb_DoubleCharacter.Checked =
						Menu.MenuItems[1].Enabled  = true;
						break;
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
		/// Saves preferences and unsubscribes from toolset-events.
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

			SpecialEffectsViewerPreferences.that.SplitterDistance = sc.SplitterDistance;

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
					listclick_All(null, EventArgs.Empty);
				}
				else if (_itFxList_module.Checked)
				{
					if ((_isListStale & STALE_Module) != 0)
					{
						_itFxList_module.Checked = false;
						listclick_Module(null, EventArgs.Empty);
					}
				}
				else if (_itFxList_campaign.Checked)
				{
					if ((_isListStale & STALE_Campaign) != 0)
					{
						_itFxList_campaign.Checked = false;
						listclick_Campaign(null, EventArgs.Empty);
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
				Close();

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


		#region eventhandlers (list)
		/// <summary>
		/// Populates the Fx-list with everything it can find.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void listclick_All(object sender, EventArgs e)
		{
			if (!_itFxList_all.Checked)
			{
				lb_Fx.BeginUpdate();

				ClearFxList();
				_itFxList_all.Checked = true;

				var entries = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF_EXT));
				foreach (IResourceEntry entry in entries)
				{
					if (_filtr == String.Empty || entry.ResRef.Value.ToLower().Contains(_filtr))
						lb_Fx.Items.Add(entry);
				}
				lb_Fx.EndUpdate();
			}

			if (!_bypassActivateSearchControl)
				ActiveControl = tb_Search;
		}

		/// <summary>
		/// Populates the Fx-list from the stock data folder.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void listclick_Stock(object sender, EventArgs e)
		{
			if (!_itFxList_stock.Checked)
			{
				lb_Fx.BeginUpdate();

				ClearFxList();
				_itFxList_stock.Checked = true;

				var entries = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF_EXT));
				foreach (IResourceEntry entry in entries)
				{
					string label = entry.Repository.Name.ToLower();
					if (   !label.Contains(MODULES) // fake it ->
						&& !label.Contains(CAMPAIGNS)
						&& !label.Contains(OVERRIDE)
						&& (_filtr == String.Empty || entry.ResRef.Value.ToLower().Contains(_filtr)))
					{
						lb_Fx.Items.Add(entry);
					}
				}
				lb_Fx.EndUpdate();
			}

			if (!_bypassActivateSearchControl)
				ActiveControl = tb_Search;
		}

		/// <summary>
		/// Populates the Fx-list from the current module folder.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void listclick_Module(object sender, EventArgs e)
		{
			if (!_itFxList_module.Checked)
			{
				lb_Fx.BeginUpdate();

				ClearFxList();
				_itFxList_module.Checked = true;

				var entries = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF_EXT));
				foreach (IResourceEntry entry in entries)
				{
					if (entry.Repository.Name.ToLower().Contains(MODULES) // fake it
						&& (_filtr == String.Empty || entry.ResRef.Value.ToLower().Contains(_filtr)))
					{
						lb_Fx.Items.Add(entry);
					}
				}
				lb_Fx.EndUpdate();
			}

			if (!_bypassActivateSearchControl)
				ActiveControl = tb_Search;
		}

		/// <summary>
		/// Populates the Fx-list from the current campaign folder.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void listclick_Campaign(object sender, EventArgs e)
		{
			if (!_itFxList_campaign.Checked)
			{
				lb_Fx.BeginUpdate();

				ClearFxList();
				_itFxList_campaign.Checked = true;

				var entries = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF_EXT));
				foreach (IResourceEntry entry in entries)
				{
					if (entry.Repository.Name.ToLower().Contains(CAMPAIGNS) // fake it
						&& (_filtr == String.Empty || entry.ResRef.Value.ToLower().Contains(_filtr)))
					{
						lb_Fx.Items.Add(entry);
					}
				}
				lb_Fx.EndUpdate();
			}

			if (!_bypassActivateSearchControl)
				ActiveControl = tb_Search;
		}

		/// <summary>
		/// Populates the Fx-list from the override folder.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void listclick_Override(object sender, EventArgs e)
		{
			if (!_itFxList_override.Checked)
			{
				lb_Fx.BeginUpdate();

				ClearFxList();
				_itFxList_override.Checked = true;

				var entries = NWN2ResourceManager.Instance.FindEntriesByType(BWResourceTypes.GetResourceType(SEF_EXT));
				foreach (IResourceEntry entry in entries)
				{
					if (entry.Repository.Name.ToLower().Contains(OVERRIDE) // fake it
						&& (_filtr == String.Empty || entry.ResRef.Value.ToLower().Contains(_filtr)))
					{
						lb_Fx.Items.Add(entry);
					}
				}
				lb_Fx.EndUpdate();
			}

			if (!_bypassActivateSearchControl)
				ActiveControl = tb_Search;
		}

		/// <summary>
		/// Clears the Fx-list and unchecks the List menuitems.
		/// </summary>
		void ClearFxList()
		{
			lb_Fx.SelectedIndex = -1;
			lb_Fx.Items.Clear();

			_itFxList_all     .Checked =
			_itFxList_stock   .Checked =
			_itFxList_module  .Checked =
			_itFxList_campaign.Checked =
			_itFxList_override.Checked = false;
		}
		#endregion eventhandlers (list)


		#region eventhandlers (view)
		/// <summary>
		/// Toggles display of the leftsidepanel.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void viewclick_Left(object sender, EventArgs e)
		{
			if (SpecialEffectsViewerPreferences.that.ShowLeftPanel =
				(_itLeft.Checked = !_itLeft.Checked))
			{
				sc_left.SplitterDistance = WidthLeftPanel;
			}
			else
				sc_left.SplitterDistance = 0;
		}

		/// <summary>
		/// Toggles toolset ownership of the plugin-window.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void viewclick_StayOnTop(object sender, EventArgs e)
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
		/// Displays the About dialog.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void helpclick_About(object sender, EventArgs e)
		{
			using (var f = new AboutF())
				f.ShowDialog(this);
		}
		#endregion eventhandlers (help)


		#region eventhandlers (controls)
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

			lb_Fx_selectedindexchanged(null, EventArgs.Empty);
		}

		/// <summary>
		/// Sets the Scene-preference and reloads the scene itself.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void rb_click(object sender, EventArgs e)
		{
			if (rb_PlacedEffect.Checked)
			{
				SpecialEffectsViewerPreferences.that.Scene = (int)Scene.placedeffect;
				Menu.MenuItems[1].Enabled = false;
			}
			else if (rb_SingleCharacter.Checked)
			{
				SpecialEffectsViewerPreferences.that.Scene = (int)Scene.singlecharacter;
				Menu.MenuItems[1].Enabled = false;
			}
			else //if (rb_DoubleCharacter.Checked)
			{
				SpecialEffectsViewerPreferences.that.Scene = (int)Scene.doublecharacter;
				Menu.MenuItems[1].Enabled = true;
			}

			lb_Fx_selectedindexchanged(null, EventArgs.Empty);
		}

		/// <summary>
		/// This is NOT "selectedindexchanged" - it's an effin click on the list
		/// OR changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void lb_Fx_selectedindexchanged(object sender, EventArgs e)
		{
			bu_Clear_click(null, EventArgs.Empty);

			if (lb_Fx.SelectedIndex != -1)
			{
				var effect = lb_Fx.SelectedItem as IResourceEntry;

				string path = effect.Repository.Name;
				Text = TITLE + " - " + path;

				var sefgroup = new SEFGroup();
				sefgroup.XmlUnserialize(effect.GetStream(false));

				if (rb_PlacedEffect.Checked)
				{
					var blueprint = new NWN2PlacedEffectBlueprint();
					var iinstance = NWN2GlobalBlueprintManager.CreateInstanceFromBlueprint(blueprint);
					(iinstance as NWN2PlacedEffectTemplate).Active = true;
					(iinstance as NWN2PlacedEffectTemplate).Effect = effect;

					NetDisplayObject oPlacedEffect = NWN2NetDisplayManager.Instance.CreateNDOForInstance(iinstance, _panel.NDWindow.Scene, 0);

					oPlacedEffect.Position = new Vector3(100f,100f,0f);
					NWN2NetDisplayManager.Instance.MoveObjects(new NetDisplayObjectCollection(oPlacedEffect), ChangeType.Absolute, false, oPlacedEffect.Position);
				}
				else if (rb_SingleCharacter.Checked)
				{
					var iIdiot1 = new NWN2CreatureInstance();
					iIdiot1.AppearanceType.Row = 4; // half-elf source/target
					iIdiot1.AppearanceSEF = effect;

					NetDisplayObject oIdiot1 = NWN2NetDisplayManager.Instance.CreateNDOForInstance(iIdiot1, _panel.NDWindow.Scene, 0);

					oIdiot1.Position = new Vector3(100f,100f,0f);
					NWN2NetDisplayManager.Instance.MoveObjects(new NetDisplayObjectCollection(oIdiot1), ChangeType.Absolute, false, oIdiot1.Position);
				}
				else //if (rb_DoubleCharacter.Checked)
				{
					InstantiateSefgroup(sefgroup);
				}

				PrintSefData(sefgroup);
				_panel.NDWindow.Scene.SpecialEffectsManager.BeginUpdating();
			}
			else
				Text = TITLE;
		}

		/// <summary>
		/// Instantiates a SEFGroup. Only double-characters play a sefgroup. A
		/// single-character could play a sefgroup I suppose, but prefer to use
		/// single-character to apply 'AppearanceSEF'.
		/// </summary>
		/// <param name="sefgroup"></param>
		void InstantiateSefgroup(SEFGroup sefgroup)
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
		/// Alters the current effect in accord with the Events menu and then
		/// plays it.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Events_click(object sender, EventArgs e)
		{
			// set the items' check
			var it = sender as MenuItem;
			if (it == Menu.MenuItems[1].MenuItems[Menu.MenuItems[1].MenuItems.Count - 2]) // clear all events
			{
				for (int i = 0; i != Menu.MenuItems[1].MenuItems.Count - 3; ++i)
					Menu.MenuItems[1].MenuItems[i].Checked = false;
			}
			else if (it == Menu.MenuItems[1].MenuItems[Menu.MenuItems[1].MenuItems.Count - 1]) // play all events
			{
				for (int i = 0; i != Menu.MenuItems[1].MenuItems.Count - 3; ++i)
					Menu.MenuItems[1].MenuItems[i].Checked = true;
			}
			else
				it.Checked = !it.Checked;

			// clear the netdisplay
			_bypassEventsClear = true;
			bu_Clear_click(null, EventArgs.Empty);
			_bypassEventsClear = false;

			// alter the sefgroup
			var effect_alt = CommonUtils.SerializationClone(lb_Fx.SelectedItem) as IResourceEntry;

			var altgroup = new SEFGroup();
			altgroup.XmlUnserialize(effect_alt.GetStream(false));

			for (int i = altgroup.Events.Count - 1; i != -1; --i)
			{
				if (!Menu.MenuItems[1].MenuItems[i].Checked)
					altgroup.Events.RemoveAt(i);
			}

			// play it
			InstantiateSefgroup(altgroup);
			_panel.NDWindow.Scene.SpecialEffectsManager.BeginUpdating();
		}

		/// <summary>
		/// Clears the scene.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void bu_Clear_click(object sender, EventArgs e)
		{
			_panel.NDWindow.Scene.SpecialEffectsManager.EndUpdating();
			_panel.NDWindow.Scene.SpecialEffectsManager.Groups.Clear();

			var objects = new NetDisplayObjectCollection();
			foreach (NetDisplayObject @object in _panel.Scene.Objects)
				objects.Add(@object);

			NWN2NetDisplayManager.Instance.RemoveObjects(objects);

			if (!_bypassEventsClear)
				Menu.MenuItems[1].MenuItems.Clear();
		}

		/// <summary>
		/// Copies the currently selected special-effect string to the clipboard.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void bu_Copy_click(object sender, EventArgs e)
		{
			if (lb_Fx.SelectedIndex != -1)
				Clipboard.SetDataObject(lb_Fx.SelectedItem.ToString());
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
		/// Search on KeyDown event when control has focus:
		/// - search textbox
		/// - fx listbox
		/// - electron panel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Search_keydown(object sender, KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case Keys.Enter:
					bu_Search_click(bu_SearchD, EventArgs.Empty);
					e.SuppressKeyPress = e.Handled = true;
					break;

				case Keys.Enter | Keys.Shift:
					bu_Search_click(bu_SearchU, EventArgs.Empty);
					e.SuppressKeyPress = e.Handled = true;
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
			int count = lb_Fx.Items.Count;
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
						if (lb_Fx.SelectedIndex == count - 1)
						{
							id = 0;
						}
						else
							id = lb_Fx.SelectedIndex + 1;

						while (!lb_Fx.Items[id].ToString().ToLower().Contains(text))
						{
							if (id == lb_Fx.SelectedIndex) // not found.
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
						if (lb_Fx.SelectedIndex < 1)
						{
							id = count - 1;
						}
						else
							id = lb_Fx.SelectedIndex - 1;

						while (!lb_Fx.Items[id].ToString().ToLower().Contains(text))
						{
							if (id == lb_Fx.SelectedIndex) // not found.
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

					lb_Fx.SelectedIndex = id;
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
				listclick_All(null, EventArgs.Empty);
			}
			else if (_itFxList_stock.Checked)
			{
				_itFxList_stock.Checked = false;
				listclick_Stock(null, EventArgs.Empty);
			}
			else if (_itFxList_module.Checked)
			{
				_itFxList_module.Checked = false;
				listclick_Module(null, EventArgs.Empty);
			}
			else if (_itFxList_campaign.Checked)
			{
				_itFxList_campaign.Checked = false;
				listclick_Campaign(null, EventArgs.Empty);
			}
			else //if (_itFxList_override.Checked)
			{
				_itFxList_override.Checked = false;
				listclick_Override(null, EventArgs.Empty);
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
				cb_Filter.Checked = !cb_Filter.Checked;
				cb_Filter_click(null, EventArgs.Empty);
			}
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
			sc_southwest.SplitterDistance = TextRenderer.MeasureText(text, Font).Height + 5;

			text = String.Empty;
			ISEFEvent sefevent;
			for (int i = 0; i != sefgroup.Events.Count; ++i)
			{
				if (text != String.Empty) text += L + L;
				text += i + L;

				sefevent = sefgroup.Events[i];

				text += "[" + sefevent.Name + "]" + L;
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
					var it = Menu.MenuItems[1].MenuItems.Add("event " + i, Events_click);
					it.Checked = true;
				}
			}
			tb_EventData.Text = text;

			if (rb_DoubleCharacter.Checked)
			{
				Menu.MenuItems[1].MenuItems.Add("-");
				Menu.MenuItems[1].MenuItems.Add("Clear all events", Events_click);
				Menu.MenuItems[1].MenuItems.Add("Play all events",  Events_click);
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
	}
}
