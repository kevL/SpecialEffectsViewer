using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;


namespace SpecialEffectsViewer
{
	sealed class AboutF
		: Form
	{
		#region Fields (static)
		static string Info;
		#endregion Fields (static)


		#region Fields
		string L = Environment.NewLine;
		#endregion Fields


		#region cTor
		internal AboutF()
		{
			InitializeComponent();

			if (Info == null)
			{
				Info = "Special Effects Viewer"                    + L
					 + "a toolset plugin for Neverwinter Nights 2" + L + L
					 + "code by kevL's"                            + L
					 + "credit codepoetz et al."                   + L + L;

				var ass = Assembly.GetExecutingAssembly();
				var an = ass.GetName();
				Info += an.Version.Major + "."
					  + an.Version.Minor + "."
					  + an.Version.Build + "."
					  + an.Version.Revision;
#if DEBUG
				Info += " debug";
#else
				Info += " release";
#endif
				Info += Environment.NewLine
					  + String.Format(CultureInfo.CurrentCulture,
									  "{0:yyyy MMM d} {0:HH}:{0:mm}:{0:ss} UTC",
									  ass.GetLinkerTime());
				Info += L;
			}

			tb_about.Text = Info;

			tb_about.SelectionStart  =
			tb_about.SelectionLength = 0;
		}
		#endregion cTor


		#region Handlers (override)
		protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case Keys.Escape:
				case Keys.Enter:
				case Keys.F2:
					Close();
					break;
			}
		}
		#endregion Handlers (override)



		#region Designer
		TextBox tb_about;


		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The
		/// Forms designer might not be able to load this method if it was
		/// changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.tb_about = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// tb_about
			// 
			this.tb_about.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tb_about.Location = new System.Drawing.Point(0, 0);
			this.tb_about.Margin = new System.Windows.Forms.Padding(0);
			this.tb_about.Multiline = true;
			this.tb_about.Name = "tb_about";
			this.tb_about.ReadOnly = true;
			this.tb_about.Size = new System.Drawing.Size(292, 114);
			this.tb_about.TabIndex = 0;
			this.tb_about.WordWrap = false;
			// 
			// AboutF
			// 
			this.ClientSize = new System.Drawing.Size(292, 114);
			this.Controls.Add(this.tb_about);
			this.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutF";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "About";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion Designer
	}



	/// <summary>
	/// Lifted from StackOverflow.com:
	/// https://stackoverflow.com/questions/1600962/displaying-the-build-date#answer-1600990
	/// - what a fucking pain in the ass.
	/// </summary>
	static class DateTimeExtension
	{
		/// <summary>
		/// Gets the time/date of build timestamp.
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		internal static DateTime GetLinkerTime(this Assembly assembly, TimeZoneInfo target = null)
		{
			var filePath = assembly.Location;
			const int c_PeHeaderOffset = 60;
			const int c_LinkerTimestampOffset = 8;

			var buffer = new byte[2048];

			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				stream.Read(buffer, 0, 2048);

			var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
			var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			return epoch.AddSeconds(secondsSince1970);
		}
	}
}
