using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Ash.OIUtils.ThreeArchiveTool
{
	class Program
	{
		static void Main(string[] args)
		{
			ProcessProgramArguments(args);

			if ((args.Length == 0) || (args.Length == args.Count(x => x.StartsWith("/"))))
			{
				ProcessDirectUserInput();
			}

			if (Options.WaitForUserInput)
			{
				WaitForUserInput();
			}
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
					if (arg.StartsWith("/"))
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

		/// <example>
		/// I prefer the "/verb=argument" syntax; but the more common "-verb argument" syntax
		/// could be supported by simply doing this instead:
		/// <code>
		/// Options.InputPath = args[++argIndex];
		/// </code>
		/// </example>
		static void ProcessOption(string[] args, ref int argIndex)
		{
			string arg = args[argIndex];

			if (arg.StartsWith("/help", StringComparison.InvariantCultureIgnoreCase))
			{
				Console.Out.WriteLine("");
				Console.Out.WriteLine("[options...] <[file|directory]...>");
				Console.Out.WriteLine("");
				//                     ....|....1....|....2....|....3....|....4....|....5....|....6....|....7....|....8
				Console.Out.WriteLine("available options:");
				Console.Out.WriteLine("  /inputPath=<pathname:string>");
				Console.Out.WriteLine("     Override input path (default: ``)");
				Console.Out.WriteLine("  /outputPath=<pathname:string>");
				Console.Out.WriteLine("     Override output path (default: `out`)");
				Console.Out.WriteLine("  /filePattern=<pattern:string>");
				Console.Out.WriteLine("     Override file pattern (default: `*.3`)");
				Console.Out.WriteLine("  /directoryPattern=<pattern:string>");
				Console.Out.WriteLine("     Override directory pattern (default: `*`)");
				Console.Out.WriteLine("  /exportData=<enable:bool>");
				Console.Out.WriteLine("     Whether to export non PNG data (default: 0)");
				Console.Out.WriteLine("  /exportSinglePixelImage=<enable:bool>");
				Console.Out.WriteLine("     Whether to export single pixel PNG's (default: 0)");
				Console.Out.WriteLine("  /preserveDirectoryStructure=<enable:bool>");
				Console.Out.WriteLine("     Preserve directory structure between input and output paths (default: 1)");
				Console.Out.WriteLine("  /verbose=<level:int>");
				Console.Out.WriteLine("     Set the level of verbosity in message output (default: 3)");
				Console.Out.WriteLine("     Where level is 0=none, 1=errors, 2=warnings, 3=traces, 4=debugging info");
				Console.Out.WriteLine("  /quiet");
				Console.Out.WriteLine("     Suppress all message output");
				Console.Out.WriteLine("  /waitForUserInput=<enable:bool>");
				Console.Out.WriteLine("     Whether to wait for user confirmation to exit the program (default: 1)");
				Console.Out.WriteLine("");
				Console.Out.WriteLine("examples:");
				Console.Out.WriteLine("  \"file.3\"");
				Console.Out.WriteLine("  /exportData=true \"file1.3\" \"file2.3\" \"file3.3\"");
				Console.Out.WriteLine("  /filePattern=\"*.*\" /verbose=4 \"files\"");
				Console.Out.WriteLine("  /outputPath=\"C:\\Users\\USER\\Pictures\\Albums\" /quiet \"Downloads\"");
				Console.Out.WriteLine("");
			}
			else if (arg.StartsWith("/waitForUserInput", StringComparison.InvariantCultureIgnoreCase))
			{
				ProcessArgument(arg, "/waitForUserInput".Length, ref Options.WaitForUserInput);
			}
			else if (arg.StartsWith("/quiet", StringComparison.InvariantCultureIgnoreCase))
			{
				Options.VerboseLevel = 0;
			}
			else if (arg.StartsWith("/verbose", StringComparison.InvariantCultureIgnoreCase))
			{
				ProcessArgument(arg, "/verbose".Length, ref Options.VerboseLevel);
			}
			else if (arg.StartsWith("/inputPath", StringComparison.InvariantCultureIgnoreCase))
			{
				ProcessArgument(arg, "/inputPath".Length, ref Options.InputPath);
				Options.InputPath = Options.InputPath.StripDoubleQuotes();
			}
			else if (arg.StartsWith("/outputPath", StringComparison.InvariantCultureIgnoreCase))
			{
				ProcessArgument(arg, "/outputPath".Length, ref Options.OutputPath);
				Options.OutputPath = Options.OutputPath.StripDoubleQuotes();
			}
			else if (arg.StartsWith("/filePattern", StringComparison.InvariantCultureIgnoreCase))
			{
				ProcessArgument(arg, "/filePattern".Length, ref Options.FilePattern);
				Options.FilePattern = Options.FilePattern.StripDoubleQuotes();
			}
			else if (arg.StartsWith("/directoryPattern", StringComparison.InvariantCultureIgnoreCase))
			{
				ProcessArgument(arg, "/directoryPattern".Length, ref Options.DirectoryPattern);
				Options.DirectoryPattern = Options.DirectoryPattern.StripDoubleQuotes();
			}
			else if (arg.StartsWith("/exportData", StringComparison.InvariantCultureIgnoreCase))
			{
				ProcessArgument(arg, "/exportData".Length, ref Options.ExportUnknownData);
			}
			else if (arg.StartsWith("/exportSinglePixelImage", StringComparison.InvariantCultureIgnoreCase))
			{
				ProcessArgument(arg, "/exportSinglePixelImage".Length, ref Options.ExportSinglePixelImage);
			}
			else if (arg.StartsWith("/preserveDirectoryStructure", StringComparison.InvariantCultureIgnoreCase))
			{
				ProcessArgument(arg, "/preserveDirectoryStructure".Length, ref Options.PreserveDirectoryStructure);
			}
			else
			{
				if (Options.VerboseLevel >= 1)
				{
					Console.Error.WriteLine("*** error: unrecognized option `{0}`", arg);
				}
			}
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

			IEnumerable<string> files = Directory.EnumerateFiles(path, Options.FilePattern, SearchOption.TopDirectoryOnly);

			foreach (string fileName in files)
			{
				ProcessFile(fileName, rootPath);
			}

			IEnumerable<string> directories = Directory.EnumerateDirectories(path, Options.DirectoryPattern, SearchOption.TopDirectoryOnly);

			foreach (string directoryName in directories)
			{
				ProcessDirectory(directoryName, rootPath);
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

			byte[] buffer = new byte[4];

			using (var inStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				if (inStream.Read(buffer, 0, 4) < 4) { throw new Exception("could not read file count."); }
				int fileCount = (int)BitConverter.ToUInt32(buffer, 0).FromLittleEndian();
				if (fileCount == 0)
				{
					return;
				}

				int[] fileLengths = new int[fileCount];

				for (int i = 0; i < fileCount; ++i)
				{
					if (inStream.Read(buffer, 0, 4) < 4) { throw new Exception(string.Format("could not read length of file `{0}`.", i)); }
					fileLengths[i] = (int)BitConverter.ToUInt32(buffer, 0).FromLittleEndian();
				}

				int counterLength = (fileCount < 10) ? 1 : (fileCount < 100) ? 2 : (fileCount < 1000) ? 3 : 4;

				for (int i = 0; i < fileCount; ++i)
				{
					bool skip = false;
					ImageHeaderChunk imageHeader = null;

					if (i == 0 && !Options.ExportUnknownData)
					{
						skip = true;
					}
					else if (!Options.ExportSinglePixelImage)
					{
						if (TryPeekImageHeaderChunk(inStream, out imageHeader))
						{
							if (imageHeader.Width == 1 && imageHeader.Height == 1)
							{
								skip = true;

								StartFileProcessing(skip, counterLength, i, fileCount);
								EndFileProcessing(imageHeader, fileLengths[i]);
							}
						}
					}

					if (skip)
					{
						inStream.Seek(fileLengths[i], SeekOrigin.Current);
						continue;
					}

					string destinationPath = BuildDestinationPath(path, rootPath, string.Concat(Path.GetFileNameWithoutExtension(path), "_", i.ToString(), i == 0 && Options.ExportUnknownData ? ".dat" : ".png"));

					Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

					using (var outStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
					{
						StartFileProcessing(skip, counterLength, i, fileCount);

						inStream.SubCopyTo(outStream, fileLengths[i]);

						EndFileProcessing(imageHeader, fileLengths[i]);
					}
				}
			}
		}

		static void StartFileProcessing(bool skip, int counterLength, int i, int fileCount)
		{
			if (Options.VerboseLevel >= 4)
			{
				Console.Out.Write($"  {{0}}  {{1,{counterLength}}}/{{2,{counterLength}}}...",
					skip ? "<-  " : "  ->", i, fileCount - 1);
			}
		}

		static void EndFileProcessing(ImageHeaderChunk imageHeader, int fileLength)
		{
			if (Options.VerboseLevel >= 4)
			{
				if (imageHeader != null)
				{
					Console.Out.Write("  {0,-4}  {1,-4}  {2,-2}  {3,-2}  {4,-2}  {5,-2}  {6,-2}",
						imageHeader.Width, imageHeader.Height, imageHeader.BitDepth, imageHeader.ColorType, imageHeader.CompressionMethod, imageHeader.FilterMethod, imageHeader.InterlaceMethod);
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

		static bool TryPeekImageHeaderChunk(Stream stream, out ImageHeaderChunk imageHeader)
		{
			long position = stream.Position;

			bool result = TryReadImageHeaderChunk(stream, out imageHeader);

			stream.Seek(position, SeekOrigin.Begin);

			return result;
		}

		static bool TryReadImageHeaderChunk(Stream stream, out ImageHeaderChunk imageHeader)
		{
			try
			{
				byte[] buffer = new byte[8];
				if (stream.Read(buffer, 0, 8) < 8) { throw new Exception("could not read file signature."); }
				if (!buffer.Match(0, 8, Constants.PngFileSignature)) { throw new Exception("invalid file signature."); }

				imageHeader = new ImageHeaderChunk(stream);
			}
			catch (Exception ex)
			{
				if (Options.VerboseLevel >= 5)
				{
					Console.Error.WriteLine("*** error: {0}", ex.Message);
				}

				imageHeader = null;

				return false;
			}

			return true;
		}

		internal static class Constants
		{
			public static readonly byte[] PngFileSignature = { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };
		}
	}

	internal class Chunk
	{
		public bool IsValid { get; protected set; }
		public uint DataLength { get; protected set; }
		public virtual string Type { get; protected set; }
		public uint Checksum { get; protected set; }

		public Chunk(Stream stream)
		{
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

			if (this.ReadData(stream, (int)dataLength) < this.DataLength) { throw new Exception("failed to read data."); }

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

	internal class ImageHeaderChunk : Chunk
	{
		private string type = "IHDR";

		public ImageHeaderChunk(Stream stream)
			: base(stream)
		{

		}

		public override string Type { get => this.type; protected set => this.type = value; }
		public int Width { get; protected set; }
		public int Height { get; protected set; }
		public int BitDepth { get; protected set; }
		public int ColorType { get; protected set; }
		public int CompressionMethod { get; protected set; }
		public int FilterMethod { get; protected set; }
		public int InterlaceMethod { get; protected set; }

		protected override bool ValidateType(string type)
		{
			return type.Equals(this.Type, StringComparison.Ordinal);
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

		public static UInt32 ByteSwap(UInt32 value)
		{
			return ((value & 0xFF) << 24)
				| (((value >> 8) & 0xFF) << 16)
				| (((value >> 16) & 0xFF) << 8)
				| ((value >> 24) & 0xFF);
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
	}

	internal static class Options
	{
		public static bool WaitForUserInput = true;
		public static int VerboseLevel = 3;
		public static string InputPath = "";
		public static string OutputPath = "out";
		public static string FilePattern = "*.3";
		public static string DirectoryPattern = "*";
		public static bool PreserveDirectoryStructure = true;
		public static bool ExportUnknownData = false;
		public static bool ExportSinglePixelImage = false;
	}
}
