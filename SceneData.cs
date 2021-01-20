using System;
using System.Drawing;
using System.Text;
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
		/// <summary>
		/// This dialog's parent.
		/// </summary>
		sevi _f;

		/// <summary>
		/// A timer that delays inspecting the scene for data until after the
		/// toolset DLLs get their shit together.
		/// </summary>
		Timer _t1 = new Timer();
		#endregion Fields


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="f"></param>
		internal SceneData(sevi f)
		{
			InitializeComponent();
			_f = f;

			_t1.Interval = SpecialEffectsViewerPreferences.that.SceneDataDelay;
			_t1.Tick += OnTick;

			bool locset = false;
			int x = SpecialEffectsViewerPreferences.that.SceneData_x;
			if (x > -1)
			{
				int y = SpecialEffectsViewerPreferences.that.SceneData_y;
				if (y > -1 && sevi.checklocation(x,y))
				{
					locset = true;
					SetDesktopLocation(x,y);
				}
				ClientSize = new Size(SpecialEffectsViewerPreferences.that.SceneData_w,
									  SpecialEffectsViewerPreferences.that.SceneData_h);
			}

			if (!locset)
				SetDesktopLocation(_f.Left + 20, _f.Top + 20);

			Show(_f);
//			ClearDatatext();	// -> don't do that, the data is incomplete. Press [F5] if you want to
		}						// see scene-data that's still available after the effect finishes rendering.
		#endregion cTor


		#region eventhandlers (override)
		/// <summary>
		/// Registers this dialog's telemetry and nulls the pointer in 'sevi'.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			SpecialEffectsViewerPreferences.that.SceneData_x = DesktopLocation.X;
			SpecialEffectsViewerPreferences.that.SceneData_y = DesktopLocation.Y;
			SpecialEffectsViewerPreferences.that.SceneData_w = ClientSize.Width;
			SpecialEffectsViewerPreferences.that.SceneData_h = ClientSize.Height;

			_t1.Dispose();
			_f.SceneData = null;
			base.OnFormClosing(e);
		}

		/// <summary>
		/// Closes this dialog on [Esc] [Enter] or [Ctrl+n]. Refresh on [F5].
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

				case Keys.F5:
					e.Handled = e.SuppressKeyPress = true;
					SetDatatext();
					break;
			}
			base.OnKeyDown(e);
		}
		#endregion eventhandlers (override)


		#region eventhanlders
		/// <summary>
		/// Stops the timer and displays the text.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnTick(object sender, EventArgs e)
		{
			_t1.Stop();
			SetDatatext();
		}
		#endregion eventhanlders


		#region Methods
		/// <summary>
		/// Clears the text and starts the timer.
		/// </summary>
		internal void ClearDatatext()
		{
			tb_Scenedata.Text = String.Empty;
			_t1.Start();
		}

		/// <summary>
		/// Conflates a bunch of data about the current scene to text.
		/// </summary>
		internal void SetDatatext()
		{
			switch ((sevi.Scene)SpecialEffectsViewerPreferences.that.Scene)
			{
				case sevi.Scene.non:             Text = TITLE;                             break;
				case sevi.Scene.doublecharacter: Text = TITLE + " - Double character";     break;
				case sevi.Scene.singlecharacter: Text = TITLE + " - Single character";     break;
				case sevi.Scene.placedeffect:    Text = TITLE + " - Placed effect object"; break;
			}

			var scene = _f._panel.Scene;

			string text = String.Empty;

			if (scene != null && scene.Objects.Count != 0)
			{
				var sb = new StringBuilder();

				NetDisplayObjectCollection objects = scene.Objects;

				sb.Append("objects.Count= " + objects.Count + EventData.L);

				foreach (NetDisplayObject @object in objects)
				{
					sb.Append(EventData.L);

					sb.Append(@object + EventData.L);

//					sb.Append("DisplayName = " + @object.DisplayName                   + EventData.L);
//					sb.Append("Scene       = " + @object.Scene                         + EventData.L);
					sb.Append("ID          = " + @object.ID                            + EventData.L);
					sb.Append("HookedToID  = " + @object.HookedToID                    + EventData.L);
					sb.Append("UserIndex   = " + @object.UserIndex                     + EventData.L);
					sb.Append("Position    = " + util.Get3dString(@object.Position)    + EventData.L);
					sb.Append("Orientation = " + util.Get4dString(@object.Orientation) + EventData.L);
					sb.Append("Scale       = " + util.Get3dString(@object.Scale)       + EventData.L);
					sb.Append("Selectable  = " + @object.Selectable                    + EventData.L);
					sb.Append("Visible     = " + @object.Visible                       + EventData.L);
					sb.Append("Tag         = " + @object.Tag                           + EventData.L);

					if (@object as NetDisplayModel != null)
					{
						sb.Append("is NetDisplayModel" + EventData.L);

						var model = @object as NetDisplayModel;
						sb.Append("  Stance             = " + model.Stance                 + EventData.L);
						sb.Append("  DisplayType        = " + model.GetDisplayType()       + EventData.L);
						sb.Append("  AffectsWalkmesh    = " + model.AffectsWalkmesh        + EventData.L);
						if (model.Attachments != null)
							sb.Append("  Attachments.Length = " + model.Attachments.Length + EventData.L);
						else
							sb.Append("  Attachments        = NULL"                        + EventData.L);
						if (model.Models != null)
							sb.Append("  Models.Length      = " + model.Models.Length      + EventData.L);
						else
							sb.Append("  Models             = NULL"                        + EventData.L);

//						sb.Append("  BaseSkeleton = "  + model.GetBaseSkeletonName()  + EventData.L);
//						sb.Append("  Skeleton = "      + model.GetSkeletonName()      + EventData.L);
//						sb.Append("  ModelSlotFile = " + model.GetModelSlotFilename() + EventData.L);
//						sb.Append("  ModelSlotPart = " + model.GetModelSlotPartName() + EventData.L);
					}
					else if (@object as NetDisplaySEF != null)
					{
						sb.Append("is NetDisplaySEF" + EventData.L);

						var sef = @object as NetDisplaySEF;
						if (sef.SEF != null)
						{
							sb.Append("  SEFGroup = " + sef.SEF      + EventData.L);
//							sb.Append("    Name = "   + sef.SEF.Name + EventData.L);
							// etc.
						}
						else
							sb.Append("  SEFGroup = NULL" + EventData.L);
					}
					else if (@object as INetDisplayVisualEffect != null)
					{
						sb.Append("is INetDisplayVisualEffect" + EventData.L);

						if (@object as NetDisplayVisualEffect != null)
						{
							sb.Append("is NetDisplayVisualEffect" + EventData.L);

							var visualeffect = @object as NetDisplayVisualEffect;
							sb.Append("  DestinationBlend             = " + visualeffect.DestinationBlend             + EventData.L);
							sb.Append("  FrameBufferEffect            = " + visualeffect.FrameBufferEffect            + EventData.L);
							sb.Append("  FrameBufferPixelDisplacement = " + visualeffect.FrameBufferPixelDisplacement + EventData.L);
							sb.Append("  SourceBlend                  = " + visualeffect.SourceBlend                  + EventData.L);
							sb.Append("  Texture                      = " + visualeffect.Texture                      + EventData.L);
							sb.Append("  TextureAnimationSpeed        = " + visualeffect.TextureAnimationSpeed        + EventData.L);
							sb.Append("  TextureType                  = " + visualeffect.TextureType                  + EventData.L);

							if (@object as NetDisplayBeam != null)
							{
								sb.Append("is NetDisplayBeam" + EventData.L);

								var beam = @object as NetDisplayBeam;
								sb.Append("  Amplitude      = " + beam.Amplitude                       + EventData.L);
								sb.Append("  BeginColor     = " + util.GetColorString(beam.BeginColor) + EventData.L);
								sb.Append("  BeginPosition  = " + util.Get3dString(beam.BeginPosition) + EventData.L);
								sb.Append("  BeginWidth     = " + beam.BeginWidth                      + EventData.L);
								sb.Append("  EndColor       = " + util.GetColorString(beam.EndColor)   + EventData.L);
								sb.Append("  EndPosition    = " + util.Get3dString(beam.EndPosition)   + EventData.L);
								sb.Append("  EndWidth       = " + beam.EndWidth                        + EventData.L);
								sb.Append("  Frequency      = " + beam.Frequency                       + EventData.L);
								sb.Append("  RateOfFlow     = " + beam.RateOfFlow                      + EventData.L);
								sb.Append("  Repeat         = " + beam.Repeat                          + EventData.L);
								sb.Append("  RepeatDistance = " + beam.RepeatDistance                  + EventData.L);
							}
							else if (@object as NetDisplayBillboard != null)
							{
								sb.Append("is NetDisplayBillboard" + EventData.L);

								var billboard = @object as NetDisplayBillboard;
								sb.Append("  AngularVelocity            = " + billboard.AngularVelocity                              + EventData.L);
								sb.Append("  BeginSize                  = " + util.Get2dString(billboard.BeginSize)                  + EventData.L);
								sb.Append("  BillboardOrientationVector = " + util.Get3dString(billboard.BillboardOrientationVector) + EventData.L);
								sb.Append("  EndColor                   = " + util.GetColorString(billboard.EndColor)                + EventData.L);
								sb.Append("  EndSize                    = " + util.Get2dString(billboard.EndSize)                    + EventData.L);
								sb.Append("  Lerp                       = " + billboard.Lerp                                         + EventData.L);
								sb.Append("  LerpPeriod                 = " + billboard.LerpPeriod                                   + EventData.L);
								sb.Append("  OrientationExplicitMode    = " + billboard.OrientationExplicitMode                      + EventData.L);
								sb.Append("  OrientationType            = " + billboard.OrientationType                              + EventData.L);
								sb.Append("  OrientPerpendicular        = " + billboard.OrientPerpendicular                          + EventData.L);
								sb.Append("  StartColor                 = " + util.GetColorString(billboard.StartColor)              + EventData.L);
							}
							else if (@object as NetDisplayLightning != null)
							{
								sb.Append("is NetDisplayLightning" + EventData.L);

								var lightning = @object as NetDisplayLightning;
								sb.Append("  BeginBranch       = " + lightning.BeginBranch                     + EventData.L);
								sb.Append("  BeginColor        = " + util.GetColorString(lightning.BeginColor) + EventData.L);
								sb.Append("  BeginPosition     = " + util.Get3dString(lightning.BeginPosition) + EventData.L);
								sb.Append("  BeginWidth        = " + lightning.BeginWidth                      + EventData.L);
								sb.Append("  BranchProbability = " + lightning.BranchProbability               + EventData.L);
								sb.Append("  EndBranch         = " + lightning.EndBranch                       + EventData.L);
								sb.Append("  EndColor          = " + util.GetColorString(lightning.EndColor)   + EventData.L);
								sb.Append("  EndPosition       = " + util.Get3dString(lightning.EndPosition)   + EventData.L);
								sb.Append("  EndWidth          = " + lightning.EndWidth                        + EventData.L);
								sb.Append("  Jaggedness        = " + lightning.Jaggedness                      + EventData.L);
								sb.Append("  Jumpiness         = " + lightning.Jumpiness                       + EventData.L);
								sb.Append("  Spread            = " + lightning.Spread                          + EventData.L);
								sb.Append("  UpdateFrequency   = " + lightning.UpdateFrequency                 + EventData.L);
							}
							else if (@object as NetDisplayLineParticleSystem != null)
							{
								sb.Append("is NetDisplayLineParticleSystem" + EventData.L);

								var system = @object as NetDisplayLineParticleSystem;
								sb.Append("  Acceleration           = " + util.Get3dString(system.Acceleration)            + EventData.L);
								sb.Append("  AngularVelocity        = " + system.AngularVelocity                           + EventData.L);
								sb.Append("  BeginPointBeginColor   = " + util.GetColorString(system.BeginPointBeginColor) + EventData.L);
								sb.Append("  BeginPointEndColor     = " + util.GetColorString(system.BeginPointEndColor)   + EventData.L);
								sb.Append("  BeginSize              = " + system.BeginSize                                 + EventData.L);
								sb.Append("  ConeHalfAngle          = " + system.ConeHalfAngle                             + EventData.L);
								sb.Append("  ControlType            = " + system.ControlType                               + EventData.L);
								sb.Append("  EmissionRadius         = " + system.EmissionRadius                            + EventData.L);
								sb.Append("  EmissionRadiusVariance = " + system.EmissionRadiusVariance                    + EventData.L);
								sb.Append("  EndPointBeginColor     = " + util.GetColorString(system.EndPointBeginColor)   + EventData.L);
								sb.Append("  EndPointEndColor       = " + util.GetColorString(system.EndPointEndColor)     + EventData.L);
								sb.Append("  EndSize                = " + system.EndSize                                   + EventData.L);
								sb.Append("  FlowType               = " + system.FlowType                                  + EventData.L);
								sb.Append("  LifetimeVariance       = " + system.LifetimeVariance                          + EventData.L);
								sb.Append("  ParticleLifetime       = " + system.ParticleLifetime                          + EventData.L);
								sb.Append("  SizeVariance           = " + system.SizeVariance                              + EventData.L);
								sb.Append("  SpawnRate              = " + system.SpawnRate                                 + EventData.L);
								sb.Append("  StartPhi               = " + system.StartPhi                                  + EventData.L);
								sb.Append("  StartTheta             = " + system.StartTheta                                + EventData.L);
								sb.Append("  SystemRelative         = " + system.SystemRelative                            + EventData.L);
								sb.Append("  Velocity               = " + util.Get3dString(system.Velocity)                + EventData.L);
								sb.Append("  VelocityVariance       = " + system.VelocityVariance                          + EventData.L);
							}
							else if (@object as NetDisplayParticleSystem != null)
							{
								sb.Append("is NetDisplayParticleSystem" + EventData.L);

								var system = @object as NetDisplayParticleSystem;
								sb.Append("  Acceleration           = " + util.Get3dString(system.Acceleration)  + EventData.L);
								sb.Append("  AngularVelocity        = " + system.AngularVelocity                 + EventData.L);
								sb.Append("  BeginSize              = " + util.Get2dString(system.BeginSize)     + EventData.L);
								sb.Append("  ConeHalfAngle          = " + system.ConeHalfAngle                   + EventData.L);
								sb.Append("  ControlType            = " + system.ControlType                     + EventData.L);
								sb.Append("  Disk                   = " + system.Disk                            + EventData.L);
								sb.Append("  EmissionRadius         = " + system.EmissionRadius                  + EventData.L);
								sb.Append("  EmissionRadiusVariance = " + system.EmissionRadiusVariance          + EventData.L);
								sb.Append("  EmissionSlot           = " + system.EmissionSlot                    + EventData.L);
								sb.Append("  EmitFromMesh           = " + system.EmitFromMesh                    + EventData.L);
								sb.Append("  EndColor               = " + util.GetColorString(system.EndColor)   + EventData.L);
								sb.Append("  EndSize                = " + util.Get2dString(system.EndSize)       + EventData.L);
								sb.Append("  FlowType               = " + system.FlowType                        + EventData.L);
								sb.Append("  GravitateTowardsCenter = " + system.GravitateTowardsCenter          + EventData.L);
								sb.Append("  LifetimeVariance       = " + system.LifetimeVariance                + EventData.L);
								sb.Append("  ModelForEmission       = " + system.ModelForEmission                + EventData.L);
								sb.Append("  OrientationType        = " + system.OrientationType                 + EventData.L);
								sb.Append("  ParticleLifetime       = " + system.ParticleLifetime                + EventData.L);
								sb.Append("  SizeVariance           = " + system.SizeVariance                    + EventData.L);
								sb.Append("  SpawnRate              = " + system.SpawnRate                       + EventData.L);
								sb.Append("  StartAngleVariance     = " + system.StartAngleVariance              + EventData.L);
								sb.Append("  StartColor             = " + util.GetColorString(system.StartColor) + EventData.L);
								sb.Append("  StartPhi               = " + system.StartPhi                        + EventData.L);
								sb.Append("  StartTheta             = " + system.StartTheta                      + EventData.L);
								sb.Append("  SystemRelative         = " + system.SystemRelative                  + EventData.L);
								sb.Append("  Velocity               = " + util.Get3dString(system.Velocity)      + EventData.L);
								sb.Append("  VelocityVariance       = " + system.VelocityVariance                + EventData.L);
							}
							else if (@object as NetDisplayTrail != null)
							{
								sb.Append("is NetDisplayTrail" + EventData.L);

								var trail = @object as NetDisplayTrail;
								sb.Append("  ModelToFollow           = " + trail.ModelToFollow                   + EventData.L);
								sb.Append("  AttachmentPointToFollow = " + trail.AttachmentPointToFollow         + EventData.L);
								sb.Append("  InstanceToFollow        = " + trail.InstanceToFollow                + EventData.L);
								sb.Append("  FadeoutTime             = " + trail.FadeoutTime                     + EventData.L);
								sb.Append("  PointTrail              = " + trail.PointTrail                      + EventData.L);
								sb.Append("  TrailColor              = " + util.GetColorString(trail.TrailColor) + EventData.L);
								sb.Append("  TrailWidth              = " + trail.TrailWidth                      + EventData.L);
								sb.Append("  UpdateFrequency         = " + trail.UpdateFrequency                 + EventData.L);
							}
						}
						else if (@object as NetDisplayProjectedTexture != null) // is NOT 'NetDisplayVisualEffect'
						{
							sb.Append("is NetDisplayProjectedTexture" + EventData.L);

							var texture = @object as NetDisplayProjectedTexture;
							sb.Append("  Color                  = " + util.GetColorString(texture.Color)    + EventData.L);
							sb.Append("  ColorEnd               = " + util.GetColorString(texture.ColorEnd) + EventData.L);
							sb.Append("  DestBlendMode          = " + texture.DestBlendMode                 + EventData.L);
							sb.Append("  FadeInTime             = " + texture.FadeInTime                    + EventData.L);
							sb.Append("  FOV                    = " + texture.FOV                           + EventData.L);
							sb.Append("  FOVEnd                 = " + texture.FOVEnd                        + EventData.L);
							sb.Append("  GroundOnly             = " + texture.GroundOnly                    + EventData.L);
							sb.Append("  Height                 = " + texture.Height                        + EventData.L);
							sb.Append("  HeightEnd              = " + texture.HeightEnd                     + EventData.L);
							sb.Append("  InitialRotation        = " + texture.InitialRotation               + EventData.L);
							sb.Append("  Length                 = " + texture.Length                        + EventData.L);
							sb.Append("  LengthEnd              = " + texture.LengthEnd                     + EventData.L);
							sb.Append("  Lerp                   = " + texture.Lerp                          + EventData.L);
							sb.Append("  LerpPeriod             = " + texture.LerpPeriod                    + EventData.L);
							sb.Append("  ProjectionType         = " + texture.ProjectionType                + EventData.L);
							sb.Append("  RotationalAcceleration = " + texture.RotationalAcceleration        + EventData.L);
							sb.Append("  RotationalVelocity     = " + texture.RotationalVelocity            + EventData.L);
							sb.Append("  SourceBlendMode        = " + texture.SourceBlendMode               + EventData.L);
							sb.Append("  Texture                = " + texture.Texture                       + EventData.L);
							sb.Append("  Width                  = " + texture.Width                         + EventData.L);
							sb.Append("  WidthEnd               = " + texture.WidthEnd                      + EventData.L);
						}
					}
					else if (@object is NetDisplayConnector)
					{
						sb.Append("is NetDisplayConnector" + EventData.L);
					}
					else if (@object is NetDisplayLight)
					{
						sb.Append("is NetDisplayLight" + EventData.L);

						if (@object is NetDisplayLightDirectional)
						{
							sb.Append("is NetDisplayLightDirectional" + EventData.L);
						}
						else if (@object is NetDisplayLightPoint)
						{
							sb.Append("is NetDisplayLightPoint" + EventData.L);
						}
					}
					else if (@object is NetDisplayOrientationModel)
					{
						sb.Append("is NetDisplayOrientationModel" + EventData.L);
					}
					else if (@object is NetDisplayPolygon)
					{
						sb.Append("is NetDisplayPolygon" + EventData.L);
					}
					else if (@object is NetDisplaySound)
					{
						sb.Append("is NetDisplaySound" + EventData.L);
					}
					else if (@object is NetDisplaySphere)
					{
						sb.Append("is NetDisplaySphere" + EventData.L);
					}
					else if (@object is NetDisplayTile)
					{
						sb.Append("is NetDisplayTile" + EventData.L);
					}
				}
				text = sb.ToString();
			}
			tb_Scenedata.Text = text;

			tb_Scenedata.SelectionStart  =
			tb_Scenedata.SelectionLength = 0;
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
