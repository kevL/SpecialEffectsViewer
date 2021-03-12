using System;
using System.Windows.Forms;


namespace specialeffectsviewer
{
	/// <summary>
	/// Static class for searching the effects-list.
	/// </summary>
	static class Search
	{
		/// <summary>
		/// Searches through the effects-list descending or ascending.
		/// </summary>
		/// <param name="effects">the effects-list</param>
		/// <param name="text">the text to search for</param>
		/// <param name="descend">true if search-descending, false if ascending</param>
		/// <param name="wholeword">true for wholeword, false for substring</param>
		/// <returns>the effect-id found or -1 if not found</returns>
		/// <remarks>The search is case-insensitive.</remarks>
		internal static int SearchEffects(ListBox effects, string text, bool descend, bool wholeword = false)
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

						string label = effects.Items[id].ToString().ToLower();
						while (( wholeword || !label.Contains(text))
							&& (!wholeword ||  label != text))
						{
							if (id == effects.SelectedIndex) // not found.
							{
								id = -1;
								break;
							}

							if (++id == its) // wrap to first node
							{
								if (effects.SelectedIndex == -1)
								{
									id = -1;
									break;
								}
								id = 0;
							}
							label = effects.Items[id].ToString().ToLower();
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

						string label = effects.Items[id].ToString().ToLower();
						while (( wholeword || !label.Contains(text))
							&& (!wholeword ||  label != text))
						{
							if (id == effects.SelectedIndex) // not found.
							{
								id = -1;
								break;
							}

							if (--id == -1) // wrap to last node
							{
								if (effects.SelectedIndex == -1)
								{
									id = -1;
									break;
								}
								id = its - 1;
							}
							label = effects.Items[id].ToString().ToLower();
						}
					}
				}
			}

			return id;
		}
	}
}
