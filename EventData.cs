using System;
using System.Text;

using OEIShared.Effects;


namespace SpecialEffectsViewer
{
	/// <summary>
	/// Sef event data.
	/// </summary>
	static class EventData
	{
		/// <summary>
		/// Gets standard - and extended - event-data as a string.
		/// </summary>
		/// <param name="sefevent"></param>
		/// <param name="id"></param>
		/// <param name="extendinfo"></param>
		/// <returns></returns>
		internal static string GetEventData(ISEFEvent sefevent, int id, bool extendinfo)
		{
			var sb = new StringBuilder();

			sb.Append(id.ToString());
			if (!String.IsNullOrEmpty(sefevent.Name))
				sb.Append(" [" + sefevent.Name + "]");
			sb.Append(util.L);

			string file = GetFileLabel(sefevent);
			if (file != null) sb.Append(file + util.L);

			int p = 6;
			if (extendinfo)
			{
				if      (sefevent as SEFGameModelEffect  != null) p =  7;
				else if (sefevent as SEFLight            != null) p = 12;
				else if (sefevent as SEFProjectedTexture != null) p = 11;
			}

			sb.Append(BwResourceTypes.GetResourceTypeString(sefevent.ResourceType)  + util.L);
			sb.Append(sefevent.EffectType                                           + util.L);
			sb.Append(pad("pos",    p) + util.Get3dString(sefevent.Position)        + util.L);
			sb.Append(pad("orient", p) + (sefevent as SEFEvent).UseOrientedPosition + util.L);
			sb.Append(pad("1st",    p) + sefevent.FirstAttachmentObject             + util.L);
			sb.Append(pad("1st",    p) + sefevent.FirstAttachment                   + util.L);
			sb.Append(pad("2nd",    p) + sefevent.SecondAttachmentObject            + util.L);
			sb.Append(pad("2nd",    p) + sefevent.SecondAttachment                  + util.L);
			sb.Append(pad("delay",  p) + sefevent.Delay                             + util.L);
			sb.Append(pad("dur",    p) + sefevent.HasMaximumDuration);
			if (sefevent.HasMaximumDuration)
				sb.Append(util.L + pad("dur", p) + sefevent.MaximumDuration);

//			if (sefevent.Parent != null)
//			{
//				sb.Append(util.L + "parent - " + sefevent.Parent);
//				sb.Append(util.L + "parent - " + GetPositionString(sefevent.ParentPosition));
//			}


			if (extendinfo)
			{
//				if (sefevent as SEFBeam != null)
//				{
//					// none.
//				}
//				else if (sefevent as SEFBillboard != null)
//				{
//					// none.
//				}
//				else if (sefevent as SEFEvent != null)
//				{
//					// Can a SEFEvent be assigned to a SEFEvent.
//					// SEFEvents *are* SEFEvents ... do not enable this because
//					// it takes precedence over any following types
//				}
				if (sefevent as SEFGameModelEffect != null)
				{
					var modeleffect = sefevent as SEFGameModelEffect;
					sb.Append(util.L + "type    - " + modeleffect.GameModelEffectType);

					string texture = modeleffect.TextureName;
					if (!String.IsNullOrEmpty(texture))
						sb.Append(util.L + "texture - " + texture);

					sb.Append(util.L + "alpha   - " + modeleffect.Alpha);
					sb.Append(util.L + "tint    - " + util.GetColorString(modeleffect.SkinTintColor));
					sb.Append(util.L + "lerpin  - " + modeleffect.LerpInTime);
					sb.Append(util.L + "lerpout - " + modeleffect.LerpOutTime);
				}
				else if (sefevent as SEFLight != null)
				{
					var light = sefevent as SEFLight;
					sb.Append(util.L + "range        - " + light.LightRange);
					sb.Append(util.L + "fadein       - " + light.FadeInTime);

					sb.Append(util.L + "shadow       - " + light.CastsShadow);
					if (light.CastsShadow)
						sb.Append(util.L + "shadow       - " + light.ShadowIntensity);

					sb.Append(util.L + "flicker      - " + light.Flicker);
					if (light.Flicker)
					{
						sb.Append(util.L + "flicker      - " + light.FlickerType);
						sb.Append(util.L + "flicker_rate - " + light.FlickerRate);
						sb.Append(util.L + "flicker_vari - " + light.FlickerVariance);
					}
					sb.Append(util.L + "lerp         - " + light.Lerp);
					if (light.Lerp)
						sb.Append(util.L + "lerp         - " + light.LerpPeriod);

					const string paddin = "               ";

					sb.Append(util.L + "vision       - " + light.VisionEffect);
					sb.Append(util.L + "start        - " + util.SplitLip(light.StartLighting, paddin));
					sb.Append(util.L + "end          - " + util.SplitLip(light.EndLighting,   paddin));
				}
//				else if (sefevent as SEFLightning != null)
//				{
//					// none.
//				}
//				else if (sefevent as SEFLineParticleSystem != null)
//				{
//					// none.
//				}
				else if (sefevent as SEFModel != null)
				{
					var model = sefevent as SEFModel;
					sb.Append(util.L + "skel   - " + model.SkeletonFile);
					sb.Append(util.L + "ani    - " + model.AnimationToPlay);
					sb.Append(util.L + "loop   - " + model.Looping);
					sb.Append(util.L + "tint   - " + model.TintSet);

					string sef = model.SEFToPlayOnModel.ToString();
					if (!String.IsNullOrEmpty(sef))
						sb.Append(util.L + "sef    - " + sef);
//						+ "." + BwResourceTypes.GetResourceTypeString(model.SEFToPlayOnModel.ResourceType); // .sef
				}
				else if (sefevent as SEFParticleMesh != null)
				{
					var mesh = sefevent as SEFParticleMesh;
					var parts = new StringBuilder();
					for (int j = 0; j != mesh.ModelParts.Count; ++j)
					{
						if (parts.Length != 0) parts.Append(util.L + "         ");
						parts.Append(mesh.ModelParts[j].ToString());
					}

					if (parts.Length != 0)
						sb.Append(util.L + "parts  - " + parts);
				}
//				else if (sefevent as SEFParticleSystem != null)
//				{
//					// none.
//				}
				else if (sefevent as SEFProjectedTexture != null)
				{
					var texture = sefevent as SEFProjectedTexture;
					sb.Append(util.L + "texture     - " + texture.Texture);
					sb.Append(util.L + "ground      - " + texture.GroundOnly);
					sb.Append(util.L + "fadein      - " + texture.FadeInTime);
					sb.Append(util.L + "projection  - " + texture.ProjectionType);
					sb.Append(util.L + "orientation - " + util.Get3dString(texture.Orientation));

					sb.Append(util.L + "height      - " + texture.Height);
					if (!FloatsEqual(texture.HeightEnd, texture.Height))
						sb.Append(util.L + "heightend   - " + texture.HeightEnd);
					sb.Append(util.L + "width       - " + texture.Width);
					if (!FloatsEqual(texture.WidthEnd, texture.Width))
						sb.Append(util.L + "widthend    - " + texture.WidthEnd);
					sb.Append(util.L + "length      - " + texture.Length);
					if (!FloatsEqual(texture.LengthEnd, texture.Length))
						sb.Append(util.L + "lengthend   - " + texture.LengthEnd);

					sb.Append(util.L + "lerp        - " + texture.Lerp);
					if (texture.Lerp)
						sb.Append(util.L + "lerp_period - " + texture.LerpPeriod);
					sb.Append(util.L + "color       - " + util.GetColorString(texture.Color));
					if (texture.ColorEnd != texture.Color)
						sb.Append(util.L + "colorend    - " + util.GetColorString(texture.ColorEnd));

					sb.Append(util.L + "rot         - " + texture.InitialRotation);
					sb.Append(util.L + "rot_veloc   - " + texture.RotationalVelocity);
					sb.Append(util.L + "rot_accel   - " + texture.RotationalAcceleration);

					sb.Append(util.L + "fov         - " + texture.FOV);
					if (!FloatsEqual(texture.FOVEnd, texture.FOV))
						sb.Append(util.L + "fovend      - " + texture.FOVEnd);

					sb.Append(util.L + "blend       - " + texture.Blending);
					sb.Append(util.L + "blend_src   - " + texture.SourceBlendMode);
					sb.Append(util.L + "blend_dst   - " + texture.DestBlendMode);
				}
				else if (sefevent as SEFSound != null)
				{
					var sound = sefevent as SEFSound;
					sb.Append(util.L + "loop   - " + sound.SoundLoops);
				}
				else if (sefevent as SEFTrail != null)
				{
					var trail = sefevent as SEFTrail;
					sb.Append(util.L + "width  - " + trail.TrailWidth);
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Gets the label of a definition file for a SEFEvent if one exists.
		/// </summary>
		/// <param name="sefevent"></param>
		/// <returns>null if not found</returns>
		static string GetFileLabel(ISEFEvent sefevent)
		{
			if (   sefevent.DefinitionFile              != null
				&& sefevent.DefinitionFile.ResRef       != null
				&& sefevent.DefinitionFile.ResRef.Value != String.Empty)
			{
				return sefevent.DefinitionFile.ResRef.Value;
			}
			return null;
		}

		/// <summary>
		/// Pads text for the event-info textbox.
		/// </summary>
		/// <param name="in"></param>
		/// <param name="len"></param>
		/// <returns></returns>
		static string pad(string @in, int len)
		{
			while (@in.Length != len)
				@in += " ";

			return @in + " - ";
		}

		/// <summary>
		/// Checks if two floats are reasonably equal.
		/// </summary>
		/// <param name="f1"></param>
		/// <param name="f2"></param>
		/// <returns></returns>
		static bool FloatsEqual(float f1, float f2)
		{
			return Math.Abs(f2 - f1) < 0.00001;
		}
	}
}
