using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace MakeFolderFromFileName.ConsoleApp
{
	class Program
	{
		static int Main(string[] args)
		{
			var outputFolderPathFormat = "<DIR>";
			var outputFolderNameFormat = "<TITLE>";
			var outputFileNameFormat = "<NAME>";
			var numberFormat = "g";
			var dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat.UniversalSortableDateTimePattern;
			var searchPattern = "*.*";
			var recursiveSearch = false;
			var move = false;
			var copy = false;
			var overwrite = false;
			var sanitize = false;
			var simulation = false;
			var verbose = false;
			var quiet = false;

			try
			{
				if (args.Length == 0)
				{
					ShowProgramUsage(outputFolderPathFormat, outputFolderNameFormat, outputFileNameFormat, numberFormat, dateTimeFormat, searchPattern, recursiveSearch, move, copy, overwrite, sanitize, simulation, verbose, quiet);
					return 0;
				}

				for (var i = 0; i < args.Length; ++i)
				{
					var arg = args[i];

					if (arg.StartsWith("/D:"))
					{
						outputFolderPathFormat = arg.Substring("/D:".Length);
					}
					else if (arg.StartsWith("/F:"))
					{
						outputFolderNameFormat = arg.Substring("/F:".Length);
					}
					else if (arg.StartsWith("/G:"))
					{
						outputFileNameFormat = arg.Substring("/G:".Length);
					}
					else if (arg.StartsWith("/N:"))
					{
						numberFormat = arg.Substring("/N:".Length);
					}
					else if (arg.StartsWith("/T:"))
					{
						dateTimeFormat = arg.Substring("/T:".Length);
					}
					else if (arg.StartsWith("/P:"))
					{
						searchPattern = arg.Substring("/P:".Length);
					}
					else if (arg.Equals("/X"))
					{
						sanitize = true;
					}
					else if (arg.Equals("/x"))
					{
						sanitize = false;
					}
					else if (arg.Equals("/S"))
					{
						simulation = true;
					}
					else if (arg.Equals("/s"))
					{
						simulation = false;
					}
					else if (arg.Equals("/R"))
					{
						recursiveSearch = true;
					}
					else if (arg.Equals("/r"))
					{
						recursiveSearch = false;
					}
					else if (arg.Equals("/M"))
					{
						move = true;
					}
					else if (arg.Equals("/m"))
					{
						move = false;
					}
					else if (arg.Equals("/C"))
					{
						copy = true;
					}
					else if (arg.Equals("/c"))
					{
						copy = false;
					}
					else if (arg.Equals("/W"))
					{
						overwrite = true;
					}
					else if (arg.Equals("/w"))
					{
						overwrite = false;
					}
					else if (arg.Equals("/V"))
					{
						verbose = true;
					}
					else if (arg.Equals("/v"))
					{
						verbose = false;
					}
					else if (arg.Equals("/Q"))
					{
						quiet = true;
					}
					else if (arg.Equals("/q"))
					{
						quiet = false;
					}
					else if (arg.Equals("/?"))
					{
						ShowProgramUsage(outputFolderPathFormat, outputFolderNameFormat, outputFileNameFormat, numberFormat, dateTimeFormat, searchPattern, recursiveSearch, move, copy, overwrite, sanitize, simulation, verbose, quiet);
					}
					else if (Directory.Exists(arg))
					{
						ProcessDirectory(new DirectoryInfo(arg), outputFolderPathFormat, outputFolderNameFormat, outputFileNameFormat, numberFormat, dateTimeFormat, searchPattern, recursiveSearch, move, copy, overwrite, sanitize, simulation, verbose, quiet);
					}
					else if (File.Exists(arg))
					{
						ProcessFile(new FileInfo(arg), outputFolderPathFormat, outputFolderNameFormat, outputFileNameFormat, numberFormat, dateTimeFormat, searchPattern, recursiveSearch, move, copy, overwrite, sanitize, simulation, verbose, quiet);
					}
					else
					{
						throw new Exception($"Unrecognized argument '{arg}' at index {i}.");
					}
				}
			}
			catch (Exception ex)
			{
				if (!quiet)
				{
					Console.Error.WriteLine(ex.Message);
				}

				return 1;
			}

			return 0;
		}

		private static void ShowProgramUsage(string outputFolderPathFormat, string outputFolderNameFormat, string outputFileNameFormat, string numberFormat, string dateTimeFormat, string searchPattern, bool recursiveSearch, bool move, bool copy, bool overwrite, bool sanitize, bool simulation, bool verbose, bool quiet)
		{
			Console.WriteLine("usage: [options...] [files or folders...]");
			Console.WriteLine("options:");
			Console.WriteLine($"  /D:format             set output folder path format (current=\"{outputFolderPathFormat}\")");
			Console.WriteLine($"  /F:format             set output folder name format (current=\"{outputFolderNameFormat}\")");
			Console.WriteLine($"  /G:format             set output file name format (current=\"{outputFileNameFormat}\")");
			Console.WriteLine($"                        format special symbols:");
			Console.WriteLine($"                <CWD> - current working directory");
			Console.WriteLine($"                <DIR> - input file directory name");
			Console.WriteLine($"              <TITLE> - input file title");
			Console.WriteLine($"                <EXT> - input file extension");
			Console.WriteLine($"               <NAME> - input file name");
			Console.WriteLine($"                <LEN> - input file length");
			Console.WriteLine($"              <CTIME> - input file creation time");
			Console.WriteLine($"              <MTIME> - input file last write time");
			Console.WriteLine($"              <ATIME> - input file last access time");
			Console.WriteLine($"  /N:format             set number format, used with <LEN> (current=\"{numberFormat}\")");
			Console.WriteLine($"  /T:format             set date/time format, used with <?TIME> (current=\"{dateTimeFormat}\")");
			Console.WriteLine($"  /P:pattern            set file search pattern (current=\"{searchPattern}\")");
			Console.WriteLine($"  /R                    enable recursive subfolder search (current={recursiveSearch})");
			Console.WriteLine($"  /M                    move input file to output folder (current={move})");
			Console.WriteLine($"  /C                    copy input file to output folder (current={copy})");
			Console.WriteLine($"  /W                    overwrite existing files (current={overwrite})");
			Console.WriteLine($"  /X                    sanitize output path and name (current={sanitize})");
			Console.WriteLine($"  /S                    enable simulation mode (current={simulation})");
			Console.WriteLine($"  /V                    enable verbose mode (current={verbose})");
			Console.WriteLine($"  /Q                    enable quiet mode (current={quiet})");
			Console.WriteLine($"  /?                    show program usage");
			Console.WriteLine($"examples:");
			Console.WriteLine($"  1. create subfolders only");
			Console.WriteLine($"     \"docs\"");
			Console.WriteLine($"  2. create subfolders, copying files over, renaming them \"folder\"");
			Console.WriteLine($"     /C /R \"/P:*.jpeg\" \"/G:folder<EXT>\" \"pics\"");
		}

		private static void ProcessDirectory(DirectoryInfo directoryInfo, string outputFolderPathFormat, string outputFolderNameFormat, string outputFileNameFormat, string numberFormat, string dateTimeFormat, string searchPattern, bool recursiveSearch, bool move, bool copy, bool overwrite, bool sanitize, bool simulation, bool verbose, bool quiet)
		{
			var files = copy || move
				? directoryInfo.GetFiles(searchPattern, recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
				: directoryInfo.EnumerateFiles(searchPattern, recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

			foreach (var fileInfo in files)
			{
				ProcessFile(fileInfo, outputFolderPathFormat, outputFolderNameFormat, outputFileNameFormat, numberFormat, dateTimeFormat, searchPattern, recursiveSearch, move, copy, overwrite, sanitize, simulation, verbose, quiet);
			}
		}

		private static void ProcessFile(FileInfo fileInfo, string outputFolderPathFormat, string outputFolderNameFormat, string outputFileNameFormat, string numberFormat, string dateTimeFormat, string searchPattern, bool recursiveSearch, bool move, bool copy, bool overwrite, bool sanitize, bool simulation, bool verbose, bool quiet)
		{
			try
			{
				if (verbose && !quiet)
				{
					Console.WriteLine($"in:  {fileInfo.FullName}");
				}

				var regex = new Regex(@"<(\w+)>");
				var folderPath = regex.Replace(outputFolderPathFormat, matchEvaluator);
				var folderName = regex.Replace(outputFolderNameFormat, matchEvaluator);
				var fileName = regex.Replace(outputFileNameFormat, matchEvaluator);

				if (sanitize)
				{
					foreach (var chr in Path.GetInvalidPathChars())
					{
						folderPath = folderPath.Replace(chr, '_');
					}

					foreach (var chr in Path.GetInvalidFileNameChars())
					{
						folderName = folderName.Replace(chr, '_');
						fileName = fileName.Replace(chr, '_');
					}
				}

				var folderFullPath = Path.Combine(folderPath, folderName);

				if (verbose && !quiet)
				{
					Console.WriteLine($"out: {folderFullPath}");
				}

				if (!simulation)
				{
					Directory.CreateDirectory(folderFullPath);
				}

				var fileFullPath = Path.Combine(folderFullPath, fileName);

				if (copy || move)
				{
					if (verbose && !quiet)
					{
						Console.WriteLine($"out: {fileFullPath}");
					}
				}

				if (!simulation)
				{
					if (move)
					{
						if (overwrite
							&& File.Exists(fileFullPath))
						{
							File.Copy(fileInfo.FullName, fileFullPath, overwrite);
							File.Delete(fileInfo.FullName);
						}
						else
						{
							File.Move(fileInfo.FullName, fileFullPath);
						}
					}
					else if (copy)
					{
						File.Copy(fileInfo.FullName, fileFullPath, overwrite);
					}
				}

				string matchEvaluator(Match match)
				{
					var token = match.Groups[1].Value;

					if (token.Equals("CWD", StringComparison.OrdinalIgnoreCase))
					{
						return Environment.CurrentDirectory;
					}
					else if (token.Equals("DIR", StringComparison.OrdinalIgnoreCase))
					{
						return Path.GetDirectoryName(fileInfo.FullName);
					}
					else if (token.Equals("TITLE", StringComparison.OrdinalIgnoreCase))
					{
						return Path.GetFileNameWithoutExtension(fileInfo.FullName);
					}
					else if (token.Equals("NAME", StringComparison.OrdinalIgnoreCase))
					{
						return fileInfo.Name;
					}
					else if (token.Equals("PATH", StringComparison.OrdinalIgnoreCase))
					{
						return fileInfo.FullName;
					}
					else if (token.Equals("EXT", StringComparison.OrdinalIgnoreCase))
					{
						return fileInfo.Extension;
					}
					else if (token.Equals("LEN", StringComparison.OrdinalIgnoreCase))
					{
						return fileInfo.Length.ToString(numberFormat);
					}
					else if (token.Equals("CTIME", StringComparison.OrdinalIgnoreCase))
					{
						return fileInfo.CreationTime.ToString(dateTimeFormat);
					}
					else if (token.Equals("MTIME", StringComparison.OrdinalIgnoreCase))
					{
						return fileInfo.LastWriteTime.ToString(dateTimeFormat);
					}
					else if (token.Equals("ATIME", StringComparison.OrdinalIgnoreCase))
					{
						return fileInfo.LastAccessTime.ToString(dateTimeFormat);
					}

					return match.Value;
				};
			}
			catch (Exception ex)
			{
				if (!quiet)
				{
					Console.Error.WriteLine(ex.Message);
				}
			}
		}
	}
}
