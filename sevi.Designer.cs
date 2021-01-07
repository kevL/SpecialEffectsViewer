using System;
using System.Windows.Forms;


namespace SpecialEffectsViewer
{
	sealed partial class sevi
	{
		#region Designer
		SplitContainer sc;
		Panel pa_Search;
		TextBox tb_Search;
		Button bu_SearchD;
		Button bu_SearchU;
		CheckBox cb_Filter;
		ListBox lb_Fx;
		Panel pa_bot;
		GroupBox gb_Scene;
		RadioButton rb_PlacedEffect;
		RadioButton rb_SingleCharacter;
		RadioButton rb_DoubleCharacter;
		Button bu_Clear;
		Button bu_Copy;
		Button bu_Close;

		/// <summary>
		/// This method is required for Windows Forms designer support. Do not
		/// change the method contents inside the source code editor. The Forms
		/// designer might not be able to load this method if it was changed
		/// manually. sure ...
		/// </summary>
		void InitializeComponent()
		{
			this.sc = new System.Windows.Forms.SplitContainer();
			this.lb_Fx = new System.Windows.Forms.ListBox();
			this.pa_Search = new System.Windows.Forms.Panel();
			this.tb_Search = new System.Windows.Forms.TextBox();
			this.bu_SearchD = new System.Windows.Forms.Button();
			this.bu_SearchU = new System.Windows.Forms.Button();
			this.cb_Filter = new System.Windows.Forms.CheckBox();
			this.pa_bot = new System.Windows.Forms.Panel();
			this.gb_Scene = new System.Windows.Forms.GroupBox();
			this.rb_DoubleCharacter = new System.Windows.Forms.RadioButton();
			this.rb_PlacedEffect = new System.Windows.Forms.RadioButton();
			this.rb_SingleCharacter = new System.Windows.Forms.RadioButton();
			this.bu_Clear = new System.Windows.Forms.Button();
			this.bu_Copy = new System.Windows.Forms.Button();
			this.bu_Close = new System.Windows.Forms.Button();
			this.sc.Panel2.SuspendLayout();
			this.sc.SuspendLayout();
			this.pa_Search.SuspendLayout();
			this.pa_bot.SuspendLayout();
			this.gb_Scene.SuspendLayout();
			this.SuspendLayout();
			// 
			// sc
			// 
			this.sc.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sc.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.sc.Location = new System.Drawing.Point(0, 0);
			this.sc.Margin = new System.Windows.Forms.Padding(0);
			this.sc.Name = "sc";
			this.sc.Panel1MinSize = 0;
			// 
			// sc.Panel2
			// 
			this.sc.Panel2.Controls.Add(this.lb_Fx);
			this.sc.Panel2.Controls.Add(this.pa_Search);
			this.sc.Panel2.Controls.Add(this.pa_bot);
			this.sc.Panel2MinSize = 0;
			this.sc.Size = new System.Drawing.Size(592, 374);
			this.sc.SplitterDistance = 400;
			this.sc.TabIndex = 0;
			this.sc.MouseDown += new System.Windows.Forms.MouseEventHandler(this.sc_MouseDown);
			this.sc.MouseMove += new System.Windows.Forms.MouseEventHandler(this.sc_MouseMove);
			this.sc.MouseUp += new System.Windows.Forms.MouseEventHandler(this.sc_MouseUp);
			// 
			// lb_Fx
			// 
			this.lb_Fx.BackColor = System.Drawing.Color.Beige;
			this.lb_Fx.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lb_Fx.FormattingEnabled = true;
			this.lb_Fx.Location = new System.Drawing.Point(0, 20);
			this.lb_Fx.Margin = new System.Windows.Forms.Padding(0);
			this.lb_Fx.Name = "lb_Fx";
			this.lb_Fx.ScrollAlwaysVisible = true;
			this.lb_Fx.Size = new System.Drawing.Size(188, 270);
			this.lb_Fx.Sorted = true;
			this.lb_Fx.TabIndex = 1;
			this.lb_Fx.SelectedIndexChanged += new System.EventHandler(this.lb_Fx_selectedindexchanged);
			this.lb_Fx.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Search_keydown);
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
			this.pa_Search.Size = new System.Drawing.Size(188, 20);
			this.pa_Search.TabIndex = 0;
			// 
			// tb_Search
			// 
			this.tb_Search.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tb_Search.Location = new System.Drawing.Point(0, 0);
			this.tb_Search.Margin = new System.Windows.Forms.Padding(0);
			this.tb_Search.Name = "tb_Search";
			this.tb_Search.Size = new System.Drawing.Size(97, 20);
			this.tb_Search.TabIndex = 0;
			this.tb_Search.WordWrap = false;
			this.tb_Search.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Search_keydown);
			// 
			// bu_SearchD
			// 
			this.bu_SearchD.Dock = System.Windows.Forms.DockStyle.Right;
			this.bu_SearchD.Location = new System.Drawing.Point(97, 0);
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
			this.bu_SearchU.Location = new System.Drawing.Point(120, 0);
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
			this.cb_Filter.Location = new System.Drawing.Point(143, 0);
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
			this.pa_bot.Controls.Add(this.gb_Scene);
			this.pa_bot.Controls.Add(this.bu_Clear);
			this.pa_bot.Controls.Add(this.bu_Copy);
			this.pa_bot.Controls.Add(this.bu_Close);
			this.pa_bot.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pa_bot.Location = new System.Drawing.Point(0, 290);
			this.pa_bot.Margin = new System.Windows.Forms.Padding(0);
			this.pa_bot.Name = "pa_bot";
			this.pa_bot.Size = new System.Drawing.Size(188, 84);
			this.pa_bot.TabIndex = 2;
			// 
			// gb_Scene
			// 
			this.gb_Scene.Controls.Add(this.rb_DoubleCharacter);
			this.gb_Scene.Controls.Add(this.rb_PlacedEffect);
			this.gb_Scene.Controls.Add(this.rb_SingleCharacter);
			this.gb_Scene.Dock = System.Windows.Forms.DockStyle.Top;
			this.gb_Scene.Location = new System.Drawing.Point(0, 0);
			this.gb_Scene.Margin = new System.Windows.Forms.Padding(0);
			this.gb_Scene.Name = "gb_Scene";
			this.gb_Scene.Padding = new System.Windows.Forms.Padding(0);
			this.gb_Scene.Size = new System.Drawing.Size(188, 60);
			this.gb_Scene.TabIndex = 0;
			this.gb_Scene.TabStop = false;
			// 
			// rb_DoubleCharacter
			// 
			this.rb_DoubleCharacter.Location = new System.Drawing.Point(6, 40);
			this.rb_DoubleCharacter.Margin = new System.Windows.Forms.Padding(0);
			this.rb_DoubleCharacter.Name = "rb_DoubleCharacter";
			this.rb_DoubleCharacter.Size = new System.Drawing.Size(124, 16);
			this.rb_DoubleCharacter.TabIndex = 2;
			this.rb_DoubleCharacter.Text = "double character";
			this.rb_DoubleCharacter.UseVisualStyleBackColor = true;
			this.rb_DoubleCharacter.Click += new System.EventHandler(this.rb_click);
			// 
			// rb_PlacedEffect
			// 
			this.rb_PlacedEffect.Checked = true;
			this.rb_PlacedEffect.Location = new System.Drawing.Point(6, 10);
			this.rb_PlacedEffect.Margin = new System.Windows.Forms.Padding(0);
			this.rb_PlacedEffect.Name = "rb_PlacedEffect";
			this.rb_PlacedEffect.Size = new System.Drawing.Size(124, 16);
			this.rb_PlacedEffect.TabIndex = 0;
			this.rb_PlacedEffect.TabStop = true;
			this.rb_PlacedEffect.Text = "placed effect";
			this.rb_PlacedEffect.UseVisualStyleBackColor = true;
			this.rb_PlacedEffect.Click += new System.EventHandler(this.rb_click);
			// 
			// rb_SingleCharacter
			// 
			this.rb_SingleCharacter.Location = new System.Drawing.Point(6, 25);
			this.rb_SingleCharacter.Margin = new System.Windows.Forms.Padding(0);
			this.rb_SingleCharacter.Name = "rb_SingleCharacter";
			this.rb_SingleCharacter.Size = new System.Drawing.Size(124, 16);
			this.rb_SingleCharacter.TabIndex = 1;
			this.rb_SingleCharacter.Text = "single character";
			this.rb_SingleCharacter.UseVisualStyleBackColor = true;
			this.rb_SingleCharacter.Click += new System.EventHandler(this.rb_click);
			// 
			// bu_Clear
			// 
			this.bu_Clear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bu_Clear.Location = new System.Drawing.Point(10, 60);
			this.bu_Clear.Margin = new System.Windows.Forms.Padding(0);
			this.bu_Clear.Name = "bu_Clear";
			this.bu_Clear.Size = new System.Drawing.Size(57, 24);
			this.bu_Clear.TabIndex = 1;
			this.bu_Clear.Text = "clear";
			this.bu_Clear.UseVisualStyleBackColor = true;
			this.bu_Clear.Click += new System.EventHandler(this.bu_Clear_click);
			// 
			// bu_Copy
			// 
			this.bu_Copy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bu_Copy.Location = new System.Drawing.Point(70, 60);
			this.bu_Copy.Margin = new System.Windows.Forms.Padding(0);
			this.bu_Copy.Name = "bu_Copy";
			this.bu_Copy.Size = new System.Drawing.Size(57, 24);
			this.bu_Copy.TabIndex = 2;
			this.bu_Copy.Text = "copy";
			this.bu_Copy.UseVisualStyleBackColor = true;
			this.bu_Copy.Click += new System.EventHandler(this.bu_Copy_click);
			// 
			// bu_Close
			// 
			this.bu_Close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bu_Close.Location = new System.Drawing.Point(130, 60);
			this.bu_Close.Margin = new System.Windows.Forms.Padding(0);
			this.bu_Close.Name = "bu_Close";
			this.bu_Close.Size = new System.Drawing.Size(57, 24);
			this.bu_Close.TabIndex = 3;
			this.bu_Close.Text = "close";
			this.bu_Close.UseVisualStyleBackColor = true;
			this.bu_Close.Click += new System.EventHandler(this.bu_Close_click);
			// 
			// sevi
			// 
			this.ClientSize = new System.Drawing.Size(592, 374);
			this.Controls.Add(this.sc);
			this.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.KeyPreview = true;
			this.Name = "sevi";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = " Special Effects Viewer";
			this.sc.Panel2.ResumeLayout(false);
			this.sc.ResumeLayout(false);
			this.pa_Search.ResumeLayout(false);
			this.pa_Search.PerformLayout();
			this.pa_bot.ResumeLayout(false);
			this.gb_Scene.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion Designer
	}
}
