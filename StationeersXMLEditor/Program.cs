using System.Diagnostics;
using System.Globalization;
using System.Xml;

internal class Program
{
	private static void Main(string[]? args)
	{
		string? gameDir = string.Empty;
		string[]? passedArgs = new string[1];

		if (args.Length == 0)
			passedArgs[0] = string.Empty;
		else
			passedArgs[0] = args[0].Trim();

		if (Directory.Exists($@"{passedArgs[0]}{StationeersFileEditor.dataDir}"))
			gameDir = passedArgs[0];
		else if (Directory.Exists($@"{StationeersFileEditor.defaultDir}{StationeersFileEditor.dataDir}"))
		{
			Console.WriteLine($"\nNo arguments were passed, but Stationeers was found in {StationeersFileEditor.defaultDir}");
			gameDir = StationeersFileEditor.defaultDir;
		}
		else
			Console.WriteLine("Couldn't automatically find Stationeers folder.");

		while (!Directory.Exists($@"{gameDir}{StationeersFileEditor.dataDir}"))
			try
			{
				Console.WriteLine("\nPlease enter your valid Stationeers folder directory:");
				gameDir = Console.ReadLine();
			}
			catch { }

		StationeersFileEditor editor = new(gameDir);

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
					value /= 10f;
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
				// Increase required stop pressure/temperature
				editor.selectedNodes = editor.xmlDoc.SelectNodes("//Start | //Stop");

				foreach (XmlNode node in editor.selectedNodes)
				{
					string parentName = node.ParentNode.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					string tempOrPress = node.ParentNode.Name;
					string startOrStop = node.Name;

					float value = 0f;

					Console.WriteLine($"current is {tempOrPress} {startOrStop}");
					if (startOrStop == "Start")
					{
						value = float.Parse(node.InnerText, CultureInfo.InvariantCulture);
						value /= 2f;
						node.InnerText = value.ToString(CultureInfo.InvariantCulture);
					}
					else if (startOrStop == "Stop")
					{
						value = float.Parse(node.InnerText, CultureInfo.InvariantCulture);
						value += value / 2f;
						if (value > 99999f)
						{
							value = 99999f;
						}
						node.InnerText = value.ToString(CultureInfo.InvariantCulture);
					}
					editor.LogFileChange($"Changed {startOrStop} {tempOrPress} of {parentName} to {value}");
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
					editor.LogFileChange($"Reduced required required energy for smelting of {parentName} to 100");
				}
			}

			// Mineables changes
			editor.whiteList = new string[] { "mineables" };
			if (editor.IsFileAllowed)
			{
				editor.selectedNodes = editor.xmlDoc.SelectNodes("//MineableData/*[self::MaxDropQuantity or self::MinDropQuantity]");
				foreach (XmlNode node in editor.selectedNodes)
				{
					float amount = float.Parse(node.InnerText, CultureInfo.InvariantCulture);
					amount *= 20f;
					node.InnerText = amount.ToString(CultureInfo.InvariantCulture);

					string parentName = node.ParentNode.SelectSingleNode("DisplayName").InnerText;
					editor.LogFileChange($"Set mined amount of {parentName} to 100");
				}

				editor.selectedNodes = editor.xmlDoc.SelectNodes("//MineableData[DisplayName='Geyser' or DisplayName='Uranium']/*[number(text()) = number(text())]");
				foreach (XmlNode node in editor.selectedNodes)
				{
					node.InnerText = "0";

					string parentName = node.ParentNode.SelectSingleNode("DisplayName").InnerText;
					editor.LogFileChange($"Set values of {parentName} to 0");
				}
			}

			editor.SaveFile();
		}
		Console.WriteLine("All XML Files updated successfully!\n\nPress any key to exit...");
		Console.ReadKey();
	}
}

/*
public static class Files
{
	public const string AutoLathe = "autolathe";
}
*/