using System;
using System.IO;
using System.Windows.Forms;


namespace specialeffectsviewer
{
	static class logger
	{
		const string Logfile = "SpecialEffectsViewer.log";

		/// <summary>
		/// Creates a logfile (overwrites the previous logfile if it exists).
		/// </summary>
		public static void create()
		{
			string pfe = Path.Combine(Application.StartupPath, Logfile);
			using (var sw = new StreamWriter(File.Open(pfe,
													   FileMode.Create,
													   FileAccess.Write,
													   FileShare.None)))
			{}
		}

		/// <summary>
		/// Writes a line to the logfile.
		/// </summary>
		/// <param name="line">the line to write</param>
		public static void log(string line = "")
		{
			string pfe = Path.Combine(Application.StartupPath, Logfile);
			using (var sw = new StreamWriter(File.Open(pfe,
													   FileMode.Append,
													   FileAccess.Write,
													   FileShare.None)))
			{
				sw.WriteLine(line);
			}
		}
	}
}
