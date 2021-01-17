using System;

using Microsoft.DirectX;

using OEIShared.Effects;


namespace SpecialEffectsViewer
{
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
			string text = string.Empty;

			text += id.ToString();
			if (!String.IsNullOrEmpty(sefevent.Name))
				text += " [" + sefevent.Name + "]";
			text += L;

			string file = GetFileLabel(sefevent);
			if (file != null) text += file + L;

			int p = 6;
			if (extendinfo)
			{
				if      (sefevent as SEFGameModelEffect  != null) p =  7;
				else if (sefevent as SEFLight            != null) p = 12;
				else if (sefevent as SEFProjectedTexture != null) p = 11;
			}

			text += BwResourceTypes.GetResourceTypeString(sefevent.ResourceType)  + L;
			text += sefevent.EffectType                                           + L;
			text += pad("pos",    p) + GetPositionString(sefevent.Position)       + L;
			text += pad("orient", p) + (sefevent as SEFEvent).UseOrientedPosition + L;
			text += pad("1st",    p) + sefevent.FirstAttachmentObject             + L;
			text += pad("1st",    p) + sefevent.FirstAttachment                   + L;
			text += pad("2nd",    p) + sefevent.SecondAttachmentObject            + L;
			text += pad("2nd",    p) + sefevent.SecondAttachment                  + L;
			text += pad("delay",  p) + sefevent.Delay                             + L;
			text += pad("dur",    p) + sefevent.HasMaximumDuration;
			if (sefevent.HasMaximumDuration)
				text += L + pad("dur", p) + sefevent.MaximumDuration;

//			if (sefevent.Parent != null)
//			{
//				text += L + "parent - " + sefevent.Parent;
//				text += L + "parent - " + GetPositionString(sefevent.ParentPosition);
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
					var modeleffect = (sefevent as SEFGameModelEffect);
					text += L + "type    - " + modeleffect.GameModelEffectType;

					string texture = modeleffect.TextureName.ToString();
					if (!String.IsNullOrEmpty(texture))
						text += L + "texture - " + texture;

					text += L + "alpha   - " + modeleffect.Alpha;
					text += L + "tint    - " + modeleffect.SkinTintColor;
					text += L + "lerpin  - " + modeleffect.LerpInTime;
					text += L + "lerpout - " + modeleffect.LerpOutTime;
				}
				else if (sefevent as SEFLight != null)
				{
					var light = (sefevent as SEFLight);
					text += L + "range        - " + light.LightRange;
					text += L + "fadein       - " + light.FadeInTime;

					text += L + "shadow       - " + light.CastsShadow;
					if (light.CastsShadow)
						text += L + "shadow       - " + light.ShadowIntensity;

					text += L + "flicker      - " + light.Flicker;
					if (light.Flicker)
					{
						text += L + "flicker      - " + light.FlickerType;
						text += L + "flicker_rate - " + light.FlickerRate;
						text += L + "flicker_vari - " + light.FlickerVariance;
					}
					text += L + "lerp         - " + light.Lerp;
					if (light.Lerp)
						text += L + "lerp         - " + light.LerpPeriod;

					text += L + "vision       - " + light.VisionEffect;
					text += L + "start        - " + SplitLip(light.StartLighting);
					text += L + "end          - " + SplitLip(light.EndLighting);
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
					var model = (sefevent as SEFModel);
					text += L + "skel   - " + model.SkeletonFile;
					text += L + "ani    - " + model.AnimationToPlay;
					text += L + "loop   - " + model.Looping;
					text += L + "tint   - " + model.TintSet;

					string sef = model.SEFToPlayOnModel.ToString();
					if (!String.IsNullOrEmpty(sef))
						text += L + "sef    - " + sef;
//						+ "." + BwResourceTypes.GetResourceTypeString(model.SEFToPlayOnModel.ResourceType); // .sef
				}
				else if (sefevent as SEFParticleMesh != null)
				{
					var mesh = (sefevent as SEFParticleMesh);
					string parts = String.Empty;
					for (int j = 0; j != mesh.ModelParts.Count; ++j)
					{
						if (!String.IsNullOrEmpty(parts)) parts += L + "         ";
						parts += mesh.ModelParts[j].ToString();
					}

					if (parts != String.Empty)
						text += L + "parts  - " + parts;
				}
//				else if (sefevent as SEFParticleSystem != null)
//				{
//					// none.
//				}
				else if (sefevent as SEFProjectedTexture != null)
				{
					var texture = (sefevent as SEFProjectedTexture);
					text += L + "texture     - " + texture.Texture;
					text += L + "ground      - " + texture.GroundOnly;
					text += L + "fadein      - " + texture.FadeInTime;
					text += L + "projection  - " + texture.ProjectionType;
					text += L + "orientation - " + GetPositionString(texture.Orientation);

					text += L + "height      - " + texture.Height;
					if (!FloatsEqual(texture.HeightEnd, texture.Height))
						text += L + "heightend   - " + texture.HeightEnd;
					text += L + "width       - " + texture.Width;
					if (!FloatsEqual(texture.WidthEnd, texture.Width))
						text += L + "widthend    - " + texture.WidthEnd;
					text += L + "length      - " + texture.Length;
					if (!FloatsEqual(texture.LengthEnd, texture.Length))
						text += L + "lengthend   - " + texture.LengthEnd;

					text += L + "lerp        - " + texture.Lerp;
					if (texture.Lerp)
						text += L + "lerp_period - " + texture.LerpPeriod;
					text += L + "color       - " + texture.Color;
					if (texture.ColorEnd != texture.Color)
						text += L + "colorend    - " + texture.ColorEnd;

					text += L + "rot         - " + texture.InitialRotation;
					text += L + "rot_veloc   - " + texture.RotationalVelocity;
					text += L + "rot_accel   - " + texture.RotationalAcceleration;

					text += L + "fov         - " + texture.FOV;
					if (!FloatsEqual(texture.FOVEnd, texture.FOV))
						text += L + "fovend      - " + texture.FOVEnd;

					text += L + "blend       - " + texture.Blending;
					text += L + "blend_src   - " + texture.SourceBlendMode;
					text += L + "blend_dst   - " + texture.DestBlendMode;
				}
				else if (sefevent as SEFSound != null)
				{
					var sound = (sefevent as SEFSound);
					text += L + "loop   - " + sound.SoundLoops;
				}
				else if (sefevent as SEFTrail != null)
				{
					var trail = (sefevent as SEFTrail);
					text += L + "width  - " + trail.TrailWidth;
				}
			}

			return text;
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
		/// Gets a string for a 3d-vector.
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		internal static string GetPositionString(Vector3 vec)
		{
			return vec.X + "," + vec.Y + "," + vec.Z;
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
