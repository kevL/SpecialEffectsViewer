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
				if (y > -1 && util.checklocation(x,y))
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


		#region eventhandlers
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
		#endregion eventhandlers


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

				sb.Append("objects.Count= " + objects.Count + util.L);

				foreach (NetDisplayObject @object in objects)
				{
					sb.Append(util.L);

					sb.Append(@object + util.L);

//					sb.Append("DisplayName = " + @object.DisplayName                   + util.L);
//					sb.Append("Scene       = " + @object.Scene                         + util.L);
					sb.Append("ID          = " + @object.ID                            + util.L);
					sb.Append("HookedToID  = " + @object.HookedToID                    + util.L);
					sb.Append("UserIndex   = " + @object.UserIndex                     + util.L);
					sb.Append("Position    = " + util.Get3dString(@object.Position)    + util.L);
					sb.Append("Orientation = " + util.Get4dString(@object.Orientation) + util.L);
					sb.Append("Scale       = " + util.Get3dString(@object.Scale)       + util.L);
					sb.Append("Selectable  = " + @object.Selectable                    + util.L);
					sb.Append("Visible     = " + @object.Visible                       + util.L);
					sb.Append("Tag         = " + @object.Tag                           + util.L);

					if (@object as NetDisplayModel != null)
					{
						sb.Append("NetDisplayModel" + util.L);

						var model = @object as NetDisplayModel;
						sb.Append("  Stance             = " + model.Stance                 + util.L);
						sb.Append("  DisplayType        = " + model.GetDisplayType()       + util.L);
						sb.Append("  AffectsWalkmesh    = " + model.AffectsWalkmesh        + util.L);
						if (model.Attachments != null)
							sb.Append("  Attachments.Length = " + model.Attachments.Length + util.L);
						else
							sb.Append("  Attachments        = NULL"                        + util.L);
						if (model.Models != null)
							sb.Append("  Models.Length      = " + model.Models.Length      + util.L);
						else
							sb.Append("  Models             = NULL"                        + util.L);

//						sb.Append("  BaseSkeleton = "  + model.GetBaseSkeletonName()  + util.L);
//						sb.Append("  Skeleton = "      + model.GetSkeletonName()      + util.L);
//						sb.Append("  ModelSlotFile = " + model.GetModelSlotFilename() + util.L);
//						sb.Append("  ModelSlotPart = " + model.GetModelSlotPartName() + util.L);
					}
					else if (@object as NetDisplaySEF != null)
					{
						sb.Append("NetDisplaySEF" + util.L);

						var sef = @object as NetDisplaySEF;
						if (sef.SEF != null)
						{
							sb.Append("  SEFGroup = " + sef.SEF      + util.L);
//							sb.Append("    Name = "   + sef.SEF.Name + util.L);
							// etc.
						}
						else
							sb.Append("  SEFGroup = NULL" + util.L);
					}
					else if (@object as INetDisplayVisualEffect != null)
					{
						sb.Append("INetDisplayVisualEffect" + util.L);

						if (@object as NetDisplayVisualEffect != null)
						{
							sb.Append("NetDisplayVisualEffect" + util.L);

							var visualeffect = @object as NetDisplayVisualEffect;
							sb.Append("  DestinationBlend             = " + visualeffect.DestinationBlend             + util.L);
							sb.Append("  FrameBufferEffect            = " + visualeffect.FrameBufferEffect            + util.L);
							sb.Append("  FrameBufferPixelDisplacement = " + visualeffect.FrameBufferPixelDisplacement + util.L);
							sb.Append("  SourceBlend                  = " + visualeffect.SourceBlend                  + util.L);
							sb.Append("  Texture                      = " + visualeffect.Texture                      + util.L);
							sb.Append("  TextureAnimationSpeed        = " + visualeffect.TextureAnimationSpeed        + util.L);
							sb.Append("  TextureType                  = " + visualeffect.TextureType                  + util.L);

							if (@object as NetDisplayBeam != null)
							{
								sb.Append("NetDisplayBeam" + util.L);

								var beam = @object as NetDisplayBeam;
								sb.Append("  Amplitude      = " + beam.Amplitude                       + util.L);
								sb.Append("  BeginColor     = " + util.GetColorString(beam.BeginColor) + util.L);
								sb.Append("  BeginPosition  = " + util.Get3dString(beam.BeginPosition) + util.L);
								sb.Append("  BeginWidth     = " + beam.BeginWidth                      + util.L);
								sb.Append("  EndColor       = " + util.GetColorString(beam.EndColor)   + util.L);
								sb.Append("  EndPosition    = " + util.Get3dString(beam.EndPosition)   + util.L);
								sb.Append("  EndWidth       = " + beam.EndWidth                        + util.L);
								sb.Append("  Frequency      = " + beam.Frequency                       + util.L);
								sb.Append("  RateOfFlow     = " + beam.RateOfFlow                      + util.L);
								sb.Append("  Repeat         = " + beam.Repeat                          + util.L);
								sb.Append("  RepeatDistance = " + beam.RepeatDistance                  + util.L);
							}
							else if (@object as NetDisplayBillboard != null)
							{
								sb.Append("NetDisplayBillboard" + util.L);

								var billboard = @object as NetDisplayBillboard;
								sb.Append("  AngularVelocity            = " + billboard.AngularVelocity                              + util.L);
								sb.Append("  BeginSize                  = " + util.Get2dString(billboard.BeginSize)                  + util.L);
								sb.Append("  BillboardOrientationVector = " + util.Get3dString(billboard.BillboardOrientationVector) + util.L);
								sb.Append("  EndColor                   = " + util.GetColorString(billboard.EndColor)                + util.L);
								sb.Append("  EndSize                    = " + util.Get2dString(billboard.EndSize)                    + util.L);
								sb.Append("  Lerp                       = " + billboard.Lerp                                         + util.L);
								sb.Append("  LerpPeriod                 = " + billboard.LerpPeriod                                   + util.L);
								sb.Append("  OrientationExplicitMode    = " + billboard.OrientationExplicitMode                      + util.L);
								sb.Append("  OrientationType            = " + billboard.OrientationType                              + util.L);
								sb.Append("  OrientPerpendicular        = " + billboard.OrientPerpendicular                          + util.L);
								sb.Append("  StartColor                 = " + util.GetColorString(billboard.StartColor)              + util.L);
							}
							else if (@object as NetDisplayLightning != null)
							{
								sb.Append("NetDisplayLightning" + util.L);

								var lightning = @object as NetDisplayLightning;
								sb.Append("  BeginBranch       = " + lightning.BeginBranch                     + util.L);
								sb.Append("  BeginColor        = " + util.GetColorString(lightning.BeginColor) + util.L);
								sb.Append("  BeginPosition     = " + util.Get3dString(lightning.BeginPosition) + util.L);
								sb.Append("  BeginWidth        = " + lightning.BeginWidth                      + util.L);
								sb.Append("  BranchProbability = " + lightning.BranchProbability               + util.L);
								sb.Append("  EndBranch         = " + lightning.EndBranch                       + util.L);
								sb.Append("  EndColor          = " + util.GetColorString(lightning.EndColor)   + util.L);
								sb.Append("  EndPosition       = " + util.Get3dString(lightning.EndPosition)   + util.L);
								sb.Append("  EndWidth          = " + lightning.EndWidth                        + util.L);
								sb.Append("  Jaggedness        = " + lightning.Jaggedness                      + util.L);
								sb.Append("  Jumpiness         = " + lightning.Jumpiness                       + util.L);
								sb.Append("  Spread            = " + lightning.Spread                          + util.L);
								sb.Append("  UpdateFrequency   = " + lightning.UpdateFrequency                 + util.L);
							}
							else if (@object as NetDisplayLineParticleSystem != null)
							{
								sb.Append("NetDisplayLineParticleSystem" + util.L);

								var system = @object as NetDisplayLineParticleSystem;
								sb.Append("  Acceleration           = " + util.Get3dString(system.Acceleration)            + util.L);
								sb.Append("  AngularVelocity        = " + system.AngularVelocity                           + util.L);
								sb.Append("  BeginPointBeginColor   = " + util.GetColorString(system.BeginPointBeginColor) + util.L);
								sb.Append("  BeginPointEndColor     = " + util.GetColorString(system.BeginPointEndColor)   + util.L);
								sb.Append("  BeginSize              = " + system.BeginSize                                 + util.L);
								sb.Append("  ConeHalfAngle          = " + system.ConeHalfAngle                             + util.L);
								sb.Append("  ControlType            = " + system.ControlType                               + util.L);
								sb.Append("  EmissionRadius         = " + system.EmissionRadius                            + util.L);
								sb.Append("  EmissionRadiusVariance = " + system.EmissionRadiusVariance                    + util.L);
								sb.Append("  EndPointBeginColor     = " + util.GetColorString(system.EndPointBeginColor)   + util.L);
								sb.Append("  EndPointEndColor       = " + util.GetColorString(system.EndPointEndColor)     + util.L);
								sb.Append("  EndSize                = " + system.EndSize                                   + util.L);
								sb.Append("  FlowType               = " + system.FlowType                                  + util.L);
								sb.Append("  LifetimeVariance       = " + system.LifetimeVariance                          + util.L);
								sb.Append("  ParticleLifetime       = " + system.ParticleLifetime                          + util.L);
								sb.Append("  SizeVariance           = " + system.SizeVariance                              + util.L);
								sb.Append("  SpawnRate              = " + system.SpawnRate                                 + util.L);
								sb.Append("  StartPhi               = " + system.StartPhi                                  + util.L);
								sb.Append("  StartTheta             = " + system.StartTheta                                + util.L);
								sb.Append("  SystemRelative         = " + system.SystemRelative                            + util.L);
								sb.Append("  Velocity               = " + util.Get3dString(system.Velocity)                + util.L);
								sb.Append("  VelocityVariance       = " + system.VelocityVariance                          + util.L);
							}
							else if (@object as NetDisplayParticleSystem != null)
							{
								sb.Append("NetDisplayParticleSystem" + util.L);

								var system = @object as NetDisplayParticleSystem;
								sb.Append("  Acceleration           = " + util.Get3dString(system.Acceleration)  + util.L);
								sb.Append("  AngularVelocity        = " + system.AngularVelocity                 + util.L);
								sb.Append("  BeginSize              = " + util.Get2dString(system.BeginSize)     + util.L);
								sb.Append("  ConeHalfAngle          = " + system.ConeHalfAngle                   + util.L);
								sb.Append("  ControlType            = " + system.ControlType                     + util.L);
								sb.Append("  Disk                   = " + system.Disk                            + util.L);
								sb.Append("  EmissionRadius         = " + system.EmissionRadius                  + util.L);
								sb.Append("  EmissionRadiusVariance = " + system.EmissionRadiusVariance          + util.L);
								sb.Append("  EmissionSlot           = " + system.EmissionSlot                    + util.L);
								sb.Append("  EmitFromMesh           = " + system.EmitFromMesh                    + util.L);
								sb.Append("  EndColor               = " + util.GetColorString(system.EndColor)   + util.L);
								sb.Append("  EndSize                = " + util.Get2dString(system.EndSize)       + util.L);
								sb.Append("  FlowType               = " + system.FlowType                        + util.L);
								sb.Append("  GravitateTowardsCenter = " + system.GravitateTowardsCenter          + util.L);
								sb.Append("  LifetimeVariance       = " + system.LifetimeVariance                + util.L);
								sb.Append("  ModelForEmission       = " + system.ModelForEmission                + util.L);
								sb.Append("  OrientationType        = " + system.OrientationType                 + util.L);
								sb.Append("  ParticleLifetime       = " + system.ParticleLifetime                + util.L);
								sb.Append("  SizeVariance           = " + system.SizeVariance                    + util.L);
								sb.Append("  SpawnRate              = " + system.SpawnRate                       + util.L);
								sb.Append("  StartAngleVariance     = " + system.StartAngleVariance              + util.L);
								sb.Append("  StartColor             = " + util.GetColorString(system.StartColor) + util.L);
								sb.Append("  StartPhi               = " + system.StartPhi                        + util.L);
								sb.Append("  StartTheta             = " + system.StartTheta                      + util.L);
								sb.Append("  SystemRelative         = " + system.SystemRelative                  + util.L);
								sb.Append("  Velocity               = " + util.Get3dString(system.Velocity)      + util.L);
								sb.Append("  VelocityVariance       = " + system.VelocityVariance                + util.L);
							}
							else if (@object as NetDisplayTrail != null)
							{
								sb.Append("NetDisplayTrail" + util.L);

								var trail = @object as NetDisplayTrail;
								sb.Append("  ModelToFollow           = " + trail.ModelToFollow                   + util.L);
								sb.Append("  AttachmentPointToFollow = " + trail.AttachmentPointToFollow         + util.L);
								sb.Append("  InstanceToFollow        = " + trail.InstanceToFollow                + util.L);
								sb.Append("  FadeoutTime             = " + trail.FadeoutTime                     + util.L);
								sb.Append("  PointTrail              = " + trail.PointTrail                      + util.L);
								sb.Append("  TrailColor              = " + util.GetColorString(trail.TrailColor) + util.L);
								sb.Append("  TrailWidth              = " + trail.TrailWidth                      + util.L);
								sb.Append("  UpdateFrequency         = " + trail.UpdateFrequency                 + util.L);
							}
						}
						else if (@object as NetDisplayProjectedTexture != null) // is NOT 'NetDisplayVisualEffect'
						{
							sb.Append("NetDisplayProjectedTexture" + util.L);

							var texture = @object as NetDisplayProjectedTexture;
							sb.Append("  Color                  = " + util.GetColorString(texture.Color)    + util.L);
							sb.Append("  ColorEnd               = " + util.GetColorString(texture.ColorEnd) + util.L);
							sb.Append("  DestBlendMode          = " + texture.DestBlendMode                 + util.L);
							sb.Append("  FadeInTime             = " + texture.FadeInTime                    + util.L);
							sb.Append("  FOV                    = " + texture.FOV                           + util.L);
							sb.Append("  FOVEnd                 = " + texture.FOVEnd                        + util.L);
							sb.Append("  GroundOnly             = " + texture.GroundOnly                    + util.L);
							sb.Append("  Height                 = " + texture.Height                        + util.L);
							sb.Append("  HeightEnd              = " + texture.HeightEnd                     + util.L);
							sb.Append("  InitialRotation        = " + texture.InitialRotation               + util.L);
							sb.Append("  Length                 = " + texture.Length                        + util.L);
							sb.Append("  LengthEnd              = " + texture.LengthEnd                     + util.L);
							sb.Append("  Lerp                   = " + texture.Lerp                          + util.L);
							sb.Append("  LerpPeriod             = " + texture.LerpPeriod                    + util.L);
							sb.Append("  ProjectionType         = " + texture.ProjectionType                + util.L);
							sb.Append("  RotationalAcceleration = " + texture.RotationalAcceleration        + util.L);
							sb.Append("  RotationalVelocity     = " + texture.RotationalVelocity            + util.L);
							sb.Append("  SourceBlendMode        = " + texture.SourceBlendMode               + util.L);
							sb.Append("  Texture                = " + texture.Texture                       + util.L);
							sb.Append("  Width                  = " + texture.Width                         + util.L);
							sb.Append("  WidthEnd               = " + texture.WidthEnd                      + util.L);
						}
					}
					else if (@object is NetDisplayConnector)
					{
						sb.Append("NetDisplayConnector" + util.L);
					}
					else if (@object as NetDisplayLight != null)
					{
						sb.Append("NetDisplayLight" + util.L);

						const string pad = "                    ";

						var light = @object as NetDisplayLight;
						sb.Append("  CastsShadow     = " + light.CastsShadow                         + util.L);
						sb.Append("  Color           = " + util.SplitLip(light.Color, pad)           + util.L);
						sb.Append("  Flicker         = " + light.Flicker                             + util.L);
						sb.Append("  FlickerRate     = " + light.FlickerRate                         + util.L);
						sb.Append("  FlickerType     = " + light.FlickerType                         + util.L);
						sb.Append("  FlickerVariance = " + light.FlickerVariance                     + util.L);
						sb.Append("  Lerp            = " + light.Lerp                                + util.L);
						sb.Append("  LerpPeriod      = " + light.LerpPeriod                          + util.L);
						sb.Append("  LerpTargetColor = " + util.SplitLip(light.LerpTargetColor, pad) + util.L);
						sb.Append("  ShadowIntensity = " + light.ShadowIntensity                     + util.L);
						sb.Append("  SwitchOn        = " + light.SwitchOn                            + util.L);

						if (@object as NetDisplayLightDirectional != null)
						{
							sb.Append("NetDisplayLightDirectional" + util.L);

							var directional = @object as NetDisplayLightDirectional;
							sb.Append("  DirectionX = " + directional.DirectionX + util.L);
							sb.Append("  DirectionY = " + directional.DirectionY + util.L);
							sb.Append("  DirectionZ = " + directional.DirectionZ + util.L);
						}
						else if (@object as NetDisplayLightPoint != null)
						{
							sb.Append("NetDisplayLightPoint" + util.L);

							var point = @object as NetDisplayLightPoint;
							sb.Append("  Range = " + point.Range + util.L);
						}
					}
					else if (@object is NetDisplayOrientationModel)
					{
						sb.Append("NetDisplayOrientationModel" + util.L);
					}
					else if (@object is NetDisplayPolygon)
					{
						sb.Append("NetDisplayPolygon" + util.L);
					}
					else if (@object is NetDisplaySound)
					{
						sb.Append("NetDisplaySound" + util.L);
					}
					else if (@object is NetDisplaySphere)
					{
						sb.Append("NetDisplaySphere" + util.L);
					}
					else if (@object is NetDisplayTile)
					{
						sb.Append("NetDisplayTile" + util.L);
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
