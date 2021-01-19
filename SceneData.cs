﻿using System;
using System.Drawing;
using System.Windows.Forms;

using OEIShared.NetDisplay;
using OEIShared.OEIMath;


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

		static int x = Int32.MinValue;
		static int y,w,h;
		#endregion Fields (static)


		#region Fields
		sevi _f;
		#endregion Fields


		#region cTor
		/// <summary>
		/// 
		/// </summary>
		/// <param name="f"></param>
		internal SceneData(sevi f)
		{
			InitializeComponent();
			_f = f;

			bool locset = false;
			if (x != Int32.MinValue)
			{
				if (sevi.checklocation(x,y))
				{
					locset = true;
					SetDesktopLocation(x,y);
				}
				ClientSize = new Size(w,h);
			}

			if (!locset)
				SetDesktopLocation(_f.Left + 20, _f.Top + 20);

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
			x = Left;
			y = Top;
			w = ClientSize.Width;
			h = ClientSize.Height;

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
				NetDisplayObjectCollection  objects = scene.Objects;

				text += "objects.Count= " + objects.Count + EventData.L;

				foreach (NetDisplayObject @object in objects)
				{
//					if (text != String.Empty) text += EventData.L;
					text += EventData.L;

					text += @object + EventData.L;

					text += "DisplayName= " + @object.DisplayName + EventData.L;
//					text += "Scene      = " + @object.Scene       + EventData.L;
					text += "ID         = " + @object.ID          + EventData.L;
					text += "HookedToID = " + @object.HookedToID  + EventData.L;
					text += "UserIndex  = " + @object.UserIndex   + EventData.L;
					text += "Position   = " + EventData.GetPositionString(@object.Position) + EventData.L;
					text += "Orientation= " + GetOrientationString(@object.Orientation)     + EventData.L;
					text += "Scale      = " + EventData.GetPositionString(@object.Scale)    + EventData.L;
					text += "Selectable = " + @object.Selectable  + EventData.L;
					text += "Visible    = " + @object.Visible     + EventData.L;
					text += "Tag        = " + @object.Tag         + EventData.L;

					if ((@object as NetDisplayModel) != null)
					{
						text += "is NetDisplayModel" + EventData.L;

						var model = @object as NetDisplayModel;
						text += "  Stance= "                 + model.Stance             + EventData.L;
						text += "  DisplayType= "            + model.GetDisplayType()   + EventData.L;
						text += "  AffectsWalkmesh= "        + model.AffectsWalkmesh    + EventData.L;
						if (model.Attachments != null)
							text += "  Attachments.Length= " + model.Attachments.Length + EventData.L;
						else
							text += "  Attachments= NULL"                               + EventData.L;
						if (model.Models != null)
							text += "  Models.Length= "      + model.Models.Length      + EventData.L;
						else
							text += "  Models= NULL"                                    + EventData.L;

//						text += "  BaseSkeleton= "  + model.GetBaseSkeletonName()  + EventData.L;
//						text += "  Skeleton= "      + model.GetSkeletonName()      + EventData.L;
//						text += "  ModelSlotFile= " + model.GetModelSlotFilename() + EventData.L;
//						text += "  ModelSlotPart= " + model.GetModelSlotPartName() + EventData.L;
					}
					else if ((@object as NetDisplaySEF) != null)
					{
						text += "is NetDisplaySEF" + EventData.L;

						var sef = @object as NetDisplaySEF;
						if (sef.SEF != null)
						{
							text += "  SEFGroup= " + sef.SEF      + EventData.L;
							text += "    Name"     + sef.SEF.Name + EventData.L;
							// etc.
						}
						else
							text += "  SEFGroup= NULL" + EventData.L;
					}
					else if ((@object as NetDisplayVisualEffect) != null) // INetDisplayVisualEffect
					{
						text += "is NetDisplayVisualEffect" + EventData.L;

						var visualeffect = @object as NetDisplayVisualEffect;
						text += "  DestinationBlend= "             + visualeffect.DestinationBlend             + EventData.L;
						text += "  SourceBlend= "                  + visualeffect.SourceBlend                  + EventData.L;
						text += "  FrameBufferEffect= "            + visualeffect.FrameBufferEffect            + EventData.L;
						text += "  FrameBufferPixelDisplacement= " + visualeffect.FrameBufferPixelDisplacement + EventData.L;
						text += "  Texture= "                      + visualeffect.Texture                      + EventData.L;
						text += "  TextureAnimationSpeed= "        + visualeffect.TextureAnimationSpeed        + EventData.L;
						text += "  TextureType= "                  + visualeffect.TextureType                  + EventData.L;

						if ((@object as NetDisplayTrail) != null)
						{
							text += "is NetDisplayTrail" + EventData.L;

							var trail = @object as NetDisplayTrail;
							text += "  ModelToFollow= "           + trail.ModelToFollow           + EventData.L;
							text += "  AttachmentPointToFollow= " + trail.AttachmentPointToFollow + EventData.L;
							text += "  InstanceToFollow= "        + trail.InstanceToFollow        + EventData.L;
							text += "  FadeoutTime= "             + trail.FadeoutTime             + EventData.L;
							text += "  PointTrail= "              + trail.PointTrail              + EventData.L;
							text += "  TrailColor= "              + trail.TrailColor              + EventData.L;
							text += "  TrailWidth= "              + trail.TrailWidth              + EventData.L;
							text += "  UpdateFrequency= "         + trail.UpdateFrequency         + EventData.L;
						}
					}


					if (@object is INetDisplayScene)
					{
						text += "is INetDisplayScene" + EventData.L;
					}

					if (@object is NetDisplayBeam)
					{
						text += "is NetDisplayBeam" + EventData.L;
					}

					if (@object is NetDisplayBillboard)
					{
						text += "is NetDisplayBillboard" + EventData.L;
					}

					if (@object is NetDisplayConnector)
					{
						text += "is NetDisplayConnector" + EventData.L;
					}

					if (@object is NetDisplayLight)
					{
						text += "is NetDisplayLight" + EventData.L;
					}

					if (@object is NetDisplayLightDirectional)
					{
						text += "is NetDisplayLightDirectional" + EventData.L;
					}

					if (@object is NetDisplayLightning)
					{
						text += "is NetDisplayLightning" + EventData.L;
					}

					if (@object is NetDisplayLightPoint)
					{
						text += "is NetDisplayLightPoint" + EventData.L;
					}

					if (@object is NetDisplayLineParticleSystem)
					{
						text += "is NetDisplayLineParticleSystem" + EventData.L;
					}

					if (@object is NetDisplayOrientationModel)
					{
						text += "is NetDisplayOrientationModel" + EventData.L;
					}

					if (@object is NetDisplayParticleSystem)
					{
						text += "is NetDisplayParticleSystem" + EventData.L;
					}

					if (@object is NetDisplayPolygon)
					{
						text += "is NetDisplayPolygon" + EventData.L;
					}

					if (@object is NetDisplayProjectedTexture)
					{
						text += "is NetDisplayProjectedTexture" + EventData.L;
					}

					if (@object is NetDisplaySound)
					{
						text += "is NetDisplaySound" + EventData.L;
					}

					if (@object is NetDisplaySphere)
					{
						text += "is NetDisplaySphere" + EventData.L;
					}

					if (@object is NetDisplayTile)
					{
						text += "is NetDisplayTile" + EventData.L;
					}
				}
			}
			tb_Scenedata.Text = text;

			tb_Scenedata.SelectionStart  =
			tb_Scenedata.SelectionLength = 0;
		}

		/// <summary>
		/// Gets a string for a quaternion.
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>
		internal static string GetOrientationString(RHQuaternion q)
		{
			return q.X + "," + q.Y + "," + q.Z + "," + q.W;
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
			this.tb_Scenedata.Size = new System.Drawing.Size(447, 699);
			this.tb_Scenedata.TabIndex = 0;
			this.tb_Scenedata.WordWrap = false;
			// 
			// SceneData
			// 
			this.ClientSize = new System.Drawing.Size(447, 699);
			this.Controls.Add(this.tb_Scenedata);
			this.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.KeyPreview = true;
			this.Name = "SceneData";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion designer
	}
}
