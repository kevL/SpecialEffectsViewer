using System;
using System.Windows.Forms;


namespace SpecialEffectsViewer
{
	sealed partial class sevi
	{
		#region Designer
		SplitContainer sc1_Effects;
		SplitContainer sc2_Options;
		GroupBox gb_Scene;
		RadioButton rb_PlacedEffect;
		RadioButton rb_SingleCharacter;
		RadioButton rb_DoubleCharacter;
		CheckBox cb_Ground;
		TextBox tb_EventData;
		Panel pa_Search;
		TextBox tb_Search;
		Button bu_SearchD;
		Button bu_SearchU;
		CheckBox cb_Filter;
		ListBox lb_Fx;
		Panel pa_bot;
		Button bu_Clear;
		Button bu_Copy;
		Button bu_Close;
		TextBox tb_SefData;
		SplitContainer sc3_Events;

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
			this.sc3_Events = new System.Windows.Forms.SplitContainer();
			this.tb_SefData = new System.Windows.Forms.TextBox();
			this.tb_EventData = new System.Windows.Forms.TextBox();
			this.cb_Ground = new System.Windows.Forms.CheckBox();
			this.gb_Scene = new System.Windows.Forms.GroupBox();
			this.rb_PlacedEffect = new System.Windows.Forms.RadioButton();
			this.rb_SingleCharacter = new System.Windows.Forms.RadioButton();
			this.rb_DoubleCharacter = new System.Windows.Forms.RadioButton();
			this.lb_Fx = new System.Windows.Forms.ListBox();
			this.pa_Search = new System.Windows.Forms.Panel();
			this.tb_Search = new System.Windows.Forms.TextBox();
			this.bu_SearchD = new System.Windows.Forms.Button();
			this.bu_SearchU = new System.Windows.Forms.Button();
			this.cb_Filter = new System.Windows.Forms.CheckBox();
			this.pa_bot = new System.Windows.Forms.Panel();
			this.bu_Clear = new System.Windows.Forms.Button();
			this.bu_Copy = new System.Windows.Forms.Button();
			this.bu_Close = new System.Windows.Forms.Button();
			this.sc1_Effects.Panel1.SuspendLayout();
			this.sc1_Effects.Panel2.SuspendLayout();
			this.sc1_Effects.SuspendLayout();
			this.sc2_Options.Panel1.SuspendLayout();
			this.sc2_Options.SuspendLayout();
			this.sc3_Events.Panel1.SuspendLayout();
			this.sc3_Events.Panel2.SuspendLayout();
			this.sc3_Events.SuspendLayout();
			this.gb_Scene.SuspendLayout();
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
			this.sc1_Effects.Panel2.Controls.Add(this.lb_Fx);
			this.sc1_Effects.Panel2.Controls.Add(this.pa_Search);
			this.sc1_Effects.Panel2.Controls.Add(this.pa_bot);
			this.sc1_Effects.Panel2MinSize = 0;
			this.sc1_Effects.Size = new System.Drawing.Size(592, 374);
			this.sc1_Effects.SplitterDistance = 375;
			this.sc1_Effects.TabIndex = 0;
			// 
			// sc2_Options
			// 
			this.sc2_Options.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sc2_Options.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.sc2_Options.IsSplitterFixed = true;
			this.sc2_Options.Location = new System.Drawing.Point(0, 0);
			this.sc2_Options.Margin = new System.Windows.Forms.Padding(0);
			this.sc2_Options.Name = "sc2_Options";
			// 
			// sc2_Options.Panel1
			// 
			this.sc2_Options.Panel1.Controls.Add(this.sc3_Events);
			this.sc2_Options.Panel1.Controls.Add(this.cb_Ground);
			this.sc2_Options.Panel1.Controls.Add(this.gb_Scene);
			this.sc2_Options.Panel1MinSize = 0;
			// 
			// sc2_Options.Panel2
			// 
			this.sc2_Options.Panel2.BackColor = System.Drawing.Color.Black;
			this.sc2_Options.Panel2MinSize = 0;
			this.sc2_Options.Size = new System.Drawing.Size(375, 374);
			this.sc2_Options.SplitterDistance = 266;
			this.sc2_Options.SplitterWidth = 1;
			this.sc2_Options.TabIndex = 0;
			// 
			// sc3_Events
			// 
			this.sc3_Events.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sc3_Events.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.sc3_Events.Location = new System.Drawing.Point(0, 76);
			this.sc3_Events.Margin = new System.Windows.Forms.Padding(0);
			this.sc3_Events.Name = "sc3_Events";
			this.sc3_Events.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// sc3_Events.Panel1
			// 
			this.sc3_Events.Panel1.Controls.Add(this.tb_SefData);
			this.sc3_Events.Panel1MinSize = 17;
			// 
			// sc3_Events.Panel2
			// 
			this.sc3_Events.Panel2.Controls.Add(this.tb_EventData);
			this.sc3_Events.Panel2MinSize = 0;
			this.sc3_Events.Size = new System.Drawing.Size(266, 298);
			this.sc3_Events.SplitterDistance = 109;
			this.sc3_Events.SplitterWidth = 2;
			this.sc3_Events.TabIndex = 2;
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
			this.tb_SefData.Size = new System.Drawing.Size(266, 109);
			this.tb_SefData.TabIndex = 0;
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
			this.tb_EventData.Size = new System.Drawing.Size(266, 187);
			this.tb_EventData.TabIndex = 0;
			// 
			// cb_Ground
			// 
			this.cb_Ground.Dock = System.Windows.Forms.DockStyle.Top;
			this.cb_Ground.Location = new System.Drawing.Point(0, 60);
			this.cb_Ground.Margin = new System.Windows.Forms.Padding(0);
			this.cb_Ground.Name = "cb_Ground";
			this.cb_Ground.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.cb_Ground.Size = new System.Drawing.Size(266, 16);
			this.cb_Ground.TabIndex = 1;
			this.cb_Ground.Text = "Ground";
			this.cb_Ground.UseVisualStyleBackColor = false;
			this.cb_Ground.Click += new System.EventHandler(this.cb_Ground_click);
			// 
			// gb_Scene
			// 
			this.gb_Scene.Controls.Add(this.rb_PlacedEffect);
			this.gb_Scene.Controls.Add(this.rb_SingleCharacter);
			this.gb_Scene.Controls.Add(this.rb_DoubleCharacter);
			this.gb_Scene.Dock = System.Windows.Forms.DockStyle.Top;
			this.gb_Scene.Location = new System.Drawing.Point(0, 0);
			this.gb_Scene.Margin = new System.Windows.Forms.Padding(0);
			this.gb_Scene.Name = "gb_Scene";
			this.gb_Scene.Padding = new System.Windows.Forms.Padding(0);
			this.gb_Scene.Size = new System.Drawing.Size(266, 60);
			this.gb_Scene.TabIndex = 0;
			this.gb_Scene.TabStop = false;
			// 
			// rb_PlacedEffect
			// 
			this.rb_PlacedEffect.Location = new System.Drawing.Point(6, 40);
			this.rb_PlacedEffect.Margin = new System.Windows.Forms.Padding(0);
			this.rb_PlacedEffect.Name = "rb_PlacedEffect";
			this.rb_PlacedEffect.Size = new System.Drawing.Size(269, 16);
			this.rb_PlacedEffect.TabIndex = 2;
			this.rb_PlacedEffect.Text = "Placed effect object";
			this.rb_PlacedEffect.UseVisualStyleBackColor = true;
			this.rb_PlacedEffect.Click += new System.EventHandler(this.rb_click);
			// 
			// rb_SingleCharacter
			// 
			this.rb_SingleCharacter.Location = new System.Drawing.Point(6, 25);
			this.rb_SingleCharacter.Margin = new System.Windows.Forms.Padding(0);
			this.rb_SingleCharacter.Name = "rb_SingleCharacter";
			this.rb_SingleCharacter.Size = new System.Drawing.Size(269, 16);
			this.rb_SingleCharacter.TabIndex = 1;
			this.rb_SingleCharacter.Text = "Single character [AppearanceSEF]";
			this.rb_SingleCharacter.UseVisualStyleBackColor = true;
			this.rb_SingleCharacter.Click += new System.EventHandler(this.rb_click);
			// 
			// rb_DoubleCharacter
			// 
			this.rb_DoubleCharacter.Checked = true;
			this.rb_DoubleCharacter.Location = new System.Drawing.Point(6, 10);
			this.rb_DoubleCharacter.Margin = new System.Windows.Forms.Padding(0);
			this.rb_DoubleCharacter.Name = "rb_DoubleCharacter";
			this.rb_DoubleCharacter.Size = new System.Drawing.Size(269, 16);
			this.rb_DoubleCharacter.TabIndex = 0;
			this.rb_DoubleCharacter.TabStop = true;
			this.rb_DoubleCharacter.Text = "Double character [source+target]";
			this.rb_DoubleCharacter.UseVisualStyleBackColor = true;
			this.rb_DoubleCharacter.Click += new System.EventHandler(this.rb_click);
			// 
			// lb_Fx
			// 
			this.lb_Fx.BackColor = System.Drawing.Color.Beige;
			this.lb_Fx.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lb_Fx.FormattingEnabled = true;
			this.lb_Fx.Location = new System.Drawing.Point(0, 20);
			this.lb_Fx.Margin = new System.Windows.Forms.Padding(0);
			this.lb_Fx.Name = "lb_Fx";
			this.lb_Fx.Size = new System.Drawing.Size(213, 328);
			this.lb_Fx.Sorted = true;
			this.lb_Fx.TabIndex = 1;
			this.lb_Fx.SelectedIndexChanged += new System.EventHandler(this.lb_Effects_selectedindexchanged);
			this.lb_Fx.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lb_Effects_keydown);
			// 
			// pa_Search
			// 
			this.pa_Search.Controls.Add(this.tb_Search);
			this.pa_Search.Controls.Add(this.bu_SearchD);
			this.pa_Search.Controls.Add(this.bu_SearchU);
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
			// bu_SearchD
			// 
			this.bu_SearchD.Dock = System.Windows.Forms.DockStyle.Right;
			this.bu_SearchD.Location = new System.Drawing.Point(122, 0);
			this.bu_SearchD.Margin = new System.Windows.Forms.Padding(0);
			this.bu_SearchD.Name = "bu_SearchD";
			this.bu_SearchD.Size = new System.Drawing.Size(23, 20);
			this.bu_SearchD.TabIndex = 1;
			this.bu_SearchD.Text = "d";
			this.bu_SearchD.UseVisualStyleBackColor = true;
			this.bu_SearchD.Click += new System.EventHandler(this.bu_Search_click);
			// 
			// bu_SearchU
			// 
			this.bu_SearchU.Dock = System.Windows.Forms.DockStyle.Right;
			this.bu_SearchU.Location = new System.Drawing.Point(145, 0);
			this.bu_SearchU.Margin = new System.Windows.Forms.Padding(0);
			this.bu_SearchU.Name = "bu_SearchU";
			this.bu_SearchU.Size = new System.Drawing.Size(23, 20);
			this.bu_SearchU.TabIndex = 2;
			this.bu_SearchU.Text = "u";
			this.bu_SearchU.UseVisualStyleBackColor = true;
			this.bu_SearchU.Click += new System.EventHandler(this.bu_Search_click);
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
			this.cb_Filter.Click += new System.EventHandler(this.cb_Filter_click);
			this.cb_Filter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cb_Filter_keydown);
			// 
			// pa_bot
			// 
			this.pa_bot.Controls.Add(this.bu_Clear);
			this.pa_bot.Controls.Add(this.bu_Copy);
			this.pa_bot.Controls.Add(this.bu_Close);
			this.pa_bot.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pa_bot.Location = new System.Drawing.Point(0, 348);
			this.pa_bot.Margin = new System.Windows.Forms.Padding(0);
			this.pa_bot.Name = "pa_bot";
			this.pa_bot.Size = new System.Drawing.Size(213, 26);
			this.pa_bot.TabIndex = 2;
			// 
			// bu_Clear
			// 
			this.bu_Clear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bu_Clear.Location = new System.Drawing.Point(41, 1);
			this.bu_Clear.Margin = new System.Windows.Forms.Padding(0);
			this.bu_Clear.Name = "bu_Clear";
			this.bu_Clear.Size = new System.Drawing.Size(57, 24);
			this.bu_Clear.TabIndex = 0;
			this.bu_Clear.Text = "clear";
			this.bu_Clear.UseVisualStyleBackColor = true;
			this.bu_Clear.Click += new System.EventHandler(this.bu_Clear_click);
			// 
			// bu_Copy
			// 
			this.bu_Copy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bu_Copy.Location = new System.Drawing.Point(98, 1);
			this.bu_Copy.Margin = new System.Windows.Forms.Padding(0);
			this.bu_Copy.Name = "bu_Copy";
			this.bu_Copy.Size = new System.Drawing.Size(57, 24);
			this.bu_Copy.TabIndex = 1;
			this.bu_Copy.Text = "copy";
			this.bu_Copy.UseVisualStyleBackColor = true;
			this.bu_Copy.Click += new System.EventHandler(this.bu_Copy_click);
			// 
			// bu_Close
			// 
			this.bu_Close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bu_Close.Location = new System.Drawing.Point(155, 1);
			this.bu_Close.Margin = new System.Windows.Forms.Padding(0);
			this.bu_Close.Name = "bu_Close";
			this.bu_Close.Size = new System.Drawing.Size(57, 24);
			this.bu_Close.TabIndex = 2;
			this.bu_Close.Text = "close";
			this.bu_Close.UseVisualStyleBackColor = true;
			this.bu_Close.Click += new System.EventHandler(this.bu_Close_click);
			// 
			// sevi
			// 
			this.ClientSize = new System.Drawing.Size(592, 374);
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
			this.sc3_Events.Panel1.ResumeLayout(false);
			this.sc3_Events.Panel1.PerformLayout();
			this.sc3_Events.Panel2.ResumeLayout(false);
			this.sc3_Events.Panel2.PerformLayout();
			this.sc3_Events.ResumeLayout(false);
			this.gb_Scene.ResumeLayout(false);
			this.pa_Search.ResumeLayout(false);
			this.pa_Search.PerformLayout();
			this.pa_bot.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion Designer
	}
}
