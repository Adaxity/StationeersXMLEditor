internal partial class Program
{
	private static string GetGameDir(string[]? args)
	{
		string[]? passedArgs = new string[1];

		if (args.Length == 0)
			passedArgs[0] = string.Empty;
		else
		{
			passedArgs[0] = args[0].Trim();
			passedArgs[0] = FindExecutablePath(passedArgs[0], "rocketstation.exe");
		}

		if (Directory.Exists($@"{passedArgs[0]}{StationeersFileEditor.defaultDataDir}"))
		{
			Console.WriteLine($"\nPath argument passed, Stationeers found in {passedArgs[0]}");
			gameDir = passedArgs[0];
		}
		else
		{
			Console.WriteLine("No arguments passed, trying to automatically find Stationeers path...\n");
			foreach (char a in "ABCDEFGHIJKLMNOPQRSTUVWXY") //z
			{
				if (!Directory.Exists($"{a}:"))
				{
					Console.WriteLine($"Disk {a}: not found, moving on...\n");
					continue;
				}
				else
				{
					Console.WriteLine($"Disk {a}: found, searching...");
					gameDir = FindExecutablePath($"{a}:\\", "rocketstation.exe");
					if (!string.IsNullOrEmpty(gameDir))
					{
						Console.WriteLine($"\tStationeers found on drive {a}: !");
						break;
					}
					else
					{
						Console.WriteLine("Stationeers not found, moving on...\n");
					}
				}
			}
			if (Directory.Exists($@"{gameDir}{StationeersFileEditor.defaultDataDir}"))
			{
				Console.WriteLine($"\nStationeers was found automatically in {gameDir}");
			}
			else
			{
				Console.WriteLine("\nCouldn't automatically find Stationeers folder.");
				while (!Directory.Exists($@"{gameDir}{StationeersFileEditor.defaultDataDir}"))
					try
					{
						Console.WriteLine("\nPlease enter your valid Stationeers folder path:");
						gameDir = Console.ReadLine();
					}
					catch
					{
						Console.WriteLine("\nAn error has occurred xd");
					}
			}
		}
		Console.WriteLine("\n");
		return gameDir;
	}

	private static string? FindExecutablePath(string directory, string executableName)
	{
		try
		{
			foreach (string file in Directory.GetFiles(directory, executableName))
			{
				return Path.GetDirectoryName(file);
			}

			foreach (string subdirectory in Directory.GetDirectories(directory))
			{
				try
				{
					string? result = FindExecutablePath(subdirectory, executableName);
					if (result != null)
					{
						return result;
					}
				}
				catch (UnauthorizedAccessException)
				{
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"\t{ex.Message}");
		}

		return null;
	}
}