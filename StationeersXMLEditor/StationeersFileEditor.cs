using System.Xml;

public class StationeersFileEditor
{
	public const string dataDir = @$"\rocketstation_Data\StreamingAssets\Data";

	public string gameDir;
	public readonly string FileSourceDir;
	public readonly string FileDestinationDir;

	public XmlDocument xmlDoc = new();
	private XmlNodeList? selectedNodes;

	public string[] XmlFiles
	{ get { return Directory.GetFiles(FileSourceDir); } }

	public string? openFile = null;
	//public string[] whiteList = Array.Empty<string>();

	public StationeersFileEditor(string gamedir)
	{
		gameDir = gamedir;
		FileSourceDir = @$"{gameDir}{dataDir}";
		FileDestinationDir = FileSourceDir;
	}

	public bool IsWhitelisted(string[] whitelist)
	{
		if (openFile == null) throw new NullReferenceException("No file opened.");
		return whitelist.Contains(openFile);
	}

	public XmlNodeList? SelectNodes(string selector)
	{
		XmlNodeList? nodes = xmlDoc.SelectNodes(selector);
		selectedNodes = nodes;
		return selectedNodes;
	}

	public void Load(string fileName)
	{
		openFile = Path.GetFileNameWithoutExtension(fileName); ;
		xmlDoc.Load(fileName);
	}

	public void SaveFile()
	{
		xmlDoc.Save($@"{FileDestinationDir}\{openFile}.xml");
		openFile = null;
	}

	public void LogChange(string change)
	{
		//if (openFile == "advancedfurnace" || openFile == "furnace")
		Console.WriteLine($@"{openFile}: {change}");
	}
}