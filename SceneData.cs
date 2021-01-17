using System;
using System.Windows.Forms;

using OEIShared.NetDisplay;


namespace SpecialEffectsViewer
{
	/// <summary>
	/// Sef event data as applied to objects in the Scene.
	/// </summary>
	sealed class SceneData
		: Form
	{
		#region Fields (static)
		const string TITLE = "Scene data";
		#endregion Fields (static)


		#region Fields
		sevi _f;
		#endregion Fields


		#region cTor
		/// <summary>
		/// 
		/// </summary>
		/// <param name="f"></param>
		/// <param name="scene"></param>
		internal SceneData(sevi f, INetDisplayScene scene)
		{
			InitializeComponent();
			_f = f;

			SetDatatext(scene);

			Show(_f);
		}
		#endregion cTor


		#region eventhandlers (override)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			_f.SceneData = null;
			base.OnFormClosing(e);
		}

		/// <summary>
		/// Closes this dialog on [Esc] [Enter] or [Ctrl+n].
		/// @note Requires 'KeyPreview' true.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case Keys.Escape:
				case Keys.Enter:
				case Keys.Control | Keys.N:
					e.Handled = e.SuppressKeyPress = true;
					Close();
					break;
			}
			base.OnKeyDown(e);
		}
		#endregion eventhandlers (override)


		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="scene"></param>
		internal void SetDatatext(INetDisplayScene scene)
		{
			switch ((sevi.Scene)SpecialEffectsViewerPreferences.that.Scene)
			{
				case sevi.Scene.non:             Text = TITLE;                             break;
				case sevi.Scene.doublecharacter: Text = TITLE + " - Double character";     break;
				case sevi.Scene.singlecharacter: Text = TITLE + " - Single character";     break;
				case sevi.Scene.placedeffect:    Text = TITLE + " - Placed effect object"; break;
			}

			string text = String.Empty;

			if (scene != null && scene.Objects.Count != 0)
			{
				NetDisplayObjectCollection objects = scene.Objects;

				foreach (var @object in objects)
				{
					text += @object + EventData.L;
				}
			}
			tb_Scenedata.Text = text;
		}
		#endregion Methods



		#region designer
		TextBox tb_Scenedata;

		/// <summary>
		/// 
		/// </summary>
		void InitializeComponent()
		{
			this.tb_Scenedata = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// tb_Scenedata
			// 
			this.tb_Scenedata.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tb_Scenedata.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tb_Scenedata.Location = new System.Drawing.Point(0, 0);
			this.tb_Scenedata.Margin = new System.Windows.Forms.Padding(0);
			this.tb_Scenedata.Multiline = true;
			this.tb_Scenedata.Name = "tb_Scenedata";
			this.tb_Scenedata.ReadOnly = true;
			this.tb_Scenedata.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tb_Scenedata.Size = new System.Drawing.Size(407, 299);
			this.tb_Scenedata.TabIndex = 0;
			this.tb_Scenedata.WordWrap = false;
			// 
			// SceneData
			// 
			this.ClientSize = new System.Drawing.Size(407, 299);
			this.Controls.Add(this.tb_Scenedata);
			this.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.KeyPreview = true;
			this.Name = "SceneData";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion designer
	}
}
