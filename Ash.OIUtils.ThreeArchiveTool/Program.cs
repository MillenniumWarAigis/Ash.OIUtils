using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ash.OIUtils.ThreeArchiveTool
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
				Console.Out.Write("Enter either nothing, an option, a file or directory name: ");

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
				Console.Out.WriteLine("  /i or /inputPath=<pathname:string>");
				Console.Out.WriteLine("     Override input path (default: ``)");
				Console.Out.WriteLine();
				Console.Out.WriteLine("  /o or /outputPath=<pathname:string>");
				Console.Out.WriteLine("     Override output path (default: `out`)");
				Console.Out.WriteLine();
				Console.Out.WriteLine("  /f or /filePattern=<pattern:string>");
				Console.Out.WriteLine("     Override file pattern (default: `*.0|*.1|*.2|*.3|*.4|*.5|*.6|*.7|*.8|*.9`)");
				Console.Out.WriteLine("     Use | to split multiple patterns");
				Console.Out.WriteLine();
				Console.Out.WriteLine("  /d or /directoryPattern=<pattern:string>");
				Console.Out.WriteLine("     Override directory pattern (default: `*`)");
				Console.Out.WriteLine("     Use | to split multiple patterns");
				Console.Out.WriteLine();
				Console.Out.WriteLine("  /u or /exportData=<enable:bool>");
				Console.Out.WriteLine("     Whether to export non unknown data entries (default: 0)");
				Console.Out.WriteLine();
				Console.Out.WriteLine("  /s or /exportSinglePixelImage=<enable:bool>");
				Console.Out.WriteLine("     Whether to export single pixel images (default: 0)");
				Console.Out.WriteLine();
				Console.Out.WriteLine("  /e or /excludeImageSizes=<sizes:size_array>");
				Console.Out.WriteLine("     Exclude images matching those exact dimensions (default: 1x1|4x4)");
				Console.Out.WriteLine();
				Console.Out.WriteLine("  /x or /password=<enable:string>");
				Console.Out.WriteLine("     Sets the decryption key (required to decrypt first data entry)");
				Console.Out.WriteLine();
				Console.Out.WriteLine("        /prettify=<enable:bool>");
				Console.Out.WriteLine("     Whether to format the json output (default: 1)");
				Console.Out.WriteLine();
				Console.Out.WriteLine("  /t or /preserveDirectoryStructure=<enable:bool>");
				Console.Out.WriteLine("     Preserve directory structure between input and output paths (default: 1)");
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
				Console.Out.WriteLine("examples:");
				Console.Out.WriteLine("");
				Console.Out.WriteLine("  \"file.3\"");
				Console.Out.WriteLine("  /exportData=true \"file1.3\" \"file2.3\" \"file3.3\"");
				Console.Out.WriteLine("  /filePattern=\"*.*\" /verbose=4 \"files\"");
				Console.Out.WriteLine("  /outputPath=\"C:\\Users\\USER\\Pictures\\Albums\" /quiet \"Downloads\"");
				Console.Out.WriteLine("");
			}
			else if (TryMatchArgument(arg, "w", "waitForUserInput", out length))
			{
				ProcessArgument(arg, length, ref Options.WaitForUserInput);
			}
			else if (TryMatchArgument(arg, "q", "quiet", out length))
			{
				Options.VerboseLevel = 0;
			}
			else if (TryMatchArgument(arg, "v", "verbose", out length))
			{
				ProcessArgument(arg, length, ref Options.VerboseLevel);
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
			else if (TryMatchArgument(arg, "u", "exportData", out length))
			{
				ProcessArgument(arg, length, ref Options.ExportUnknownData);
			}
			else if (TryMatchArgument(arg, "s", "exportSinglePixelImage", out length))
			{
				ProcessArgument(arg, length, ref Options.ExportSinglePixelImage);
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
			else if (TryMatchArgument(arg, "e", "excludeImageSizes", out length))
			{
				// quick hack per request!

				StringBuilder sb = new StringBuilder();

				for (int i = 0; i < Options.ExcludedImageWidths.Length; ++i)
				{
					if (i != 0) { sb.Append('|'); }
					sb.AppendFormat("{0}x{1}", Options.ExcludedImageWidths[i], Options.ExcludedImageHeights[i]);
				}

				string value = sb.ToString();

				ProcessArgument(arg, length, ref value);

				string[] sizes = value.Split(new char[] { '|' });

				Options.ExcludedImageWidths = new int[sizes.Length];
				Options.ExcludedImageHeights = new int[sizes.Length];

				for (int i = 0; i < sizes.Length; ++i)
				{
					string size = sizes[i];
					string[] sizeComponents = sizes[i].Split(new char[] { 'x' });

					Options.ExcludedImageWidths[i] = int.Parse(sizeComponents[0]);
					Options.ExcludedImageWidths[i] = int.Parse(sizeComponents[1]);
				}
			}

			else
			{
				if (Options.VerboseLevel >= 1)
				{
					Console.Error.WriteLine("*** error: unrecognized option `{0}`", arg);
				}
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

			string pathFileExtension = Path.GetExtension(path);
			int archiveType = 0;
			bool IsValidArchive = pathFileExtension.First() == '.' && int.TryParse(pathFileExtension.Substring(1), out archiveType);

			if (!IsValidArchive)
			{
				if (Options.VerboseLevel >= 6)
				{
					Console.Error.WriteLine("  skipping...");
				}
				return;
			}

			byte[] buffer = new byte[4];

			using (var inStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				if (inStream.Read(buffer, 0, 4) < 4) { throw new Exception("could not read file count."); }
				int fileCount = (int)BitConverter.ToUInt32(buffer, 0).FromLittleEndian();
				if (fileCount == 0)
				{
					return;
				}

				int fileCountLengthBase10 = CountLength(fileCount, 10);
				int fileLengthLengthBase16 = CountLength((int)inStream.Length, 16);
				int[] fileLengths = new int[fileCount];

				for (int i = 0; i < fileCount; ++i)
				{
					if (inStream.Read(buffer, 0, 4) < 4) { throw new Exception(string.Format("could not read length of file `{0}`.", i)); }
					fileLengths[i] = (int)BitConverter.ToUInt32(buffer, 0).FromLittleEndian();
				}

				for (int i = 0; i < fileCount; ++i)
				{
					bool skip = false;
					PngImageHeaderChunk pngImageHeader = null;
					JfifStartOfFrame0Segment jfifSof0 = null;
					bool hasStartOfImageMarker = false;
					Mp3Tag mp3Tag = null;
					string json = null;
					int imageWidth = 0;
					int imageHeight = 0;
					int[] checkOrder;
					if (archiveType == 1)
					{
						checkOrder = new int[] { 2, 0, 1, 3 };
					}
					else if (archiveType == 6)
					{
						checkOrder = new int[] { 1, 0, 2, 3 };
					}
					else
					{
						checkOrder = new int[] { 0, 1, 2, 3 };
					}

					// archive 1 and 8 don't have any json.
					// archive 2 has a couple of json but mainly png's
					// others have only 1 json as the first entry.
					if (i == 0 && archiveType != 1 && archiveType != 8)
					{
						TryPeekOrReadJson(inStream, fileLengths[i], Options.DataEncryptionKey, out json);
					}

					if (json == null)
					{
						for (int k = 0; k < checkOrder.Length; ++k)
						{
							if (checkOrder[k] == 0 && TryPeekPngImageHeaderChunk(inStream, out pngImageHeader))
							{
								imageWidth = pngImageHeader.Width;
								imageHeight = pngImageHeader.Height;
								break;
							}
							else if (checkOrder[k] == 1 && TryPeekJfif(inStream, out jfifSof0, out hasStartOfImageMarker) || hasStartOfImageMarker)
							{
								if (jfifSof0 != null)
								{
									imageWidth = jfifSof0.Width;
									imageHeight = jfifSof0.Height;
								}
								break;
							}
							else if (checkOrder[k] == 2 && TryPeekMp3Header(inStream, out mp3Tag))
							{
								break;
							}
							else if (checkOrder[k] == 3 && TryPeekOrReadJson(inStream, fileLengths[i], Options.DataEncryptionKey, out json))
							{
								break;
							}
						}
					}

					if (i == 0 && !Options.ExportUnknownData
						&& json == null && pngImageHeader == null && jfifSof0 == null && !hasStartOfImageMarker && mp3Tag == null)
					{
						skip = true;
					}
					else if (!Options.ExportSinglePixelImage)
					{
						if (Options.ExcludedImageWidths.Contains(imageWidth)
							&& Options.ExcludedImageHeights.Contains(imageHeight))
						{
							skip = true;

							StartFileProcessing(skip, fileCountLengthBase10, i, fileCount);
							EndFileProcessing(json, pngImageHeader, jfifSof0, hasStartOfImageMarker, mp3Tag, (int)inStream.Position, fileLengths[i], fileLengthLengthBase16);
						}
					}

					if (skip)
					{
						inStream.Seek(fileLengths[i], SeekOrigin.Current);
						continue;
					}

					string destinationPath = BuildDestinationPath(path, rootPath, string.Concat(Path.GetFileNameWithoutExtension(path), "_", i.ToString(),
						(pngImageHeader != null) ? ".png"
						: (jfifSof0 != null) || hasStartOfImageMarker ? ".jpg"
						: (mp3Tag != null) ? ".mp3"
						: (json != null) ? ".json"
						: ".dat"));

					Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

					using (var outStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
					{
						StartFileProcessing(skip, fileCountLengthBase10, i, fileCount);

						if (json != null)
						{
							if (Options.PrettifyJson)
							{
								try { json = json.PrettifyJson(); }
								catch (Exception) { }
							}

							byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

							outStream.Write(jsonBytes, 0, jsonBytes.Length);
						}
						else
						{
							inStream.SubCopyTo(outStream, fileLengths[i]);
						}

						EndFileProcessing(json, pngImageHeader, jfifSof0, hasStartOfImageMarker, mp3Tag, (int)(inStream.Position - fileLengths[i]), fileLengths[i], fileLengthLengthBase16);
					}
				}
			}
		}

		static int CountLength(int value, int radix)
		{
			if (radix == 0) { throw new ArgumentException("radix is zero.", "radix"); }

			int i = 0;
			for (; value > 0; i++, value /= radix) { }

			return i;
		}

		static void StartFileProcessing(bool skip, int fileCountLengthBase10, int i, int fileCount)
		{
			if (Options.VerboseLevel >= 4)
			{
				Console.Out.Write($"  {{0}}  {{1,{fileCountLengthBase10}}}/{{2,{fileCountLengthBase10}}}...",
					skip ? "<-  " : "  ->", i, fileCount - 1);
			}
		}

		static void EndFileProcessing(string json, PngImageHeaderChunk pngImageHeader, JfifStartOfFrame0Segment jfifSof0, bool hasStartOfImageMarker, Mp3Tag mp3Tag, int startPosition, int fileLength, int fileCountLengthBase16)
		{
			if (Options.VerboseLevel >= 4)
			{
				if (pngImageHeader != null)
				{
					Console.Out.Write(" PNG {0,-4}  {1,-4}  {2,-2}  {3,-2}  {4,-2}  {5,-2}  {6,-2}",
						pngImageHeader.Width, pngImageHeader.Height, pngImageHeader.BitDepth, pngImageHeader.ColorType, pngImageHeader.CompressionMethod, pngImageHeader.FilterMethod, pngImageHeader.InterlaceMethod);
				}
				else if (jfifSof0 != null)
				{
					Console.Out.Write(" JPG {0,-4}  {1,-4}  {2,-2}  {3,-14}",
						jfifSof0.Width, jfifSof0.Height, jfifSof0.BitDepth, " ");
				}
				else if (hasStartOfImageMarker)
				{
					Console.Out.Write(" JPG {0,-30}", " ");
				}
				else if (mp3Tag != null)
				{
					Console.Out.Write(" MP3 {0,-30}", " ");
				}
				else if (json != null)
				{
					Console.Out.Write(" JSON{0,-30}", " ");
				}
				else
				{
					Console.Out.Write(" DAT {0,-30}", " ");
				}
				if (Options.VerboseLevel >= 5)
				{
					Console.Out.Write($" [{{0:X{fileCountLengthBase16}}},{{1:X{fileCountLengthBase16}}}]", startPosition, startPosition + fileLength - 1);
				}
				Console.Out.WriteLine("  ({0} bytes)", fileLength);
			}
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


		static bool TryPeekOrReadJson(Stream stream, int length, string key, out string json)
		{
			long position = stream.Position;
			bool result = false;

			try
			{
				ValidateKey(key);

				json = DecodeJson(stream, length, key);
				result = true;
			}
			catch (Exception ex)
			{
				if (Options.VerboseLevel >= 5)
				{
					Console.Error.WriteLine("*** error: {0}", ex.Message);
				}

				json = null;
				stream.Seek(position, SeekOrigin.Begin);
			}

			return result;
		}

		static string DecodeJson(Stream stream, int length, string key)
		{
			byte[] buffer = new byte[length];
			if (stream.Read(buffer, 0, length) < length) { throw new Exception("could not read json data."); }

			byte[] keyBytes = Encoding.UTF8.GetBytes(key);
			byte[] decryptedBytes = DesDecrypt(buffer, keyBytes);
			string output = Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);

			return output;
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

		static void ValidateKey(string key)
		{
			if (string.IsNullOrEmpty(key)) { throw new Exception("data cannot be decrypted without a key. Please specify one with /x"); }
		}

		static bool TryPeekJfif(Stream stream, out JfifStartOfFrame0Segment startOfFrame0, out bool hasStartOfImageMarker)
		{
			long position = stream.Position;

			bool result = TryReadJfif(stream, out startOfFrame0, out hasStartOfImageMarker);

			stream.Seek(position, SeekOrigin.Begin);

			return result;
		}

		static bool TryReadJfif(Stream stream, out JfifStartOfFrame0Segment startOfFrame0, out bool hasStartOfImageMarker)
		{
			hasStartOfImageMarker = false;

			try
			{
				for (int i = 0; ; ++i)
				{
					byte marker = JfifSegment.ReadMarker(stream);
					JfifSegment segment = null;

					// note that the following two markers could be moved out of the loop,
					// but I prefer to keep it here for cleaner code.
					if (i == 0)
					{
						if (marker != JfifStartOfImageSegment.Constants.Marker) { throw new Exception("first marker is not start of image."); }

						hasStartOfImageMarker = true;
					}
					else if (i == 1)
					{
						try
						{
							if (marker == JfifApp0Segment.Constants.Marker)
							{
								segment = new JfifApp0Segment(stream, marker);
							}
							else
							{
								throw new NotImplementedException(string.Format("application version marker 0x{0:X2) is not supported.", marker));
							}
						}
						catch (NotImplementedException ex)
						{
							throw ex;
						}
						catch (Exception ex)
						{
							Console.Error.WriteLine("*** error: {0}", ex.Message);
							throw new Exception("invalid app marker segment.");
						}
					}
					else if (marker == JfifStartOfScanSegment.Constants.Marker)
					{
						// since we're looking for SOF's, we shouldn't have hit a SOS.
						throw new Exception("unexpected start of scan.");
					}
					else if (marker == JfifEndOfImageSegment.Constants.Marker)
					{
						// since we're looking for SOF's, we shouldn't have hit a EOI.
						throw new Exception("unexpected end of image.");
					}
					else if (marker == JfifStartOfFrame0Segment.Constants.Marker)
					{
						try
						{
							segment = new JfifStartOfFrame0Segment(stream, marker);

							startOfFrame0 = (JfifStartOfFrame0Segment)segment;
						}
						catch (Exception ex)
						{
							Console.Error.WriteLine("*** error: {0}", ex.Message);
							throw new Exception("invalid sof0 marker segment.");
						}

						break;
					}
					else if (marker == JfifStartOfFrame1Segment.Constants.Marker)
					{
						try
						{
							segment = new JfifStartOfFrame1Segment(stream, marker);

							startOfFrame0 = (JfifStartOfFrame1Segment)segment;
						}
						catch (Exception ex)
						{
							Console.Error.WriteLine("*** error: {0}", ex.Message);
							throw new Exception("invalid sof0 marker segment.");
						}

						break;
					}
					else if (marker == JfifStartOfFrame2Segment.Constants.Marker)
					{
						try
						{
							segment = new JfifStartOfFrame2Segment(stream, marker);

							startOfFrame0 = (JfifStartOfFrame2Segment)segment;
						}
						catch (Exception ex)
						{
							Console.Error.WriteLine("*** error: {0}", ex.Message);
							throw new Exception("invalid sof2 marker segment.");
						}

						break;
					}
					else
					{
						try
						{
							segment = new JfifSegment(stream, marker);
						}
						catch (Exception ex)
						{
							Console.Error.WriteLine("*** error: {0}", ex.Message);
							throw new Exception("invalid unknown segment.");
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (Options.VerboseLevel >= 5)
				{
					Console.Error.WriteLine("*** error: {0}", ex.Message);
				}

				startOfFrame0 = null;

				return false;
			}

			return true;
		}

		static bool TryPeekPngImageHeaderChunk(Stream stream, out PngImageHeaderChunk pngImageHeader)
		{
			long position = stream.Position;

			bool result = TryReadPngImageHeaderChunk(stream, out pngImageHeader);

			stream.Seek(position, SeekOrigin.Begin);

			return result;
		}

		static bool TryReadPngImageHeaderChunk(Stream stream, out PngImageHeaderChunk pngImageHeader)
		{
			try
			{
				byte[] buffer = new byte[8];
				if (stream.Read(buffer, 0, 8) < 8) { throw new Exception("could not read png file signature."); }
				if (!buffer.Match(0, 8, Constants.PngFileSignature)) { throw new Exception("invalid png file signature."); }

				pngImageHeader = new PngImageHeaderChunk(stream);
			}
			catch (Exception ex)
			{
				if (Options.VerboseLevel >= 5)
				{
					Console.Error.WriteLine("*** error: {0}", ex.Message);
				}

				pngImageHeader = null;

				return false;
			}

			return true;
		}

		static bool TryPeekMp3Header(Stream stream, out Mp3Tag mp3Tag)
		{
			long position = stream.Position;

			bool result = TryReadMp3HeaderChunk(stream, out mp3Tag);

			stream.Seek(position, SeekOrigin.Begin);

			return result;
		}

		static bool TryReadMp3HeaderChunk(Stream stream, out Mp3Tag mp3Tag)
		{
			try
			{
				byte[] buffer = new byte[3];
				if (stream.Read(buffer, 0, 3) < 3) { throw new Exception("could not read mp3 file signature."); }
				if (!buffer.Match(0, 3, Constants.Mp3FileSignature)) { throw new Exception("invalid mp3 file signature."); }

				mp3Tag = new Mp3Tag(stream);
			}
			catch (Exception ex)
			{
				if (Options.VerboseLevel >= 5)
				{
					Console.Error.WriteLine("*** error: {0}", ex.Message);
				}

				mp3Tag = null;

				return false;
			}

			return true;
		}

		internal static class Constants
		{
			public static readonly byte[] PngFileSignature = { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };
			public static readonly byte[] Mp3FileSignature = { 0x49, 0x44, 0x33 };
		}
	}

	// dummy class, let's not bother outputting any info about it.
	internal class Mp3Tag
	{
		public Mp3Tag(Stream stream)
		{
			Read(stream);
		}

		private int Read(Stream stream)
		{
			return 0;
		}
	}

	internal class PngChunk
	{
		public bool IsValid { get; protected set; }
		public uint DataLength { get; protected set; }
		public string Type { get; protected set; }
		public uint Checksum { get; protected set; }

		public PngChunk(Stream stream)
		{
			// note that it's bad design to have virtual calls in constructors
			// but it should be fine in this small program.
			Read(stream);
		}

		private int Read(Stream stream)
		{
			byte[] buffer = new byte[4];

			if (stream.Read(buffer, 0, 4) < 4) { throw new Exception("failed to read data length."); }
			uint dataLength = BitConverter.ToUInt32(buffer, 0).FromBigEndian();

			if (stream.Read(buffer, 0, 4) < 4) { throw new Exception("failed to read chunk type."); }
			string type = new string(Encoding.ASCII.GetChars(buffer, 0, 4));
			if (!this.ValidateType(type)) { throw new Exception("invalid chunk type."); }

			if (this.ReadData(stream, (int)dataLength) < dataLength) { throw new Exception("failed to read data."); }

			if (stream.Read(buffer, 0, 4) < 4) { throw new Exception("failed to read checksum."); }
			uint checksum = BitConverter.ToUInt32(buffer, 0).FromBigEndian();

			this.DataLength = dataLength;
			this.Type = type;
			this.Checksum = checksum;
			this.IsValid = true;

			return 4 + 4 + (int)this.DataLength + 4;
		}

		protected virtual bool ValidateType(string type)
		{
			return true;
		}

		protected virtual int ReadData(Stream stream, int dataLength)
		{
			long currentPosition = stream.Position;
			long newPosition = stream.Seek(dataLength, SeekOrigin.Current);

			return (int)(newPosition - currentPosition);
		}
	}

	internal class PngImageHeaderChunk : PngChunk
	{
		public class Constants
		{
			public static string Type = "IHDR";
		}

		public PngImageHeaderChunk(Stream stream)
			: base(stream)
		{

		}

		public int Width { get; protected set; }
		public int Height { get; protected set; }
		public int BitDepth { get; protected set; }
		public int ColorType { get; protected set; }
		public int CompressionMethod { get; protected set; }
		public int FilterMethod { get; protected set; }
		public int InterlaceMethod { get; protected set; }

		protected override bool ValidateType(string type)
		{
			return type.Equals(Constants.Type, StringComparison.Ordinal);
		}

		protected override int ReadData(Stream stream, int dataLength)
		{
			byte[] buffer = new byte[4];

			if (stream.Read(buffer, 0, 4) < 4) { throw new Exception("failed to read width."); }
			int width = (int)BitConverter.ToUInt32(buffer, 0).FromBigEndian();

			if (stream.Read(buffer, 0, 4) < 4) { throw new Exception("failed to read height."); }
			int height = (int)BitConverter.ToUInt32(buffer, 0).FromBigEndian();

			if (stream.Read(buffer, 0, 1) < 1) { throw new Exception("failed to read bit depth."); }
			byte bitDepth = buffer[0];

			if (stream.Read(buffer, 0, 1) < 1) { throw new Exception("failed to read bit color type."); }
			byte colorType = buffer[0];

			if (stream.Read(buffer, 0, 1) < 1) { throw new Exception("failed to read bit compression method."); }
			byte compressionMethod = buffer[0];

			if (stream.Read(buffer, 0, 1) < 1) { throw new Exception("failed to read bit filter method."); }
			byte filterMethod = buffer[0];

			if (stream.Read(buffer, 0, 1) < 1) { throw new Exception("failed to read bit interlace method."); }
			byte interlaceMethod = buffer[0];

			this.Width = width;
			this.Height = height;
			this.BitDepth = bitDepth;
			this.ColorType = colorType;
			this.CompressionMethod = compressionMethod;
			this.FilterMethod = filterMethod;
			this.InterlaceMethod = interlaceMethod;

			return dataLength;
		}
	}

	internal class JfifSegment
	{
		public bool IsValid { get; protected set; }
		public byte Marker { get; protected set; }
		public virtual int DataLength { get; protected set; }

		public JfifSegment(Stream stream, byte marker)
		{
			// note that it's bad design to have virtual calls in constructors
			// but it should be fine in this small program.
			Read(stream, marker);
		}

		static public byte ReadMarker(Stream stream)
		{
			byte[] buffer = new byte[2];
			if (stream.Read(buffer, 0, 2) < 2) { throw new Exception("could not read marker."); }
			if (buffer[0] != 0xFF) { throw new Exception("first byte of marker is not 0xFF."); }

			return buffer[1];
		}

		private int Read(Stream stream, byte marker)
		{
			if (!this.ValidateMarker(marker)) { throw new Exception("invalid marker."); }

			byte[] buffer = new byte[2];
			if (stream.Read(buffer, 0, 2) < 2) { throw new Exception("failed to read marker length."); }
			ushort dataLength = BitConverter.ToUInt16(buffer, 0).FromBigEndian();

			if (this.ReadData(stream, dataLength) < dataLength - 2) { throw new Exception("failed to read data."); }

			this.DataLength = dataLength;
			this.Marker = marker;
			this.IsValid = true;

			return this.DataLength;
		}

		protected virtual bool ValidateMarker(byte type)
		{
			return true;
		}

		protected virtual int ReadData(Stream stream, int dataLength)
		{
			long currentPosition = stream.Position;
			long newPosition = stream.Seek(dataLength - 2, SeekOrigin.Current);

			return (int)(newPosition - currentPosition);
		}
	}

	internal class JfifStartOfImageSegment : JfifSegment
	{
		public class Constants
		{
			public static byte Marker = 0xD8;
		}

		public JfifStartOfImageSegment(Stream stream, byte marker)
			: base(stream, marker)
		{
		}

		protected override bool ValidateMarker(byte marker)
		{
			return marker == Constants.Marker;
		}
	}

	internal class JfifEndOfImageSegment : JfifSegment
	{
		public class Constants
		{
			public static byte Marker = 0x09;
		}

		public JfifEndOfImageSegment(Stream stream, byte marker)
			: base(stream, marker)
		{
		}

		protected override bool ValidateMarker(byte marker)
		{
			return marker == Constants.Marker;
		}
	}

	internal class JfifStartOfScanSegment : JfifSegment
	{
		public class Constants
		{
			public static byte Marker = 0xDA;
		}

		// JfifSegment is currently hard-coded to read in a length field,
		// which SOS segments do not have, so prevent any instantiation of this class.
		private JfifStartOfScanSegment(Stream stream, byte marker)
			: base(stream, marker)
		{
		}

		protected override bool ValidateMarker(byte marker)
		{
			return marker == Constants.Marker;
		}
	}

	internal class JfifApp0Segment : JfifSegment
	{
		public class Constants
		{
			public static byte Marker = 0xE0;
			public static string Identifier = "JFIF";
		}

		public JfifApp0Segment(Stream stream, byte marker)
			: base(stream, marker)
		{
		}

		public string Identifier { get; protected set; }
		public ushort Version { get; protected set; }
		public byte PixelDensityUnits { get; protected set; }
		public ushort HorizontalPixelDensity { get; protected set; }
		public ushort VerticalPixelDensity { get; protected set; }
		public byte HorizontalThumbnailPixelCount { get; protected set; }
		public byte VerticalThumbnailPixelCount { get; protected set; }
		public byte[] ThumbnailPixels { get; protected set; }

		public byte MajorVersion { get => (byte)((this.Version >> 8) & 0xFF); }
		public byte MinorVersion { get => (byte)(this.Version & 0xFF); }

		protected override bool ValidateMarker(byte marker)
		{
			return marker == Constants.Marker;
		}

		protected override int ReadData(Stream stream, int dataLength)
		{
			byte[] buffer = new byte[8];

			if (stream.Read(buffer, 0, 5) < 5) { throw new Exception("failed to read identifier."); }
			string identifier = new string(Encoding.ASCII.GetChars(buffer, 0, 5));
			if (identifier.Equals(Constants.Identifier, StringComparison.Ordinal)) { throw new Exception("invalid identifier."); }

			if (stream.Read(buffer, 0, 2) < 2) { throw new Exception("failed to read version."); }
			ushort version = BitConverter.ToUInt16(buffer, 0).FromBigEndian();

			if (stream.Read(buffer, 0, 1) < 1) { throw new Exception("failed to read pixel density units."); }
			byte pixelDensityUnits = buffer[0];

			if (stream.Read(buffer, 0, 2) < 2) { throw new Exception("failed to read horizontal pixel density."); }
			ushort horizontalPixelDensity = BitConverter.ToUInt16(buffer, 0).FromBigEndian();
			if (horizontalPixelDensity == 0) { throw new Exception("horizontal pixel density is zero."); }

			if (stream.Read(buffer, 0, 2) < 2) { throw new Exception("failed to read vertical pixel density."); }
			ushort verticalPixelDensity = BitConverter.ToUInt16(buffer, 0).FromBigEndian();
			if (verticalPixelDensity == 0) { throw new Exception("vertical pixel density is zero."); }

			if (stream.Read(buffer, 0, 1) < 1) { throw new Exception("failed to read horizontal thumbnail pixel count."); }
			byte horizontalThumbnailPixelCount = buffer[0];

			if (stream.Read(buffer, 0, 1) < 1) { throw new Exception("failed to read vertical thumbnail pixel count."); }
			byte verticalThumbnailPixelCount = buffer[0];

			byte[] pixelBuffer = null;
			if (horizontalThumbnailPixelCount != 0 && verticalThumbnailPixelCount != 0)
			{
				// not needed.
				/*if (this.ReadThumbnailPixels)
				{
					pixelBuffer = new byte[horizontalThumbnailPixelCount * verticalThumbnailPixelCount * 3];
					if (stream.Read(buffer, 0, pixelBuffer.Length) < pixelBuffer.Length) { throw new Exception("failed to read thumbnail pixels."); }
				}
				else
				*/
				{
					long oldPosition = stream.Position;
					long newPosition = stream.Seek(horizontalThumbnailPixelCount * verticalThumbnailPixelCount * 3, SeekOrigin.Current);
					if (newPosition != (oldPosition + (horizontalThumbnailPixelCount * verticalThumbnailPixelCount * 3))) { throw new Exception("failed to skip thumbnail pixels."); }
				}
			}

			this.Identifier = identifier;
			this.Version = version;
			this.PixelDensityUnits = pixelDensityUnits;
			this.HorizontalPixelDensity = horizontalPixelDensity;
			this.VerticalPixelDensity = verticalPixelDensity;
			this.HorizontalThumbnailPixelCount = horizontalThumbnailPixelCount;
			this.VerticalThumbnailPixelCount = verticalThumbnailPixelCount;
			this.ThumbnailPixels = pixelBuffer;

			return dataLength;
		}
	}

	internal class JfifStartOfFrame0Segment : JfifSegment
	{
		public class Constants
		{
			public static byte Marker = 0xC0;
		}

		public JfifStartOfFrame0Segment(Stream stream, byte marker)
			: base(stream, marker)
		{

		}

		public byte BitDepth { get; protected set; }
		public ushort Width { get; protected set; }
		public ushort Height { get; protected set; }

		protected override bool ValidateMarker(byte marker)
		{
			return marker == Constants.Marker;
		}

		protected override int ReadData(Stream stream, int dataLength)
		{
			byte[] buffer = new byte[8];

			if (stream.Read(buffer, 0, 1) < 1) { throw new Exception("failed to read bit depth."); }
			byte bitDepth = buffer[0];

			if (stream.Read(buffer, 0, 2) < 2) { throw new Exception("failed to read height."); }
			ushort height = BitConverter.ToUInt16(buffer, 0).FromBigEndian();
			if (height == 0) { throw new Exception("height is zero."); }

			if (stream.Read(buffer, 0, 2) < 2) { throw new Exception("failed to read width."); }
			ushort width = BitConverter.ToUInt16(buffer, 0).FromBigEndian();
			if (width == 0) { throw new Exception("width is zero."); }

			// we don't care about the rest of the data, so just skip it.
			long oldPosition = stream.Position;
			long newPosition = stream.Seek(this.DataLength - 2 - (1 + 2 + 2), SeekOrigin.Current);
			if (newPosition != (oldPosition + (this.DataLength - 2 - (1 + 2 + 2)))) { throw new Exception("failed to skip remaining data."); }

			this.BitDepth = bitDepth;
			this.Width = width;
			this.Height = height;

			return dataLength;
		}
	}

	// TODO I can't find the specifications for SOF1 so for now, just assume it's the same as SOF0.
	internal class JfifStartOfFrame1Segment : JfifStartOfFrame0Segment
	{
		public new class Constants
		{
			public static byte Marker = 0xC1;
		}

		public JfifStartOfFrame1Segment(Stream stream, byte marker)
			: base(stream, marker)
		{

		}

		protected override bool ValidateMarker(byte marker)
		{
			return marker == Constants.Marker;
		}
	}

	// TODO I can't find the specifications for SOF2 so for now, just assume it's the same as SOF0.
	internal class JfifStartOfFrame2Segment : JfifStartOfFrame0Segment
	{
		public new class Constants
		{
			public static byte Marker = 0xC2;
		}

		public JfifStartOfFrame2Segment(Stream stream, byte marker)
			: base(stream, marker)
		{

		}

		protected override bool ValidateMarker(byte marker)
		{
			return marker == Constants.Marker;
		}
	}

	internal static class ArrayExtensionMethods
	{
		public static bool Match(this byte[] array, int startIndex, byte[] pattern)
		{
			return Match(array, startIndex, array.Length, pattern);
		}

		public static bool Match(this byte[] array, int startIndex, int endIndex, byte[] pattern)
		{
			if (pattern == null) { throw new ArgumentException("pattern is null.", "pattern"); }
			else if (startIndex < 0) { throw new ArgumentOutOfRangeException("startIndex", startIndex, "startIndex is negative."); }
			else if (endIndex < 0) { throw new ArgumentOutOfRangeException("endIndex", endIndex, "endIndex is negative."); }
			else if (startIndex > endIndex) { throw new ArgumentOutOfRangeException("endIndex", endIndex, "endIndex is less than startIndex."); }

			for (int i = 0; (i < pattern.Length) && (startIndex + i < endIndex); ++i)
			{
				if (array[startIndex + i] != pattern[i])
				{
					return false;
				}
			}

			return true;
		}
	}

	internal static class StreamExtensionMethods
	{
		public static void SubCopyTo(this Stream source, Stream destination, int count)
		{
			SubCopyTo(source, destination, count, 81920);
		}

		public static void SubCopyTo(this Stream source, Stream destination, int count, int bufferSize)
		{
			if (destination == null) { throw new ArgumentException("destination is null.", "destination"); }
			else if (bufferSize <= 0) { throw new ArgumentOutOfRangeException("bufferSize", bufferSize, "bufferSize is negative or zero."); }
			else if (!source.CanRead) { throw new NotSupportedException("The current stream does not support reading."); }
			else if (!destination.CanWrite) { throw new NotSupportedException("destination does not support writing."); }

			byte[] buffer = new byte[bufferSize];

			for (int readCount = 0; count > 0; count -= readCount)
			{
				readCount = source.Read(buffer, 0, Math.Min(buffer.Length, count));
				if (readCount == 0)
				{
					break;
				}

				destination.Write(buffer, 0, readCount);
			}
		}
	}

	internal static class EndianExtensionMethods
	{
		public static UInt32 FromLittleEndian(this UInt32 value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return value;
			}
			else
			{
				return ByteSwap(value);
			}
		}

		public static UInt32 FromBigEndian(this UInt32 value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return ByteSwap(value);
			}
			else
			{
				return value;
			}
		}

		public static UInt16 FromBigEndian(this UInt16 value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return ByteSwap(value);
			}
			else
			{
				return value;
			}
		}

		public static UInt32 ByteSwap(UInt32 value)
		{
			return ((value & 0xFF) << 24)
				| (((value >> 8) & 0xFF) << 16)
				| (((value >> 16) & 0xFF) << 8)
				| ((value >> 24) & 0xFF);
		}

		public static UInt16 ByteSwap(UInt16 value)
		{
			int a = value & 0xFF;
			int b = (value >> 8) & 0xFF;

			return (ushort)((a << 8) | b);
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
		public static string FilePattern = "*.1|*.2|*.3|*.4|*.5|*.6|*.7|*.8";
		public static string DirectoryPattern = "*";
		public static bool PreserveDirectoryStructure = true;
		public static bool ExportUnknownData = false;
		public static bool ExportSinglePixelImage = false;
		public static int[] ExcludedImageWidths = new int[] { 1, 4 };
		public static int[] ExcludedImageHeights = new int[] { 1, 4 };
		public static string DataEncryptionKey = "";
		public static bool PrettifyJson = true;
		// it's more common/standard to have those set to "-", "--", and true respectively.
		// feel free to change it to whatever you prefer.
		public static string ShortOptionPrefix = "/";
		public static string LongOptionPrefix = "/";
		public static bool IsOptionCaseSensitive = false;
	}
}
