using System;


namespace specialeffectsviewer
{
	/// <summary>
	/// An APpearance Row. (apr)
	/// An object for parsing out a label to show in the Source/Target dropdowns
	/// while retaining a reference to its row-id in Appearance.2da.
	/// </summary>
	sealed class apr
	{
		#region Properties
		/// <summary>
		/// LABEL in Appearance.2da.
		/// </summary>
		internal string Label
		{ get; private set; }

		/// <summary>
		/// Row # in Appearance.2da.
		/// </summary>
		internal int Id
		{ get; private set; }

		/// <summary>
		/// Is feline.
		/// </summary>
		internal bool Fela
		{ get; private set; }

		/// <summary>
		/// Is androgenous or Male.
		/// </summary>
		internal bool Andr
		{ get; private set; }
		#endregion Properties


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="label"></param>
		/// <param name="id"></param>
		/// <param name="fela"></param>
		/// <param name="andr"></param>
		internal apr(string label, int id, bool fela, bool andr)
		{
			Label = label;
			Id    = id;
			Fela  = fela;
			Andr  = andr;
		}
		#endregion cTor


		#region Methods (override)
		/// <summary>
		/// The string that appears in the dropdown.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Id + " - " + Label;
		}
		#endregion Methods (override)
	}
}
