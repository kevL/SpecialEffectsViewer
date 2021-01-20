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
		internal static string L = Environment.NewLine;

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
			sb.Append(L);

			string file = GetFileLabel(sefevent);
			if (file != null) sb.Append(file + L);

			int p = 6;
			if (extendinfo)
			{
				if      (sefevent as SEFGameModelEffect  != null) p =  7;
				else if (sefevent as SEFLight            != null) p = 12;
				else if (sefevent as SEFProjectedTexture != null) p = 11;
			}

			sb.Append(BwResourceTypes.GetResourceTypeString(sefevent.ResourceType)  + L);
			sb.Append(sefevent.EffectType                                           + L);
			sb.Append(pad("pos",    p) + util.Get3dString(sefevent.Position)        + L);
			sb.Append(pad("orient", p) + (sefevent as SEFEvent).UseOrientedPosition + L);
			sb.Append(pad("1st",    p) + sefevent.FirstAttachmentObject             + L);
			sb.Append(pad("1st",    p) + sefevent.FirstAttachment                   + L);
			sb.Append(pad("2nd",    p) + sefevent.SecondAttachmentObject            + L);
			sb.Append(pad("2nd",    p) + sefevent.SecondAttachment                  + L);
			sb.Append(pad("delay",  p) + sefevent.Delay                             + L);
			sb.Append(pad("dur",    p) + sefevent.HasMaximumDuration);
			if (sefevent.HasMaximumDuration)
				sb.Append(L + pad("dur", p) + sefevent.MaximumDuration);

//			if (sefevent.Parent != null)
//			{
//				sb.Append(L + "parent - " + sefevent.Parent);
//				sb.Append(L + "parent - " + GetPositionString(sefevent.ParentPosition));
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
					sb.Append(L + "type    - " + modeleffect.GameModelEffectType);

					string texture = modeleffect.TextureName;
					if (!String.IsNullOrEmpty(texture))
						sb.Append(L + "texture - " + texture);

					sb.Append(L + "alpha   - " + modeleffect.Alpha);
					sb.Append(L + "tint    - " + util.GetColorString(modeleffect.SkinTintColor));
					sb.Append(L + "lerpin  - " + modeleffect.LerpInTime);
					sb.Append(L + "lerpout - " + modeleffect.LerpOutTime);
				}
				else if (sefevent as SEFLight != null)
				{
					var light = sefevent as SEFLight;
					sb.Append(L + "range        - " + light.LightRange);
					sb.Append(L + "fadein       - " + light.FadeInTime);

					sb.Append(L + "shadow       - " + light.CastsShadow);
					if (light.CastsShadow)
						sb.Append(L + "shadow       - " + light.ShadowIntensity);

					sb.Append(L + "flicker      - " + light.Flicker);
					if (light.Flicker)
					{
						sb.Append(L + "flicker      - " + light.FlickerType);
						sb.Append(L + "flicker_rate - " + light.FlickerRate);
						sb.Append(L + "flicker_vari - " + light.FlickerVariance);
					}
					sb.Append(L + "lerp         - " + light.Lerp);
					if (light.Lerp)
						sb.Append(L + "lerp         - " + light.LerpPeriod);

					sb.Append(L + "vision       - " + light.VisionEffect);
					sb.Append(L + "start        - " + SplitLip(light.StartLighting));
					sb.Append(L + "end          - " + SplitLip(light.EndLighting));
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
					sb.Append(L + "skel   - " + model.SkeletonFile);
					sb.Append(L + "ani    - " + model.AnimationToPlay);
					sb.Append(L + "loop   - " + model.Looping);
					sb.Append(L + "tint   - " + model.TintSet);

					string sef = model.SEFToPlayOnModel.ToString();
					if (!String.IsNullOrEmpty(sef))
						sb.Append(L + "sef    - " + sef);
//						+ "." + BwResourceTypes.GetResourceTypeString(model.SEFToPlayOnModel.ResourceType); // .sef
				}
				else if (sefevent as SEFParticleMesh != null)
				{
					var mesh = sefevent as SEFParticleMesh;
					var parts = new StringBuilder();
					for (int j = 0; j != mesh.ModelParts.Count; ++j)
					{
						if (parts.Length != 0) parts.Append(L + "         ");
						parts.Append(mesh.ModelParts[j].ToString());
					}

					if (parts.Length != 0)
						sb.Append(L + "parts  - " + parts);
				}
//				else if (sefevent as SEFParticleSystem != null)
//				{
//					// none.
//				}
				else if (sefevent as SEFProjectedTexture != null)
				{
					var texture = sefevent as SEFProjectedTexture;
					sb.Append(L + "texture     - " + texture.Texture);
					sb.Append(L + "ground      - " + texture.GroundOnly);
					sb.Append(L + "fadein      - " + texture.FadeInTime);
					sb.Append(L + "projection  - " + texture.ProjectionType);
					sb.Append(L + "orientation - " + util.Get3dString(texture.Orientation));

					sb.Append(L + "height      - " + texture.Height);
					if (!FloatsEqual(texture.HeightEnd, texture.Height))
						sb.Append(L + "heightend   - " + texture.HeightEnd);
					sb.Append(L + "width       - " + texture.Width);
					if (!FloatsEqual(texture.WidthEnd, texture.Width))
						sb.Append(L + "widthend    - " + texture.WidthEnd);
					sb.Append(L + "length      - " + texture.Length);
					if (!FloatsEqual(texture.LengthEnd, texture.Length))
						sb.Append(L + "lengthend   - " + texture.LengthEnd);

					sb.Append(L + "lerp        - " + texture.Lerp);
					if (texture.Lerp)
						sb.Append(L + "lerp_period - " + texture.LerpPeriod);
					sb.Append(L + "color       - " + util.GetColorString(texture.Color));
					if (texture.ColorEnd != texture.Color)
						sb.Append(L + "colorend    - " + util.GetColorString(texture.ColorEnd));

					sb.Append(L + "rot         - " + texture.InitialRotation);
					sb.Append(L + "rot_veloc   - " + texture.RotationalVelocity);
					sb.Append(L + "rot_accel   - " + texture.RotationalAcceleration);

					sb.Append(L + "fov         - " + texture.FOV);
					if (!FloatsEqual(texture.FOVEnd, texture.FOV))
						sb.Append(L + "fovend      - " + texture.FOVEnd);

					sb.Append(L + "blend       - " + texture.Blending);
					sb.Append(L + "blend_src   - " + texture.SourceBlendMode);
					sb.Append(L + "blend_dst   - " + texture.DestBlendMode);
				}
				else if (sefevent as SEFSound != null)
				{
					var sound = sefevent as SEFSound;
					sb.Append(L + "loop   - " + sound.SoundLoops);
				}
				else if (sefevent as SEFTrail != null)
				{
					var trail = sefevent as SEFTrail;
					sb.Append(L + "width  - " + trail.TrailWidth);
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
		/// Splits a LightIntensityPair.
		/// </summary>
		/// <param name="pair"></param>
		/// <returns></returns>
		static string SplitLip(object pair)
		{
			string diff = pair.ToString();

			int pos = diff.IndexOf("Intensity", StringComparison.Ordinal);
			string inte = diff.Substring(pos);

			diff = diff.Substring(0, diff.Length - inte.Length - 2);
			pos = diff.IndexOf("Ambient", StringComparison.Ordinal);
			string ambi = diff.Substring(pos);

			diff = diff.Substring(0, diff.Length - ambi.Length - 2);
			pos = diff.IndexOf("Specular", StringComparison.Ordinal);
			string spec = diff.Substring(pos);

			diff = diff.Substring(0, diff.Length - spec.Length - 2);

			return diff                     + L
				 + "               " + spec + L
				 + "               " + ambi + L
				 + "               " + inte;
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
