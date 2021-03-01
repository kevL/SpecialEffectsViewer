using System;
using System.Windows.Forms;


namespace SpecialEffectsViewer
{
	sealed partial class sevi
	{
		#region Designer
		SplitContainer sc1_Effects;

		Panel pa_Search;
		TextBox tb_Search;
		Button bu_SearchUp;
		Button bu_SearchDn;
		CheckBox cb_Filter;

		ListBox lb_Effects;

		Panel pa_bot;
		Button bu_Play;
		Button bu_Stop;
		Button bu_Copy;

		SplitContainer sc2_Options;
		TabControl tc_Options;

		TabPage tp_Options;
		GroupBox gb_Scene;
		RadioButton rb_DoubleCharacter;
		RadioButton rb_SingleCharacter;
		RadioButton rb_PlacedEffect;
		GroupBox gb_Appearance;
		Label la_Source;
		Label la_Target;
		ComboBox co_Source;
		ComboBox co_Target;
		CheckBox cb_SourceF;
		CheckBox cb_TargetF;
		GroupBox gb_Ground;
		CheckBox cb_Ground;

		TabPage tp_Events;
		SplitContainer sc3_Events;
		TextBox tb_SefData;
		TextBox tb_EventData;


		/// <summary>
		/// Resizes the bottom-panel buttons when the events-splitter is moved.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void sc1_Effects_splittermoved(object sender, SplitterEventArgs e)
		{
			int width = pa_bot.Width / 3;
			bu_Play.Width =
			bu_Stop.Width =
			bu_Copy.Width = width;

			bu_Play.Left = 0;
			bu_Stop.Left = bu_Play.Width;
			bu_Copy.Left = bu_Play.Width + bu_Stop.Width;
		}

		/// <summary>
		/// Invalidates the options-panel when the options-splitter is moved.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void sc2_Options_splittermoved(object sender, SplitterEventArgs e)
		{
			tc_Options.Invalidate();
		}

		/// <summary>
		/// Gets the height of the splitcontainer that contains the event-info.
		/// </summary>
		/// <returns>height of event-info container</returns>
		internal int GetEventsContainerHeight()
		{
			return sc3_Events.Height;
		}


		/// <summary>
		/// This method is required for Windows Forms designer support. Do not
		/// change the method contents inside the source code editor. The Forms
		/// designer might not be able to load this method if it was changed
		/// manually. sure ...
		/// </summary>
		void InitializeComponent()
		{
			this.sc1_Effects = new System.Windows.Forms.SplitContainer();
			this.sc2_Options = new System.Windows.Forms.SplitContainer();
			this.tc_Options = new System.Windows.Forms.TabControl();
			this.tp_Options = new System.Windows.Forms.TabPage();
			this.gb_Ground = new System.Windows.Forms.GroupBox();
			this.cb_Ground = new System.Windows.Forms.CheckBox();
			this.gb_Appearance = new System.Windows.Forms.GroupBox();
			this.co_Source = new System.Windows.Forms.ComboBox();
			this.co_Target = new System.Windows.Forms.ComboBox();
			this.la_Source = new System.Windows.Forms.Label();
			this.la_Target = new System.Windows.Forms.Label();
			this.cb_SourceF = new System.Windows.Forms.CheckBox();
			this.cb_TargetF = new System.Windows.Forms.CheckBox();
			this.gb_Scene = new System.Windows.Forms.GroupBox();
			this.rb_DoubleCharacter = new System.Windows.Forms.RadioButton();
			this.rb_SingleCharacter = new System.Windows.Forms.RadioButton();
			this.rb_PlacedEffect = new System.Windows.Forms.RadioButton();
			this.tp_Events = new System.Windows.Forms.TabPage();
			this.sc3_Events = new System.Windows.Forms.SplitContainer();
			this.tb_SefData = new System.Windows.Forms.TextBox();
			this.tb_EventData = new System.Windows.Forms.TextBox();
			this.lb_Effects = new System.Windows.Forms.ListBox();
			this.pa_Search = new System.Windows.Forms.Panel();
			this.tb_Search = new System.Windows.Forms.TextBox();
			this.bu_SearchUp = new System.Windows.Forms.Button();
			this.bu_SearchDn = new System.Windows.Forms.Button();
			this.cb_Filter = new System.Windows.Forms.CheckBox();
			this.pa_bot = new System.Windows.Forms.Panel();
			this.bu_Play = new System.Windows.Forms.Button();
			this.bu_Stop = new System.Windows.Forms.Button();
			this.bu_Copy = new System.Windows.Forms.Button();
			this.sc1_Effects.Panel1.SuspendLayout();
			this.sc1_Effects.Panel2.SuspendLayout();
			this.sc1_Effects.SuspendLayout();
			this.sc2_Options.Panel1.SuspendLayout();
			this.sc2_Options.SuspendLayout();
			this.tc_Options.SuspendLayout();
			this.tp_Options.SuspendLayout();
			this.gb_Ground.SuspendLayout();
			this.gb_Appearance.SuspendLayout();
			this.gb_Scene.SuspendLayout();
			this.tp_Events.SuspendLayout();
			this.sc3_Events.Panel1.SuspendLayout();
			this.sc3_Events.Panel2.SuspendLayout();
			this.sc3_Events.SuspendLayout();
			this.pa_Search.SuspendLayout();
			this.pa_bot.SuspendLayout();
			this.SuspendLayout();
			// 
			// sc1_Effects
			// 
			this.sc1_Effects.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sc1_Effects.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.sc1_Effects.Location = new System.Drawing.Point(0, 0);
			this.sc1_Effects.Margin = new System.Windows.Forms.Padding(0);
			this.sc1_Effects.Name = "sc1_Effects";
			// 
			// sc1_Effects.Panel1
			// 
			this.sc1_Effects.Panel1.Controls.Add(this.sc2_Options);
			this.sc1_Effects.Panel1MinSize = 0;
			// 
			// sc1_Effects.Panel2
			// 
			this.sc1_Effects.Panel2.Controls.Add(this.lb_Effects);
			this.sc1_Effects.Panel2.Controls.Add(this.pa_Search);
			this.sc1_Effects.Panel2.Controls.Add(this.pa_bot);
			this.sc1_Effects.Panel2MinSize = 0;
			this.sc1_Effects.Size = new System.Drawing.Size(792, 454);
			this.sc1_Effects.SplitterDistance = 575;
			this.sc1_Effects.TabIndex = 0;
			this.sc1_Effects.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.sc1_Effects_splittermoved);
			// 
			// sc2_Options
			// 
			this.sc2_Options.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sc2_Options.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.sc2_Options.Location = new System.Drawing.Point(0, 0);
			this.sc2_Options.Margin = new System.Windows.Forms.Padding(0);
			this.sc2_Options.Name = "sc2_Options";
			// 
			// sc2_Options.Panel1
			// 
			this.sc2_Options.Panel1.Controls.Add(this.tc_Options);
			this.sc2_Options.Panel1MinSize = 0;
			// 
			// sc2_Options.Panel2
			// 
			this.sc2_Options.Panel2.BackColor = System.Drawing.Color.Black;
			this.sc2_Options.Panel2MinSize = 0;
			this.sc2_Options.Size = new System.Drawing.Size(575, 454);
			this.sc2_Options.SplitterDistance = 255;
			this.sc2_Options.SplitterWidth = 3;
			this.sc2_Options.TabIndex = 0;
			this.sc2_Options.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.sc2_Options_splittermoved);
			// 
			// tc_Options
			// 
			this.tc_Options.Controls.Add(this.tp_Options);
			this.tc_Options.Controls.Add(this.tp_Events);
			this.tc_Options.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tc_Options.Location = new System.Drawing.Point(0, 0);
			this.tc_Options.Margin = new System.Windows.Forms.Padding(0);
			this.tc_Options.Name = "tc_Options";
			this.tc_Options.SelectedIndex = 0;
			this.tc_Options.Size = new System.Drawing.Size(255, 454);
			this.tc_Options.TabIndex = 0;
			// 
			// tp_Options
			// 
			this.tp_Options.Controls.Add(this.gb_Ground);
			this.tp_Options.Controls.Add(this.gb_Appearance);
			this.tp_Options.Controls.Add(this.gb_Scene);
			this.tp_Options.Location = new System.Drawing.Point(4, 22);
			this.tp_Options.Name = "tp_Options";
			this.tp_Options.Padding = new System.Windows.Forms.Padding(3);
			this.tp_Options.Size = new System.Drawing.Size(247, 428);
			this.tp_Options.TabIndex = 0;
			this.tp_Options.Text = "Options";
			this.tp_Options.UseVisualStyleBackColor = true;
			// 
			// gb_Ground
			// 
			this.gb_Ground.Controls.Add(this.cb_Ground);
			this.gb_Ground.Dock = System.Windows.Forms.DockStyle.Top;
			this.gb_Ground.Location = new System.Drawing.Point(3, 133);
			this.gb_Ground.Margin = new System.Windows.Forms.Padding(0);
			this.gb_Ground.Name = "gb_Ground";
			this.gb_Ground.Padding = new System.Windows.Forms.Padding(0);
			this.gb_Ground.Size = new System.Drawing.Size(241, 35);
			this.gb_Ground.TabIndex = 2;
			this.gb_Ground.TabStop = false;
			// 
			// cb_Ground
			// 
			this.cb_Ground.Location = new System.Drawing.Point(7, 11);
			this.cb_Ground.Margin = new System.Windows.Forms.Padding(0);
			this.cb_Ground.Name = "cb_Ground";
			this.cb_Ground.Size = new System.Drawing.Size(100, 20);
			this.cb_Ground.TabIndex = 0;
			this.cb_Ground.Text = "show Ground";
			this.cb_Ground.UseVisualStyleBackColor = false;
			this.cb_Ground.Click += new System.EventHandler(this.cb_Ground_click);
			// 
			// gb_Appearance
			// 
			this.gb_Appearance.Controls.Add(this.co_Source);
			this.gb_Appearance.Controls.Add(this.co_Target);
			this.gb_Appearance.Controls.Add(this.la_Source);
			this.gb_Appearance.Controls.Add(this.la_Target);
			this.gb_Appearance.Controls.Add(this.cb_SourceF);
			this.gb_Appearance.Controls.Add(this.cb_TargetF);
			this.gb_Appearance.Dock = System.Windows.Forms.DockStyle.Top;
			this.gb_Appearance.Location = new System.Drawing.Point(3, 66);
			this.gb_Appearance.Margin = new System.Windows.Forms.Padding(0);
			this.gb_Appearance.Name = "gb_Appearance";
			this.gb_Appearance.Padding = new System.Windows.Forms.Padding(0);
			this.gb_Appearance.Size = new System.Drawing.Size(241, 67);
			this.gb_Appearance.TabIndex = 1;
			this.gb_Appearance.TabStop = false;
			// 
			// co_Source
			// 
			this.co_Source.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.co_Source.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.co_Source.FormattingEnabled = true;
			this.co_Source.Location = new System.Drawing.Point(49, 13);
			this.co_Source.Margin = new System.Windows.Forms.Padding(0);
			this.co_Source.Name = "co_Source";
			this.co_Source.Size = new System.Drawing.Size(155, 21);
			this.co_Source.TabIndex = 1;
			this.co_Source.SelectedIndexChanged += new System.EventHandler(this.co_Appearance_selectedindexchanged);
			// 
			// co_Target
			// 
			this.co_Target.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.co_Target.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.co_Target.FormattingEnabled = true;
			this.co_Target.Location = new System.Drawing.Point(49, 38);
			this.co_Target.Margin = new System.Windows.Forms.Padding(0);
			this.co_Target.Name = "co_Target";
			this.co_Target.Size = new System.Drawing.Size(155, 21);
			this.co_Target.TabIndex = 4;
			this.co_Target.SelectedIndexChanged += new System.EventHandler(this.co_Appearance_selectedindexchanged);
			// 
			// la_Source
			// 
			this.la_Source.Location = new System.Drawing.Point(2, 16);
			this.la_Source.Margin = new System.Windows.Forms.Padding(0);
			this.la_Source.Name = "la_Source";
			this.la_Source.Size = new System.Drawing.Size(48, 20);
			this.la_Source.TabIndex = 0;
			this.la_Source.Text = "Source";
			// 
			// la_Target
			// 
			this.la_Target.Location = new System.Drawing.Point(2, 41);
			this.la_Target.Margin = new System.Windows.Forms.Padding(0);
			this.la_Target.Name = "la_Target";
			this.la_Target.Size = new System.Drawing.Size(48, 20);
			this.la_Target.TabIndex = 3;
			this.la_Target.Text = "Target";
			// 
			// cb_SourceF
			// 
			this.cb_SourceF.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cb_SourceF.CheckAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.cb_SourceF.Location = new System.Drawing.Point(209, 11);
			this.cb_SourceF.Margin = new System.Windows.Forms.Padding(0);
			this.cb_SourceF.Name = "cb_SourceF";
			this.cb_SourceF.Size = new System.Drawing.Size(29, 20);
			this.cb_SourceF.TabIndex = 2;
			this.cb_SourceF.Text = "F";
			this.cb_SourceF.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.cb_SourceF.UseVisualStyleBackColor = true;
			this.cb_SourceF.CheckedChanged += new System.EventHandler(this.cb_Fela_checkedchanged);
			// 
			// cb_TargetF
			// 
			this.cb_TargetF.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cb_TargetF.CheckAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.cb_TargetF.Location = new System.Drawing.Point(209, 36);
			this.cb_TargetF.Margin = new System.Windows.Forms.Padding(0);
			this.cb_TargetF.Name = "cb_TargetF";
			this.cb_TargetF.Size = new System.Drawing.Size(29, 20);
			this.cb_TargetF.TabIndex = 5;
			this.cb_TargetF.Text = "F";
			this.cb_TargetF.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.cb_TargetF.UseVisualStyleBackColor = true;
			this.cb_TargetF.CheckedChanged += new System.EventHandler(this.cb_Fela_checkedchanged);
			// 
			// gb_Scene
			// 
			this.gb_Scene.Controls.Add(this.rb_DoubleCharacter);
			this.gb_Scene.Controls.Add(this.rb_SingleCharacter);
			this.gb_Scene.Controls.Add(this.rb_PlacedEffect);
			this.gb_Scene.Dock = System.Windows.Forms.DockStyle.Top;
			this.gb_Scene.Location = new System.Drawing.Point(3, 3);
			this.gb_Scene.Margin = new System.Windows.Forms.Padding(0);
			this.gb_Scene.Name = "gb_Scene";
			this.gb_Scene.Padding = new System.Windows.Forms.Padding(0);
			this.gb_Scene.Size = new System.Drawing.Size(241, 63);
			this.gb_Scene.TabIndex = 0;
			this.gb_Scene.TabStop = false;
			// 
			// rb_DoubleCharacter
			// 
			this.rb_DoubleCharacter.Location = new System.Drawing.Point(6, 10);
			this.rb_DoubleCharacter.Margin = new System.Windows.Forms.Padding(0);
			this.rb_DoubleCharacter.Name = "rb_DoubleCharacter";
			this.rb_DoubleCharacter.Size = new System.Drawing.Size(219, 16);
			this.rb_DoubleCharacter.TabIndex = 0;
			this.rb_DoubleCharacter.Text = "Double character [source+target]";
			this.rb_DoubleCharacter.UseVisualStyleBackColor = true;
			this.rb_DoubleCharacter.Click += new System.EventHandler(this.rb_Scene_click);
			// 
			// rb_SingleCharacter
			// 
			this.rb_SingleCharacter.Location = new System.Drawing.Point(6, 26);
			this.rb_SingleCharacter.Margin = new System.Windows.Forms.Padding(0);
			this.rb_SingleCharacter.Name = "rb_SingleCharacter";
			this.rb_SingleCharacter.Size = new System.Drawing.Size(219, 16);
			this.rb_SingleCharacter.TabIndex = 1;
			this.rb_SingleCharacter.Text = "Single character [AppearanceSEF]";
			this.rb_SingleCharacter.UseVisualStyleBackColor = true;
			this.rb_SingleCharacter.Click += new System.EventHandler(this.rb_Scene_click);
			// 
			// rb_PlacedEffect
			// 
			this.rb_PlacedEffect.Location = new System.Drawing.Point(6, 42);
			this.rb_PlacedEffect.Margin = new System.Windows.Forms.Padding(0);
			this.rb_PlacedEffect.Name = "rb_PlacedEffect";
			this.rb_PlacedEffect.Size = new System.Drawing.Size(149, 16);
			this.rb_PlacedEffect.TabIndex = 2;
			this.rb_PlacedEffect.Text = "placed effect object";
			this.rb_PlacedEffect.UseVisualStyleBackColor = true;
			this.rb_PlacedEffect.Click += new System.EventHandler(this.rb_Scene_click);
			// 
			// tp_Events
			// 
			this.tp_Events.Controls.Add(this.sc3_Events);
			this.tp_Events.Location = new System.Drawing.Point(4, 22);
			this.tp_Events.Name = "tp_Events";
			this.tp_Events.Padding = new System.Windows.Forms.Padding(3);
			this.tp_Events.Size = new System.Drawing.Size(247, 428);
			this.tp_Events.TabIndex = 1;
			this.tp_Events.Text = "Events";
			this.tp_Events.UseVisualStyleBackColor = true;
			// 
			// sc3_Events
			// 
			this.sc3_Events.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sc3_Events.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.sc3_Events.Location = new System.Drawing.Point(3, 3);
			this.sc3_Events.Margin = new System.Windows.Forms.Padding(0);
			this.sc3_Events.Name = "sc3_Events";
			this.sc3_Events.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// sc3_Events.Panel1
			// 
			this.sc3_Events.Panel1.Controls.Add(this.tb_SefData);
			this.sc3_Events.Panel1MinSize = 0;
			// 
			// sc3_Events.Panel2
			// 
			this.sc3_Events.Panel2.Controls.Add(this.tb_EventData);
			this.sc3_Events.Panel2MinSize = 0;
			this.sc3_Events.Size = new System.Drawing.Size(241, 422);
			this.sc3_Events.SplitterDistance = 83;
			this.sc3_Events.SplitterWidth = 2;
			this.sc3_Events.TabIndex = 0;
			// 
			// tb_SefData
			// 
			this.tb_SefData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tb_SefData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tb_SefData.Location = new System.Drawing.Point(0, 0);
			this.tb_SefData.Margin = new System.Windows.Forms.Padding(0);
			this.tb_SefData.Multiline = true;
			this.tb_SefData.Name = "tb_SefData";
			this.tb_SefData.ReadOnly = true;
			this.tb_SefData.Size = new System.Drawing.Size(241, 83);
			this.tb_SefData.TabIndex = 0;
			this.tb_SefData.WordWrap = false;
			this.tb_SefData.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tb_keydown);
			// 
			// tb_EventData
			// 
			this.tb_EventData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tb_EventData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tb_EventData.Location = new System.Drawing.Point(0, 0);
			this.tb_EventData.Margin = new System.Windows.Forms.Padding(0);
			this.tb_EventData.Multiline = true;
			this.tb_EventData.Name = "tb_EventData";
			this.tb_EventData.ReadOnly = true;
			this.tb_EventData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tb_EventData.Size = new System.Drawing.Size(241, 337);
			this.tb_EventData.TabIndex = 0;
			this.tb_EventData.WordWrap = false;
			this.tb_EventData.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tb_keydown);
			// 
			// lb_Effects
			// 
			this.lb_Effects.BackColor = System.Drawing.Color.Beige;
			this.lb_Effects.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lb_Effects.FormattingEnabled = true;
			this.lb_Effects.Location = new System.Drawing.Point(0, 20);
			this.lb_Effects.Margin = new System.Windows.Forms.Padding(0);
			this.lb_Effects.Name = "lb_Effects";
			this.lb_Effects.Size = new System.Drawing.Size(213, 411);
			this.lb_Effects.Sorted = true;
			this.lb_Effects.TabIndex = 1;
			this.lb_Effects.SelectedIndexChanged += new System.EventHandler(this.lb_Effects_selectedindexchanged);
			// 
			// pa_Search
			// 
			this.pa_Search.Controls.Add(this.tb_Search);
			this.pa_Search.Controls.Add(this.bu_SearchUp);
			this.pa_Search.Controls.Add(this.bu_SearchDn);
			this.pa_Search.Controls.Add(this.cb_Filter);
			this.pa_Search.Dock = System.Windows.Forms.DockStyle.Top;
			this.pa_Search.Location = new System.Drawing.Point(0, 0);
			this.pa_Search.Margin = new System.Windows.Forms.Padding(0);
			this.pa_Search.Name = "pa_Search";
			this.pa_Search.Size = new System.Drawing.Size(213, 20);
			this.pa_Search.TabIndex = 0;
			// 
			// tb_Search
			// 
			this.tb_Search.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tb_Search.Location = new System.Drawing.Point(0, 0);
			this.tb_Search.Margin = new System.Windows.Forms.Padding(0);
			this.tb_Search.Name = "tb_Search";
			this.tb_Search.Size = new System.Drawing.Size(122, 20);
			this.tb_Search.TabIndex = 0;
			this.tb_Search.WordWrap = false;
			this.tb_Search.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tb_Search_keydown);
			// 
			// bu_SearchUp
			// 
			this.bu_SearchUp.Dock = System.Windows.Forms.DockStyle.Right;
			this.bu_SearchUp.Location = new System.Drawing.Point(122, 0);
			this.bu_SearchUp.Margin = new System.Windows.Forms.Padding(0);
			this.bu_SearchUp.Name = "bu_SearchUp";
			this.bu_SearchUp.Size = new System.Drawing.Size(23, 20);
			this.bu_SearchUp.TabIndex = 1;
			this.bu_SearchUp.Text = "u";
			this.bu_SearchUp.UseVisualStyleBackColor = true;
			this.bu_SearchUp.Click += new System.EventHandler(this.bu_Search_click);
			// 
			// bu_SearchDn
			// 
			this.bu_SearchDn.Dock = System.Windows.Forms.DockStyle.Right;
			this.bu_SearchDn.Location = new System.Drawing.Point(145, 0);
			this.bu_SearchDn.Margin = new System.Windows.Forms.Padding(0);
			this.bu_SearchDn.Name = "bu_SearchDn";
			this.bu_SearchDn.Size = new System.Drawing.Size(23, 20);
			this.bu_SearchDn.TabIndex = 2;
			this.bu_SearchDn.Text = "d";
			this.bu_SearchDn.UseVisualStyleBackColor = true;
			this.bu_SearchDn.Click += new System.EventHandler(this.bu_Search_click);
			// 
			// cb_Filter
			// 
			this.cb_Filter.Appearance = System.Windows.Forms.Appearance.Button;
			this.cb_Filter.Dock = System.Windows.Forms.DockStyle.Right;
			this.cb_Filter.Location = new System.Drawing.Point(168, 0);
			this.cb_Filter.Margin = new System.Windows.Forms.Padding(0);
			this.cb_Filter.Name = "cb_Filter";
			this.cb_Filter.Size = new System.Drawing.Size(45, 20);
			this.cb_Filter.TabIndex = 3;
			this.cb_Filter.Text = "filtr";
			this.cb_Filter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.cb_Filter.UseVisualStyleBackColor = true;
			this.cb_Filter.CheckedChanged += new System.EventHandler(this.cb_Filter_checkedchanged);
			this.cb_Filter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cb_Filter_keydown);
			// 
			// pa_bot
			// 
			this.pa_bot.Controls.Add(this.bu_Play);
			this.pa_bot.Controls.Add(this.bu_Stop);
			this.pa_bot.Controls.Add(this.bu_Copy);
			this.pa_bot.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pa_bot.Location = new System.Drawing.Point(0, 431);
			this.pa_bot.Margin = new System.Windows.Forms.Padding(0);
			this.pa_bot.Name = "pa_bot";
			this.pa_bot.Size = new System.Drawing.Size(213, 23);
			this.pa_bot.TabIndex = 2;
			// 
			// bu_Play
			// 
			this.bu_Play.Location = new System.Drawing.Point(0, 0);
			this.bu_Play.Margin = new System.Windows.Forms.Padding(0);
			this.bu_Play.Name = "bu_Play";
			this.bu_Play.Size = new System.Drawing.Size(70, 23);
			this.bu_Play.TabIndex = 0;
			this.bu_Play.Text = "PLAY";
			this.bu_Play.UseVisualStyleBackColor = true;
			this.bu_Play.Click += new System.EventHandler(this.mi_events_Play);
			// 
			// bu_Stop
			// 
			this.bu_Stop.Location = new System.Drawing.Point(72, 0);
			this.bu_Stop.Margin = new System.Windows.Forms.Padding(0);
			this.bu_Stop.Name = "bu_Stop";
			this.bu_Stop.Size = new System.Drawing.Size(70, 23);
			this.bu_Stop.TabIndex = 1;
			this.bu_Stop.Text = "STOP";
			this.bu_Stop.UseVisualStyleBackColor = true;
			this.bu_Stop.Click += new System.EventHandler(this.mi_events_Stop);
			// 
			// bu_Copy
			// 
			this.bu_Copy.Enabled = false;
			this.bu_Copy.Location = new System.Drawing.Point(144, 0);
			this.bu_Copy.Margin = new System.Windows.Forms.Padding(0);
			this.bu_Copy.Name = "bu_Copy";
			this.bu_Copy.Size = new System.Drawing.Size(70, 23);
			this.bu_Copy.TabIndex = 2;
			this.bu_Copy.Text = "COPY";
			this.bu_Copy.UseVisualStyleBackColor = true;
			this.bu_Copy.Click += new System.EventHandler(this.bu_Copy_click);
			// 
			// sevi
			// 
			this.ClientSize = new System.Drawing.Size(792, 454);
			this.Controls.Add(this.sc1_Effects);
			this.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.KeyPreview = true;
			this.Name = "sevi";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = " Special Effects Viewer";
			this.sc1_Effects.Panel1.ResumeLayout(false);
			this.sc1_Effects.Panel2.ResumeLayout(false);
			this.sc1_Effects.ResumeLayout(false);
			this.sc2_Options.Panel1.ResumeLayout(false);
			this.sc2_Options.ResumeLayout(false);
			this.tc_Options.ResumeLayout(false);
			this.tp_Options.ResumeLayout(false);
			this.gb_Ground.ResumeLayout(false);
			this.gb_Appearance.ResumeLayout(false);
			this.gb_Scene.ResumeLayout(false);
			this.tp_Events.ResumeLayout(false);
			this.sc3_Events.Panel1.ResumeLayout(false);
			this.sc3_Events.Panel1.PerformLayout();
			this.sc3_Events.Panel2.ResumeLayout(false);
			this.sc3_Events.Panel2.PerformLayout();
			this.sc3_Events.ResumeLayout(false);
			this.pa_Search.ResumeLayout(false);
			this.pa_Search.PerformLayout();
			this.pa_bot.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion Designer
	}
}
