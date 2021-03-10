using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualBasic;

namespace WindowsFormsApp1
{
	class XMLHandler
	{
		public static string GetRawContent(string path)
		{
			XmlReader contentReader = XmlReader.Create(path);
			contentReader.MoveToContent();
			contentReader.ReadToDescendant("content");
			string textContent = contentReader.ReadElementContentAsString();
			int len = textContent.Length;
//			textContent = textContent.Substring(9, len - 12);
			contentReader.Dispose();
			return textContent;
		}

		public static string GetValueRegardless(XmlReader reader, string attribute, string defaultValue)
		{
			string result = defaultValue;
			if (reader.GetAttribute(attribute) != null)
			{
				result = reader.GetAttribute(attribute);
			}
			return result;
		}

		public static bool GetFinalValue (string first, string second)
		{
			bool result = false;
			if (first.Equals(second))
			{
				if (first == "true")
				{
					result = true;
				}
			}
			else
			{
				result = true;
			}
			return result;
		}

		public static Dictionary<string, string[]> GetStyleProperties(string path)
		{
			XmlReader contentReader = XmlReader.Create(path);
			contentReader.MoveToContent();
			contentReader.ReadToDescendant("content");
			contentReader.ReadToNextSibling("elements");
			contentReader.ReadToNextSibling("styles");
			bool styleStart = false;
//			Console.WriteLine("Styles node: " + contentReader.Name);
			Dictionary<string, string[]> styles = new Dictionary<string, string[]>();

			while (contentReader.Read())
			{
				if (contentReader.Name == "style")
				{
					styleStart = true;
					string name = contentReader.GetAttribute("name");
					string isBold = GetValueRegardless(contentReader, "bold", "false");
					string isItalic = GetValueRegardless(contentReader, "italic", "false");
					string isUnderlined = GetValueRegardless(contentReader, "underlined", "false");
					string font = GetValueRegardless(contentReader, "family", "Times New Roman");
					string size = GetValueRegardless(contentReader, "size", "11px"); ;
					string alignment = GetValueRegardless(contentReader, "Alignment", "0");
					string color = GetValueRegardless(contentReader, "foreground", "#000000");
					string bgColor = GetValueRegardless(contentReader, "background", "#ffffff");

					string[] style = {isBold, isItalic, isUnderlined, font, size, alignment, color , bgColor };
					styles.Add(name, style);
				}
				else if (contentReader.Name != "style" && styleStart == true)
				{
					break;
				}
			}
			contentReader.Dispose();
			return styles;
		}

		public static string HandleXmlFormatting(string path, string textContent, Dictionary<string, string[]> styles)
		{
//			XmlReaderSettings settings = new XmlReaderSettings();
//			settings.DtdProcessing = DtdProcessing.Parse;
			XmlReader contentReader = XmlReader.Create(path);
			contentReader.MoveToContent(); //properties
			contentReader.ReadToDescendant("content");
			contentReader.ReadToNextSibling("elements");
//			contentReader.ReadToNextSibling("properties");

			int numParagraphs = textContent.Split('\r').Length;

			int curOffset = 0;
			int curLength = 0;

			Console.WriteLine("first:" + contentReader.Name); // content

			string textData = "";

			bool paragraphOpening = false;

			while (contentReader.Read())
			{
				Console.WriteLine(contentReader.Name); //names

				if (contentReader.GetAttribute("startOffset") != null)
				{
					string curStyleName = contentReader.GetAttribute("resolver");

					if(contentReader.GetAttribute("resolver") == null)
					{
						curStyleName = "hvl-default";
					}

					string[] curStyleData = { };
					styles.TryGetValue(curStyleName, out curStyleData);
//					Console.WriteLine("Number of style properties: " + curStyleData.Length);

					string elementBold = "false";
					string elementItalic = "false";
					string elementUnderlined = "false";

					string font = curStyleData[3];
					string size = curStyleData[4];
					string alignment = curStyleData[5];
					string textColor = curStyleData[6];
					string bgColor = curStyleData[7];

					int textColorVal = 0;
					int bgColorVal = 0;

					bool convertedOffset = Int32.TryParse(contentReader.GetAttribute("startOffset"), out curOffset);
					bool convertedLength = Int32.TryParse(contentReader.GetAttribute("length"), out curLength);

					

					if (contentReader.Name == "content" || contentReader.Name == "field")
					{
						if (contentReader.GetAttribute("bold") == "true")
						{
							elementBold = "true";
						}
						if (contentReader.GetAttribute("italic") == "true")
						{
							elementItalic = "true";
						}
						if (contentReader.GetAttribute("underlined") == "true")
						{
							elementUnderlined = "true";
						}
						if (contentReader.GetAttribute("family") != null)
						{
							font = contentReader.GetAttribute("family");
						}
						if (contentReader.GetAttribute("size") != null)
						{
							size = contentReader.GetAttribute("size");
						}
						//						if (contentReader.GetAttribute("Alignment") != null)
						//						{
						//							alignment = contentReader.GetAttribute("Alignment"); // 0=left, 1=center, 2=right 3=both sides
						//					}
						if (contentReader.GetAttribute("foreground") != null)
						{
							bool textColorSuccess = Int32.TryParse(contentReader.GetAttribute("foreground"), out textColorVal);
							textColorVal = textColorVal + 16777216;
							Console.WriteLine(textColorVal);
							textColor = Convert.ToString(textColorVal, 16);
							int ln = textColor.Length;

							for (int i=0; i<6-ln; i++)
							{
								textColor = "0" + textColor;
							}
							textColor = "#" + textColor;

						}
						if (contentReader.GetAttribute("background") != null)
						{
							bool bgColorSuccess = Int32.TryParse(contentReader.GetAttribute("background"), out bgColorVal);
							bgColorVal = bgColorVal + 16777216;
							bgColor = Convert.ToString(bgColorVal, 16);
							int ln2 = bgColor.Length;

							for (int i = 0; i < 6 - ln2; i++)
							{
								bgColor = "0" + bgColor;
							}
							bgColor = "#" + bgColor;
						}
						bool isBold = GetFinalValue(curStyleData[0], elementBold);
						bool isItalic = GetFinalValue(curStyleData[1], elementItalic);
						bool isUnderlined = GetFinalValue(curStyleData[2], elementUnderlined);

						textData = textData + "<span style=" + "'color:" + textColor + "; background-color:" + bgColor + "; font-family:" + font + "; font-size:" + size + ";";
//						Console.WriteLine(textData);

						if (isBold == true)
						{
							textData = textData + " font-weight:bold;";
						}
						if (isItalic == true)
						{
							textData = textData + " font-style:italic;";
						}
						if (isUnderlined == true)
						{
							textData = textData + " text-decoration:underline;";
						}

//						Console.WriteLine("Max length: " + textContent.Length + ", Offset: " + curOffset + ", Length: " + curLength);
						textData = textData + "'>" + textContent.Substring(curOffset, curLength) + "</span>";
					}
					if (contentReader.Name == "tab" || contentReader.Name == "space")
					{
						int numChars = 0;
						bool numSuccess = Int32.TryParse(contentReader.GetAttribute("length"), out numChars);

						for (int i = 0; i < numChars; i++)
						{
							if(contentReader.Name == "tab")
							{
								textData = textData + "\t";
							}
							else
							{
								textData = textData + " ";
							}
						}
						
					}

				}
				if (contentReader.Name == "paragraph")
				{
					if(paragraphOpening == false)
					{
						int index = 0;
						string alignmentString = "";

						if (contentReader.GetAttribute("Alignment") != null)
						{
							bool succeeded = Int32.TryParse(contentReader.GetAttribute("Alignment"), out index);
							string[] alignments = { "left", "center", "right", "justify" };
							alignmentString = " style='text-align:" + alignments[index] + "'";
						}
						
						textData = textData + "<p" + alignmentString + ">";
						paragraphOpening = true;
					}
					else
					{
						paragraphOpening = false;
						textData = textData + "</p>";
					}
				}
				if (contentReader.Name == "image")
				{
					string data = contentReader.GetAttribute("imageData");

					if(data.Substring(0,6) == "iVBORw") 
					{
						textData = textData + "<img src='data:image/png;base64," + data + "'>";
					}
					
				}

				if (contentReader.Name == "styles")
				{
					break;
				}
			}
			contentReader.Dispose();
			return textData;
		}
			


	
	}
}
