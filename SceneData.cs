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
		#region Fields
		sevi _f;
		#endregion Fields


		#region cTor
		/// <summary>
		/// 
		/// </summary>
		/// <param name="f"></param>
		/// <param name="scene"></param>
		internal SceneData(sevi f, NetDisplayScene scene)
		{
			InitializeComponent();
			_f = f;

			NetDisplayObjectCollection objects = scene.Objects;

			string text = String.Empty;
			foreach (var @object in objects)
			{
				text += @object + EventData.L;
			}
			tb_Scenedata.Text = text;


			Show();
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
		#endregion eventhandlers (override)



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
			this.tb_Scenedata.Size = new System.Drawing.Size(292, 274);
			this.tb_Scenedata.TabIndex = 0;
			this.tb_Scenedata.WordWrap = false;
			// 
			// SceneData
			// 
			this.ClientSize = new System.Drawing.Size(292, 274);
			this.Controls.Add(this.tb_Scenedata);
			this.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "SceneData";
			this.Text = "Scene data";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion designer
	}
}
