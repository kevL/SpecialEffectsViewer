using System;
using System.Media;
using System.Windows.Forms;


namespace SpecialEffectsViewer
{
	/// <summary>
	/// Static class for searching the effects-list.
	/// </summary>
	static class Search
	{
		/// <summary>
		/// Searches through the effects-list descending or ascending.
		/// </summary>
		/// <param name="effects"></param>
		/// <param name="text"></param>
		/// <param name="descend"></param>
		/// <returns></returns>
		internal static int SearchEffects(ListBox effects, string text, bool descend)
		{
			int id = -1;

			if (!String.IsNullOrEmpty(text))
			{
				int its = effects.Items.Count;
				if (its != 0)
				{
					text = text.ToLower();

					if (descend)
					{
						if (effects.SelectedIndex == its - 1)
						{
							id = 0;
						}
						else
							id = effects.SelectedIndex + 1;

						while (!effects.Items[id].ToString().ToLower().Contains(text))
						{
							if (id == effects.SelectedIndex) // not found.
							{
								SystemSounds.Beep.Play();
								break;
							}

							if (++id == its) // wrap to first node
								id = 0;
						}
					}
					else
					{
						if (effects.SelectedIndex < 1)
						{
							id = its - 1;
						}
						else
							id = effects.SelectedIndex - 1;

						while (!effects.Items[id].ToString().ToLower().Contains(text))
						{
							if (id == effects.SelectedIndex) // not found.
							{
								SystemSounds.Beep.Play();
								break;
							}

							if (--id == -1) // wrap to last node
								id = its - 1;
						}
					}
				}
			}
			return id;
		}
	}
}
