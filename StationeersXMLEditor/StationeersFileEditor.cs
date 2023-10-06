﻿using System.Xml;

public class StationeersFileEditor
{
	private const string defaultStationeersDir = @"C:\Program Files (x86)\Steam\steamapps\common\Stationeers";

	public string gameDir;
	public readonly string FileSourceDir;
	public readonly string FileDestinationDir;

	public XmlDocument xmlDoc = new();
	public XmlNodeList? selectedNodes;

	public string[] XmlFiles
	{ get { return Directory.GetFiles(FileSourceDir); } }

	public string? openFile = null;
	public string[] whiteList = Array.Empty<string>();

	public bool IsFileAllowed
	{
		get
		{
			if (openFile == null) throw new NullReferenceException("No file opened.");
			return whiteList.Contains(openFile);
		}
	}

	public StationeersFileEditor(string gamedir = defaultStationeersDir)
	{
		gameDir = gamedir;
		FileSourceDir = @$"{gameDir}\rocketstation_Data\StreamingAssets\Data";
		FileDestinationDir = FileSourceDir;
	}

	public void LoadFile(string fileName)
	{
		openFile = Path.GetFileNameWithoutExtension(fileName); ;
		xmlDoc.Load(fileName);
	}

	public void SaveFile()
	{
		xmlDoc.Save($@"{FileDestinationDir}\{openFile}.xml");
		openFile = null;
	}

	public void LogFileChange(string change)
	{
		Console.WriteLine($@"Made changes to {openFile}: {change}");
	}
}