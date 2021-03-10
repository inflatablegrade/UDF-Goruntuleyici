using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO.Compression;


namespace WindowsFormsApp1
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			string sourceContent = string.Empty;
			string sourcePath = string.Empty;
			string curTimestamp = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString();
			Console.WriteLine(curTimestamp);
				
			using (OpenFileDialog sourceDialog = new OpenFileDialog()) {
				sourceDialog.Filter = "UDF files (*.udf)|*.udf|All files (*.*)|*.*";
				sourceDialog.RestoreDirectory = true;				

				if (sourceDialog.ShowDialog() == DialogResult.OK)
				{
					sourcePath = sourceDialog.FileName;
					string[] segments = sourcePath.Split('\\');
					string fileName = segments[segments.Length-1];
					string[] periodSegments = fileName.Split('.');
					string firstPart = periodSegments[0];
					string folderPath = sourcePath.Substring(0, sourcePath.Length - fileName.Length);					

					Stream stream = sourceDialog.OpenFile();
					Encoding encoding = Encoding.GetEncoding(1254);

					using (StreamReader reader = new StreamReader(stream, encoding))
					{
						sourceContent = reader.ReadToEnd();

						FileStream targetStream = new FileStream(folderPath + firstPart + "-" + curTimestamp + ".zip", FileMode.CreateNew);
						using (StreamWriter writer = new StreamWriter(targetStream, encoding))
						{
							writer.Write(sourceContent);
						}
					}

					stream.Dispose();

					//					using (ArchiveFile archiveFile = new ArchiveFile(folderPath + firstPart + "-" + curTimestamp + ".zip"))
					//					{
					//						archiveFile.Extract(folderPath + firstPart + "-" + curTimestamp); // extract all
					//					}
					ZipFile.ExtractToDirectory(folderPath + firstPart + "-" + curTimestamp + ".zip", folderPath + firstPart + "-" + curTimestamp);


					Dictionary<string, string[]> styles = XMLHandler.GetStyleProperties(folderPath + firstPart + "-" + curTimestamp + "\\" + "content.xml");
                    string rawContent = XMLHandler.GetRawContent(folderPath + firstPart + "-" + curTimestamp + "\\" + "content.xml");
					string res = XMLHandler.HandleXmlFormatting(folderPath + firstPart + "-" + curTimestamp + "\\" + "content.xml", rawContent, styles);

					webBrowser1.DocumentText = res;

					File.Delete(folderPath + firstPart + "-" + curTimestamp + ".zip");
					Directory.Delete(folderPath + firstPart + "-" + curTimestamp, true);
				}
			}
		}
	}
}
