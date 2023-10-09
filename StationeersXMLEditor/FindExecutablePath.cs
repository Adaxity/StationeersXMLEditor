using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal partial class Program
{
	static string? FindExecutablePath(string directory, string executableName)
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
			Console.WriteLine($"An error occurred: {ex.Message}");
		}

		return null;
	}
}