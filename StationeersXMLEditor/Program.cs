﻿using System.Globalization;
using System.Xml;

internal partial class Program
{
	private static string? gameDir = string.Empty;
	private static StationeersFileEditor? editor;

	private static void Main(string[]? args)
	{
		editor = new(GetGameDir(new[] { "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Stationeers" }));

		editor.BackupOriginalFiles();

		foreach (string filePath in Directory.GetFiles(editor.BackupDir))
		{
			Console.WriteLine($"\nLoading {filePath} ...");
			editor.LoadFile(filePath);

			if (editor.ElementExists("stationeers_edited"))
			{
				Console.WriteLine($"{editor.OpenFile} has already been edited, skipping");
				continue;
			}
			else
			{
				XmlElement newElement = editor.xmlDoc.CreateElement("stationeers_edited");
				editor.xmlDoc.DocumentElement.PrependChild(newElement);
				Console.WriteLine($"Adding <stationeers_edited> tag to {editor.OpenFile}");
			}

			// Assemblers changes
			if (editor.IsWhitelisted(new[] { "autolathe", "DynamicObjectsFabricator", "electronics", "fabricator", "gascanisters", "organicsprinter", "paints", "PipeBender", "rocketmanufactory", "security", "toolmanufacturer" }))
			{
				// Remove all empty nodes
				foreach (XmlNode node in editor.SelectNodes("//*[text()='0']"))
				{
					string parentName = node.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					node.ParentNode.RemoveChild(node);
					editor.LogChange($"Removed empty {node.Name} tag of {parentName}");
				}

				// Change crafting values
				foreach (XmlNode node in editor.SelectNodes("//Recipe/*"))
				{
					float value = float.Parse(node.InnerText, CultureInfo.InvariantCulture);
					switch (node.Name)
					{
						case "Time":
							float min = 0.25f;
							value /= 10f;
							if (value < min) value = min;
							if (value > min && value < 1f) value = 1f;
							break;

						case "Energy":
							value /= 2f;
							if (value < 1) value = 1;
							break;

						default:
							value /= 10f;
							break;
					}
					node.InnerText = value.ToString(CultureInfo.InvariantCulture);
					string parentName = node.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					editor.LogChange($"Reduced the {node.Name} of {parentName} to {value}");
				}
			}

			// Furnace and Advanced Furnace changes
			if (editor.IsWhitelisted(new[] { "furnace", "advancedfurnace" }))
			{
				// Increase required stop pressure/temperature, reduce required start pressure/temperature
				foreach (XmlNode node in editor.SelectNodes("//Start | //Stop"))
				{
					string parentName = node.ParentNode.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					string tempOrPress = node.ParentNode.Name;
					string startOrStop = node.Name;

					float value = 0f;

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
					editor.LogChange($"Changed {startOrStop} {tempOrPress} of {parentName} to {value}");
				}
			}

			// Advanced Furnace changes
			if (editor.IsWhitelisted(new[] { "advancedfurnace" }))
			{
				//Normalize output (in vanilla you get 1g of a superalloy from 4g of ores/ingots = doesn't make sense)
				foreach (XmlNode node in editor.SelectNodes("//Output"))
				{
					node.InnerText = "1";

					string parentName = node.ParentNode.SelectSingleNode("PrefabName").InnerText;
					editor.LogChange($"Real-ified output amount of {parentName}");
				}
			}

			// Arc Furnace changes
			if (editor.IsWhitelisted(new[] { "arcfurnace" }))
			{
				// Reduce required Time and Energy for smelting
				foreach (XmlNode node in editor.SelectNodes("//Time | //Energy"))
				{
					if (node.Name == "Time")
					{
						node.InnerText = "0.25";
					}
					else if (node.Name == "Energy")
					{
						node.InnerText = "100";
					}
					string parentName = node.ParentNode.ParentNode.SelectSingleNode("PrefabName").InnerText;
					editor.LogChange($"Reduced required {node.Name} of {parentName} to {node.InnerText}");
				}
			}

			// Mineables changes
			if (editor.IsWhitelisted(new[] { "mineables" }))
			{
				// Change drop quantity of ores
				foreach (XmlNode node in editor.SelectNodes("//MaxDropQuantity | //MinDropQuantity"))
				{
					float amount = float.Parse(node.InnerText, CultureInfo.InvariantCulture);
					amount *= 20f;
					node.InnerText = amount.ToString(CultureInfo.InvariantCulture);

					string parentName = node.ParentNode.SelectSingleNode("DisplayName").InnerText;
					editor.LogChange($"Set mined amount of {parentName} to 100");
				}

				// Prevent geysers and uranium from spawning
				foreach (XmlNode node in editor.SelectNodes("//MineableData[DisplayName='Geyser' or DisplayName='Uranium']/*[number(text()) = number(text())]"))
				{
					node.InnerText = "0";

					string parentName = node.ParentNode.SelectSingleNode("DisplayName").InnerText;
					editor.LogChange($"Set values of {parentName} to 0");
				}
			}

			editor.SaveFile();
		}
		Console.WriteLine("\nAll XML Files updated successfully!\n\nPress any key to exit...");
		Console.ReadKey();
	}
}