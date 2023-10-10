using System.Xml;

public class StationeersFileEditor
{
	public const string defaultDataDir = @$"\rocketstation_Data\StreamingAssets\Data";

	public string gameDir;

	public string DataDir
	{
		get
		{
			return @$"{gameDir}{defaultDataDir}";
		}
	}

	public string BackupDir
	{
		get
		{
			return $@"{gameDir}\rocketstation_Data\StreamingAssets\DATA_ORIGINAL";
		}
	}

	public XmlDocument xmlDoc = new();
	private XmlNodeList? selectedNodes;

	public string? OpenFile;

	public StationeersFileEditor(string gamedir)
	{
		gameDir = gamedir;
	}

	public bool IsWhitelisted(string[] whitelist)
	{
		if (OpenFile == null) throw new NullReferenceException("No file opened.");
		return whitelist.Contains(OpenFile);
	}

	public XmlNodeList? SelectNodes(string selector)
	{
		XmlNodeList? nodes = xmlDoc.SelectNodes(selector);
		selectedNodes = nodes;
		return selectedNodes;
	}

	public void LoadFile(string filePath)
	{
		xmlDoc.Load(filePath);
		OpenFile = Path.GetFileNameWithoutExtension(filePath);
	}

	public void SaveFile()
	{
		xmlDoc.Save($@"{DataDir}\{OpenFile}.xml");
		OpenFile = null;
	}

	public void LogChange(string change)
	{
		//if (OpenFile == "advancedfurnace" || OpenFile == "furnace")
		Console.WriteLine($@"{OpenFile}.xml: {change}");
	}

	public bool ElementExists(string elementName)
	{
		XmlNodeList nodes = xmlDoc.GetElementsByTagName(elementName);
		return nodes.Count > 0;
	}

	public void BackupOriginalFiles()
	{
		if (!Directory.Exists(BackupDir))
		{
			Directory.CreateDirectory(BackupDir);
			Console.WriteLine(@"Backup path doesn't exist, creating one now");
		}
		Console.WriteLine("Backing up original files...");
		int i = 0;
		foreach (string filePath in Directory.GetFiles(DataDir))
		{
			LoadFile(filePath);
			if (!ElementExists("stationeers_edited"))
			{
				xmlDoc.Save($@"{BackupDir}\{OpenFile}.xml");
				LogChange("Backed up");
				OpenFile = null;
				i++;
			}
			else
			{
				LogChange("Not original, skipping...");
			}
		}
		Console.WriteLine($"{i} original files have been backed up.\n");
	}
}