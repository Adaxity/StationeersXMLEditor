using System.Globalization;
using System.Xml;

internal class Program
{
	public static string gameDir = @"D:\SteamLibrary\steamapps\common\Stationeers\rocketstation_Data\StreamingAssets\Data";

	private static void Main()
	{
		StationeersFileEditor editor = new StationeersFileEditor(gameDir);

		foreach (string file in editor.XmlFiles)
		{
			editor.LoadFile(file);

			// Assemblers changes
			editor.whiteList = new string[] { "autolathe", "DynamicObjectsFabricator", "electronics", "fabricator", "gascanisters", "organicsprinter", "paints", "PipeBender", "rocketmanufactory", "security", "toolmanufacturer" };
			if (editor.IsFileAllowed)
			{
				// Remove all empty nodes
				foreach (XmlNode node in editor.xmlDoc.SelectNodes("//*[text()='0']"))
				{
					node.ParentNode.RemoveChild(node);

					editor.LogFileChange($"Removed empty {node.Name} tag of something");
				}

				// Reduce crafting time
				foreach (XmlNode node in editor.xmlDoc.SelectNodes("//Time"))
				{
					float time = float.Parse(node.InnerText, CultureInfo.InvariantCulture);
					time /= 10f;
					float min = 0.25f;
					if (time < min) time = min;
					if (time > min && time < 1f) time = 1f;
					node.InnerText = time.ToString(CultureInfo.InvariantCulture);

					string parentName = node.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					editor.LogFileChange($"Reduced crafting time of {parentName} to {time}");
				}

				// Reduce required crafting materials
				editor.selectedNodes = editor.xmlDoc.SelectNodes("//Recipe/descendant::*[not(self::Time or self::Energy)]");
				foreach (XmlNode node in editor.selectedNodes)
				{
					float value = float.Parse(node.InnerText, CultureInfo.InvariantCulture);
					value /= 2f;
					//if (value < 0.5f) value = 0.5f;
					//if (value > 0.5f && value < 1f) value = 1f;
					node.InnerText = value.ToString(CultureInfo.InvariantCulture);

					string parentName = node.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					editor.LogFileChange($"Reduced the required {node.Name} of {parentName} to {value}");
				}
			}

			// Furnace and Advanced Furnace changes
			editor.whiteList = new string[] { "furnace", "advancedfurnace" };
			if (editor.IsFileAllowed)
			{
				// Reduce required start pressure/temperature
				editor.selectedNodes = editor.xmlDoc.SelectNodes("//Start");

				foreach (XmlNode node in editor.selectedNodes)
				{
					float start = float.Parse(node.InnerText, CultureInfo.InvariantCulture);
					start /= 2f;
					node.InnerText = start.ToString(CultureInfo.InvariantCulture);

					string parentName = node.ParentNode.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					string what = node.ParentNode.Name;
					editor.LogFileChange($"Reduced required start {what} of {parentName} to {start}");
				}

				// Increase required stop pressure/temperature
				editor.selectedNodes = editor.xmlDoc.SelectNodes("//Stop");

				foreach (XmlNode node in editor.selectedNodes)
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
					editor.LogFileChange($"Increased ceiling {tempOrPress} of {parentName} to {stop}");
				}
			}

			// Advanced Furnace changes
			editor.whiteList = new string[] { "advancedfurnace" };
			if (editor.IsFileAllowed)
			{
				editor.selectedNodes = editor.xmlDoc.SelectNodes("//Output");

				foreach (XmlNode node in editor.selectedNodes)
				{
					node.InnerText = "1";

					string parentName = node.ParentNode.SelectSingleNode("PrefabName").InnerText;
					editor.LogFileChange($"Real-ified output amount of {parentName}");
				}
			}

			// Arc Furnace changes
			editor.whiteList = new string[] { "arcfurnace" };
			if (editor.IsFileAllowed)
			{
				editor.selectedNodes = editor.xmlDoc.SelectNodes("//Time"); // Reduce smelting time

				foreach (XmlNode node in editor.selectedNodes)
				{
					node.InnerText = "0.25";

					string parentName = node.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					editor.LogFileChange($"Reduced smelting time of {parentName} to 0.25");
				}

				editor.selectedNodes = editor.xmlDoc.SelectNodes("//Energy"); // Reduce required energy to smelt

				foreach (XmlNode node in editor.selectedNodes)
				{
					node.InnerText = "100";

					string parentName = node.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					editor.LogFileChange($"Reduced required energy of {parentName} to 100");
				}
			}
			editor.SaveFile();
		}
		Console.WriteLine("All XML Files updated successfully!\n");
	}
}

/*
public static class Files
{
	public const string AutoLathe = "autolathe";
}
*/