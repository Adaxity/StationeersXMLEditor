using System.Globalization;
using System.Net.NetworkInformation;
using System.Xml;

class Program
{
	public static string gameDir = @"D:\SteamLibrary\steamapps\common\Stationeers\rocketstation_Data\StreamingAssets\Data";
	public static string sourceDir = gameDir;
	public static string destinationDir = gameDir;

	public static XmlDocument xmlDoc = new();
	public static XmlNodeList? selectedNodes;

	public static string fileName = "";
	public static string[] ?Whitelist;

	static void Main()
	{
		string[] files = Directory.GetFiles(sourceDir);

		foreach (string file in files)
		{
			XmlLoad(file);

			// Assemblers changes
			Whitelist = new string[] { "autolathe", "DynamicObjectsFabricator", "electronics", "fabricator", "gascanisters", "organicsprinter", "paints", "PipeBender", "rocketmanufactory", "security", "toolmanufacturer" };
			if (FileIsAllowed(fileName, Whitelist))
			{
				// Remove all empty nodes
				foreach (XmlNode node in xmlDoc.SelectNodes("//*[text()='0']"))
				{
					node.ParentNode.RemoveChild(node);

					XmlReport($"Removed empty {node.Name} tag of something");
				}

				// Reduce crafting time
				foreach (XmlNode node in xmlDoc.SelectNodes("//Time"))
				{
					float time = float.Parse(node.InnerText, CultureInfo.InvariantCulture);
					time /= 10f;
					float min = 0.25f;
					if (time < min) time = min;
					if (time > min && time < 1f) time = 1f;
					node.InnerText = time.ToString(CultureInfo.InvariantCulture);

					string parentName = node.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					XmlReport($"Reduced crafting time of {parentName} to {time}");
				}

				// Reduce required crafting materials
				selectedNodes = xmlDoc.SelectNodes("//Recipe/descendant::*[not(self::Time or self::Energy)]");
				foreach (XmlNode node in selectedNodes)
				{
					float value = float.Parse(node.InnerText, CultureInfo.InvariantCulture);
					value /= 2f;
					//if (value < 0.5f) value = 0.5f;
					//if (value > 0.5f && value < 1f) value = 1f;
					node.InnerText = value.ToString(CultureInfo.InvariantCulture);

					string parentName = node.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					XmlReport($"Reduced the required {node.Name} of {parentName} to {value}");
				}
			}

			// Furnace and Advanced Furnace changes
			Whitelist = new string[] { "furnace", "advancedfurnace" };
			if (FileIsAllowed(fileName, Whitelist))
			{
				// Reduce required start pressure/temperature
				selectedNodes = xmlDoc.SelectNodes("//Start");

				foreach (XmlNode node in selectedNodes)
				{
					float start = float.Parse(node.InnerText, CultureInfo.InvariantCulture);
					start /= 2f;
					node.InnerText = start.ToString(CultureInfo.InvariantCulture);

					string parentName = node.ParentNode.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					string what = node.ParentNode.Name;
					XmlReport($"Reduced required start {what} of {parentName} to {start}");
				}

				// Increase required stop pressure/temperature
				selectedNodes = xmlDoc.SelectNodes("//Stop");

				foreach (XmlNode node in selectedNodes)
				{
					float stop = float.Parse(node.InnerText, CultureInfo.InvariantCulture);
					stop += stop / 2f;
					if (stop > 99999f)
					{
						stop = 99999f;
					}
					node.InnerText = stop.ToString(CultureInfo.InvariantCulture);

					string parentName = node.ParentNode.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					string tempOrPress = node.ParentNode.Name;
					XmlReport($"Increased ceiling {tempOrPress} of {parentName} to {stop}");
				}
			}

			// Advanced Furnace changes
			Whitelist = new string[] { "advancedfurnace" };
			if (FileIsAllowed(fileName, Whitelist))
			{
				selectedNodes = xmlDoc.SelectNodes("//Output");

				foreach (XmlNode node in selectedNodes)
				{
					node.InnerText = "1";

					string parentName = node.ParentNode.SelectSingleNode("PrefabName").InnerText;
					XmlReport($"Real-ified output amount of {parentName}");
				}
			}

			// Arc Furnace changes
			Whitelist = new string[] { "arcfurnace" };
			if (FileIsAllowed(fileName, Whitelist))
			{
				selectedNodes = xmlDoc.SelectNodes("//Time"); // Reduce smelting time

				foreach (XmlNode node in selectedNodes)
				{
					node.InnerText = "0.25";

					string parentName = node.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					XmlReport($"Reduced smelting time of {parentName} to 0.25");
				}

				selectedNodes = xmlDoc.SelectNodes("//Energy"); // Reduce required energy to smelt

				foreach (XmlNode node in selectedNodes)
				{
					node.InnerText = "100";

					string parentName = node.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					XmlReport($"Reduced required energy of {parentName} to 100");
				}
			}
			XmlSave();
		}
		Console.WriteLine("All XML files updated successfully!\n");
	}

	public static void XmlLoad(string file)
	{
		fileName = Path.GetFileName(file);
		xmlDoc.Load($@"{file}");
	}

	public static void XmlSave()
	{
		xmlDoc.Save($@"{destinationDir}\{fileName}");
	}

	public static void XmlReport(string change)
	{
		Console.WriteLine($@"Made changes to {fileName}: {change}");
	}

	public static bool FileIsAllowed(string fileName, string[] whitelist)
	{
		foreach (string b in whitelist)
		{
			if (fileName.ToLower() == $"{b}.xml".ToLower())
			{
				return true;
			}
		}
		return false;
	}
}