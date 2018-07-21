using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ash.OIUtils.HttpServerResponse
{
	class Program
	{
		static void Main(string[] args)
		{
			WriteProgramHeader();

			ProcessProgramArguments(args);

			if ((args.Length == 0) || (args.Length == args.Count(x => x.StartsWith(Options.ShortOptionPrefix) || x.StartsWith(Options.LongOptionPrefix))))
			{
				ProcessDirectUserInput();
			}

			if (Options.WaitForUserInput)
			{
				WaitForUserInput();
			}
		}

		static void WriteProgramHeader()
		{
			FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

			Console.Out.WriteLine("{0} v{1}", fileVersionInfo.ProductName, fileVersionInfo.FileVersion);
			Console.Out.WriteLine();
		}

		static void WaitForUserInput()
		{
			Console.Out.WriteLine();
			Console.Out.Write("Done. Press any key to exit...");
			Console.ReadKey(intercept: true);
		}

		static void ProcessDirectUserInput()
		{
			Console.Out.WriteLine("Type /help for usage or press Enter to quit the program.");
			Console.Out.WriteLine();

			do
			{
				Console.Out.Write("Enter either nothing, an option, or a file name: ");

				string line = Console.ReadLine();

				if (string.IsNullOrEmpty(line))
				{
					break;
				}
				else
				{
					// TODO ideally, we'd want to split <line> (a string) into <args> (an array of strings).
					ProcessProgramArguments(new string[] { line });
				}
			} while (true);
		}


		static void ProcessProgramArguments(string[] args)
		{
			for (int i = 0; i < args.Length; ++i)
			{
				string arg = args[i];

				try
				{
					if (arg.StartsWith(Options.ShortOptionPrefix) || arg.StartsWith(Options.LongOptionPrefix))
					{
						ProcessOption(args, ref i);
					}
					else
					{
						// dropping a file with spaces inserts a filename with double quotes.
						arg = arg.StripDoubleQuotes();

						if (!RequirePassword(args))
						{
							continue;
						}

						ProcessDirectoryOrFile(arg, ref i);
					}
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine("*** exception: {0}", ex.Message);

					if (Options.VerboseLevel >= 4)
					{
						Console.Error.WriteLine();
						Console.Error.WriteLine(ex.StackTrace);
						Console.Error.WriteLine();
					}
				}
			}
		}

		static void ProcessOption(string[] args, ref int argIndex)
		{
			string arg = args[argIndex];
			int length = 0;

			if (TryMatchArgument(arg, "h", "help", out length))
			{
				Console.Out.WriteLine("");
				Console.Out.WriteLine("usage:");
				Console.Out.WriteLine("");
				Console.Out.WriteLine("  [options...] <[file|directory]...>");
				Console.Out.WriteLine("");
				//                     ....|....1....|....2....|....3....|....4....|....5....|....6....|....7....|....8
				Console.Out.WriteLine("options:");
				Console.Out.WriteLine("");
				Console.Out.WriteLine("  /f or /filePattern=<pattern:string>");
				Console.Out.WriteLine("     Override file pattern (default: `*.*`)");
				Console.Out.WriteLine("     Use | to split multiple patterns");
				Console.Out.WriteLine();
				Console.Out.WriteLine("  /d or /directoryPattern=<pattern:string>");
				Console.Out.WriteLine("     Override directory pattern (default: `*`)");
				Console.Out.WriteLine();
				Console.Out.WriteLine("  /t or /preserveDirectoryStructure=<enable:bool>");
				Console.Out.WriteLine("     Preserve directory structure between input and output paths (default: 1)");
				Console.Out.WriteLine();
				Console.Out.WriteLine("  /x or /password=<enable:string>");
				Console.Out.WriteLine("     Sets the decryption key (required to decrypt first data entry)");
				Console.Out.WriteLine();
				Console.Out.WriteLine("        /prettify=<enable:bool>");
				Console.Out.WriteLine("     Whether to format the json output (default: 1)");
				//Console.Out.WriteLine();
				//Console.Out.WriteLine("        /crc=<enable:bool>");
				//Console.Out.WriteLine("     Whether to perform a CRC on the decoded stream (default: 0)");
				Console.Out.WriteLine();
				Console.Out.WriteLine("  /v or /verbose=<level:int>");
				Console.Out.WriteLine("     Set the level of verbosity in message output (default: 3)");
				Console.Out.WriteLine("     Where level is 0=none, 1=errors, 2=warnings, 3=traces, 4=debugging info");
				Console.Out.WriteLine();
				Console.Out.WriteLine("  /q or /quiet");
				Console.Out.WriteLine("     Suppress all message output");
				Console.Out.WriteLine();
				Console.Out.WriteLine("  /w or /waitForUserInput=<enable:bool>");
				Console.Out.WriteLine("     Whether to wait for user confirmation to exit the program (default: 1)");
				Console.Out.WriteLine("");
			}
			else if (TryMatchArgument(arg, "i", "inputPath", out length))
			{
				ProcessArgument(arg, length, ref Options.InputPath);
				Options.InputPath = Options.InputPath.StripDoubleQuotes();
			}
			else if (TryMatchArgument(arg, "o", "outputPath", out length))
			{
				ProcessArgument(arg, length, ref Options.OutputPath);
				Options.OutputPath = Options.OutputPath.StripDoubleQuotes();
			}
			else if (TryMatchArgument(arg, "f", "filePattern", out length))
			{
				ProcessArgument(arg, length, ref Options.FilePattern);
				Options.FilePattern = Options.FilePattern.StripDoubleQuotes();
			}
			else if (TryMatchArgument(arg, "d", "directoryPattern", out length))
			{
				ProcessArgument(arg, length, ref Options.DirectoryPattern);
				Options.DirectoryPattern = Options.DirectoryPattern.StripDoubleQuotes();
			}
			else if (TryMatchArgument(arg, "t", "preserveDirectoryStructure", out length))
			{
				ProcessArgument(arg, length, ref Options.PreserveDirectoryStructure);
			}
			else if (TryMatchArgument(arg, "x", "password", out length))
			{
				ProcessArgument(arg, length, ref Options.DataEncryptionKey);
			}
			else if (TryMatchArgument(arg, "", "prettify", out length))
			{
				ProcessArgument(arg, length, ref Options.PrettifyJson);
			}
			else if (TryMatchArgument(arg, "", "crc", out length))
			{
				ProcessArgument(arg, length, ref Options.PerformCRC);
			}
			else if (TryMatchArgument(arg, "w", "waitForUserInput", out length))
			{
				ProcessArgument(arg, length, ref Options.WaitForUserInput);
			}
		}

		static bool TryMatchArgument(string arg, string shortName, string longName, out int length)
		{
			string prefixedShortName = Options.ShortOptionPrefix + shortName;
			string prefixedLongName = Options.LongOptionPrefix + longName;
			StringComparison stringComparison = Options.IsOptionCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

			if (!string.IsNullOrEmpty(prefixedShortName) && (arg.Equals(prefixedShortName, stringComparison) || arg.StartsWith(prefixedShortName + "=", stringComparison)))
			{
				length = prefixedShortName.Length;
			}
			else if (!string.IsNullOrEmpty(prefixedLongName) && (arg.Equals(prefixedLongName, stringComparison) || arg.StartsWith(prefixedLongName + "=", stringComparison)))
			{
				length = prefixedLongName.Length;
			}
			else
			{
				length = 0;
				return false;
			}

			return true;
		}

		static void ProcessArgument(string arg, int startIndex, ref string destination)
		{
			ProcessArgument(arg, startIndex, ref destination, x => x);
		}

		static void ProcessArgument(string arg, int startIndex, ref int destination)
		{
			ProcessArgument(arg, startIndex, ref destination, x => int.Parse(x));
		}

		static void ProcessArgument(string arg, int startIndex, ref bool destination)
		{
			ProcessArgument(arg, startIndex, ref destination, x => x.ToBoolean());
		}

		/// <example>
		/// I prefer the "/verb=argument" syntax; but to support the more common "-verb argument" syntax,
		/// you can just replace the parameters:
		/// <code>string arg, int startIndex</code> with <code>string[] args, ref int argIndex</code>
		/// and the argument extraction:
		/// <code>arg.Substring(startIndex + 1)</code> with <code>args[++argIndex]</code>.
		/// </example>
		static void ProcessArgument<TValue>(string arg, int startIndex, ref TValue destination, Func<string, TValue> converter)
		{
			if ((startIndex + 1) <= arg.Length)
			{
				destination = converter.Invoke(arg.Substring(startIndex + 1));
			}
			else
			{
				Console.Out.WriteLine(destination);
			}
		}

		static void RequireArgument(string[] args, int argIndex, int count)
		{
			if (argIndex + count > args.Length)
			{
				throw new ArgumentException(string.Format("the option {0} requires {1} arguments", args[argIndex], count), args[argIndex]);
			}
		}

		static bool RequirePassword(string[] args)
		{
			if (string.IsNullOrEmpty(Options.DataEncryptionKey))
			{
				for (int i = 0; i < args.Length; ++i)
				{
					string arg = args[i];
					int length = 0;

					if (TryMatchArgument(arg, "x", "password", out length))
					{
						ProcessArgument(arg, length, ref Options.DataEncryptionKey);
					}
				}
			}

			if (string.IsNullOrEmpty(Options.DataEncryptionKey))
			{
				Console.Out.Write("Please input the decryption key, or nothing to exit: ");

				string line = Console.In.ReadLine();

				if (string.IsNullOrEmpty(line))
				{
					return false;
				}

				Options.DataEncryptionKey = line;
			}

			return true;
		}

		static void ProcessDirectoryOrFile(string arg, ref int argIndex)
		{
			string sourcePath = BuildSourcePath(arg);
			bool isFile = File.Exists(sourcePath);
			bool isDirectory = Directory.Exists(sourcePath);

			if (!isFile && !isDirectory)
			{
				if (Options.VerboseLevel >= 1)
				{
					Console.Error.WriteLine("*** error: `{0}` is not a valid file or directory.", sourcePath);
				}
				return;
			}

			Stopwatch timer = null;

			if (Options.VerboseLevel >= 4)
			{
				timer = Stopwatch.StartNew();
			}

			if (isDirectory)
			{
				ProcessDirectory(sourcePath, sourcePath);
			}
			else
			{
				ProcessFile(sourcePath, Path.GetDirectoryName(sourcePath));
			}

			if (Options.VerboseLevel >= 4)
			{
				timer.Stop();

				Console.Out.WriteLine("Completed in {0}.", timer.Elapsed);
				Console.Out.WriteLine();
			}
		}

		static string BuildSourcePath(string path)
		{
			string sourcePath;

			if (!string.IsNullOrEmpty(Options.InputPath))
			{
				sourcePath = Path.Combine(Options.InputPath, path);
			}
			else
			{
				sourcePath = path;
			}

			return sourcePath;
		}

		static void ProcessDirectory(string path, string rootPath)
		{
			if (Options.VerboseLevel >= 3)
			{
				Console.Out.WriteLine("Processing directory `{0}`...", path.Replace(AppDomain.CurrentDomain.BaseDirectory, ""));
			}

			if (!Directory.Exists(path))
			{
				if (Options.VerboseLevel >= 1)
				{
					Console.Error.WriteLine("*** error: the directory does not exist.");
				}
				return;
			}

			foreach (string filePattern in Options.FilePattern.Split(new char[] { '|' }))
			{
				IEnumerable<string> files = Directory.EnumerateFiles(path, filePattern, SearchOption.TopDirectoryOnly);

				foreach (string fileName in files)
				{
					ProcessFile(fileName, rootPath);
				}
			}

			foreach (string directoryPattern in Options.DirectoryPattern.Split(new char[] { '|' }))
			{
				IEnumerable<string> directories = Directory.EnumerateDirectories(path, directoryPattern, SearchOption.TopDirectoryOnly);

				foreach (string directoryName in directories)
				{
					ProcessDirectory(directoryName, rootPath);
				}
			}
		}

		static void ProcessFile(string path, string rootPath)
		{
			if (Options.VerboseLevel >= 3)
			{
				Console.Out.WriteLine("Processing file `{0}`...", Path.GetFileName(path));
			}

			if (!File.Exists(path))
			{
				if (Options.VerboseLevel >= 1)
				{
					Console.Error.WriteLine("*** error: the file `{0}` does not exist.", path);
				}
				return;
			}

			string fileContent = File.ReadAllText(path);

			if (string.IsNullOrEmpty(fileContent))
			{
				Console.Error.WriteLine("*** warning: file is empty.");
				return;
			}

			string response = fileContent;

			string json = Decode(response, Options.DataEncryptionKey);

			if (Options.PrettifyJson)
			{
				json = json.PrettifyJson();
			}

			string destinationPath = BuildDestinationPath(path, rootPath, string.Concat(Path.GetFileNameWithoutExtension(path), ".json"));

			Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

			File.WriteAllText(destinationPath, json);
		}

		static string BuildDestinationPath(string path, string rootPath, string outputFileName)
		{
			string destinationPath;

			if (!string.IsNullOrEmpty(Options.OutputPath))
			{
				if (Options.PreserveDirectoryStructure)
				{
					string subPath = Path.GetDirectoryName(path);
					subPath = subPath.Replace(rootPath, "");
					subPath = subPath.Replace(AppDomain.CurrentDomain.BaseDirectory, "");
					if (subPath.Any() && (subPath.First() == Path.DirectorySeparatorChar || subPath.First() == Path.AltDirectorySeparatorChar))
					{
						subPath = subPath.Substring(1);
					}

					destinationPath = Path.Combine(Options.OutputPath, subPath, outputFileName);
				}
				else
				{
					destinationPath = Path.Combine(Options.OutputPath, outputFileName);
				}
			}
			else if (!string.IsNullOrEmpty(Path.GetDirectoryName(path)))
			{
				destinationPath = Path.Combine(Path.GetDirectoryName(path), outputFileName);
			}
			else
			{
				destinationPath = outputFileName;
			}

			return destinationPath;
		}

		static string Decode(string response, string key)
		{
			if (string.IsNullOrEmpty(response)) { throw new ArgumentException("response is null or empty.", "response"); }
			if (string.IsNullOrEmpty(key)) { throw new ArgumentException("password is null or empty.", "response"); }

			string body = ParseBody(response);

			body = body.Replace("\\/", "/");

			ValidateBase64String(body);

			byte[] bodyBytes = Convert.FromBase64String(body);
			byte[] keyBytes = Encoding.UTF8.GetBytes(key);
			byte[] decryptedBytes = DesDecrypt(bodyBytes, keyBytes);
			string decoded = Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
			string output = decoded.Substring(16);

			if (Options.PerformCRC)
			{
				string incomingCrc = decoded.Substring(0, 8);
				string unknown = decoded.Substring(7, 8);
				uint computedCrc = ComputeCrc32(output);

				if (incomingCrc.Equals(computedCrc.ToString("X8"), StringComparison.OrdinalIgnoreCase))
				{
					throw new Exception(string.Format("CRC don't match. {0} != {1}", incomingCrc, computedCrc.ToString("X8")));
				}
			}

			return output;
		}

		static string ParseBody(string response)
		{
			const string startSentry = "\"body\":\"";
			const string endSentry = "\",\"headers\":{\"";
			int startIndex = response.IndexOf(startSentry) + startSentry.Length;
			int endIndex = response.LastIndexOf(endSentry);

			if (startIndex != -1 && endIndex != -1 && startIndex < endIndex)
			{
				return response.Substring(startIndex, endIndex - startIndex);
			}
			// assume it is extracted already.
			else
			{
				return response;
			}
		}

		static byte[] DesDecrypt(byte[] input, byte[] key)
		{
			byte[] output = new byte[input.Length];

			for (int i = 0; i < input.Length; ++i)
			{
				output[i] = (byte)((input[i] ^ key[i % key.Length]) & 0xFF);
			}

			return output;
		}

		// TODO
		static uint ComputeCrc32(string input)
		{
			throw new NotImplementedException ();
		}

		static void ValidateBase64String(string input)
		{
			if (input.Length == 0) { throw new Exception("Base64 string is empty."); }
			if ((input.Length % 4) != 0) { throw new Exception("Base64 string length is not a multiple of 4."); }

			int i = input.Length;
			for (; i > 0; --i)
			{
				if (input[i - 1] != '=')
				{
					break;
				}
				else if (input.Length - i > 2)
				{
					throw new Exception("Base64 string contains more than 2 padding characters.");
				}
			}

			for (; i > 0; --i)
			{
				char chr = input[i - 1];

				if (char.IsWhiteSpace(chr))
				{
					continue;
				}
				else if (!Constants.ValidBase64CharsString.Contains(chr))
				{
					throw new Exception(string.Format("Base64 string contains illegal character `{0}` at {1}.", chr, i - 1));
				}
			}
		}

		internal static class Constants
		{
			internal const string ValidBase64CharsString =
				"ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
				"abcdefghijklmnopqrstuvwxyz" +
				"0123456789" +
				"+/";
		}
	}

	internal static class StringExtensionMethods
	{
		public static readonly string[] trueValues = new string[] { "1", "true"/*, "on", "yes",*/ };
		public static readonly string[] falseValues = new string[] { "0", "false"/*, "off", "no",*/ };

		public static bool ToBoolean(this string value)
		{
			if (falseValues.Contains(value, StringComparer.InvariantCultureIgnoreCase))
			{
				return false;
			}
			else if (trueValues.Contains(value, StringComparer.InvariantCultureIgnoreCase))
			{
				return true;
			}
			else
			{
				throw new ArgumentException("Cannot convert boolean from value string.", "value");
			}
		}

		public static string StripDoubleQuotes(this string value)
		{
			return value.StripDelimiters('\"', '\"');
		}

		public static string StripDelimiters(this string value, char startDelimiterChar, char endDelimiterChar)
		{
			if (value.Length >= 2 && value.First() == startDelimiterChar && value.Last() == endDelimiterChar)
			{
				return value.Substring(1, value.Length - 2);
			}
			else
			{
				return value;
			}
		}

		public static string JsonIndentString = "    ";

		// https://stackoverflow.com/questions/4580397/json-formatter-in-c
		public static string PrettifyJson(this string json)
		{
			int indentation = 0;
			int quoteCount = 0;
			var result =
				from ch in json
				let quotes = ch == '"' ? quoteCount++ : quoteCount
				let lineBreak = ch == ',' && quotes % 2 == 0 ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(JsonIndentString, indentation)) : null
				let openChar = ch == '{' || ch == '[' ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(JsonIndentString, ++indentation)) : ch.ToString()
				let closeChar = ch == '}' || ch == ']' ? Environment.NewLine + String.Concat(Enumerable.Repeat(JsonIndentString, --indentation)) + ch : ch.ToString()
				select lineBreak ?? (openChar.Length > 1
								? openChar
								: closeChar);

			return String.Concat(result);
		}
	}

	internal static class Options
	{
		public static bool WaitForUserInput = true;
		public static int VerboseLevel = 3;
		public static string InputPath = "";
		public static string OutputPath = "out";
		public static string FilePattern = "*.txt";
		public static string DirectoryPattern = "*";
		public static bool PreserveDirectoryStructure = true;
		public static string DataEncryptionKey = "";
		public static bool PrettifyJson = true;
		public static bool PerformCRC = false;
		public static string ShortOptionPrefix = "/";
		public static string LongOptionPrefix = "/";
		public static bool IsOptionCaseSensitive = false;
	}
}
