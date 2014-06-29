using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSO2FileCheck
{
	public class FileHashList
	{
		// It might be more useful to use a dictionary here so we can look
		// up files by file name.
		private List<FileHashEntry> _files = new List<FileHashEntry>();

		public IEnumerable<FileHashEntry> Files
		{
			get { return _files; }
		}

		public DirectoryInfo RootDirectory = null;

		private FileHashList(DirectoryInfo rootDirectory, IEnumerable<FileHashEntry> files)
		{
			this.RootDirectory = rootDirectory;
			this._files = files.ToList();
		}

		/// <summary>
		/// Computes the hash of each file in a directory
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static FileHashList FromDirectory(string path)
		{
			return FromDirectory(new DirectoryInfo(path));
		}

		/// <summary>
		/// Computes the hash of each file in a directory
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static FileHashList FromDirectory(DirectoryInfo path)
		{
			if (!path.Exists)
			{
				throw new FileNotFoundException(String.Format("Directory does not exist: {0}", path.FullName));
			}

			var files = path.GetFiles();
			var count = files.Length;

			// Don't try to parallelize this. The bottleneck is the disk read speed, so trying
			// to read multiple files from the same disk just prevents us from doing sequential
			// reads and slows everything down.
			var entries = files.Select((file, index) =>
			{
				// This might slow things down a bit, but it's nice to see progress updates in
				// the console since this runs for so long. Ideally, this would be a Task using
				// an IProgress to post progress updates to the UI.
				Console.WriteLine("Hashing {0} ({1} of {2})", file.Name, index + 1, count);
				return FileHashEntry.FromFile(file);
			});

			return new FileHashList(path, entries);
		}

		/// <summary>
		/// Loads the hashes in a cache file
		/// </summary>
		/// <param name="rootDirectory"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static FileHashList FromListFile(string rootDirectory, string path)
		{
			return FromListFile(new DirectoryInfo(rootDirectory), new FileInfo(path));
		}

		/// <summary>
		/// Loads the hashes in a cache file
		/// </summary>
		/// <param name="rootDirectory"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static FileHashList FromListFile(DirectoryInfo rootDirectory, FileInfo path)
		{
			if (!path.Exists)
			{
				throw new FileNotFoundException(String.Format("File does not exist: {0}", path.FullName));
			}

			var entries = new List<FileHashEntry>();
			using (var stream = new StreamReader(path.OpenRead()))
			{
				string line;
				while ((line = stream.ReadLine()) != null)
				{
					if (!String.IsNullOrWhiteSpace(line))
					{
						entries.Add(new FileHashEntry(line));
					}
				}
			}

			return new FileHashList(rootDirectory, entries);
		}

		/// <summary>
		/// Gets a list of all the files which no longer match the cached hashes in the list
		/// </summary>
		/// <returns></returns>
		public IEnumerable<FileHashEntry> GetChangedFiles()
		{
			var root = RootDirectory.FullName;
			return this.Files.Where(file => file.HasFileChanged(root));
		}

		public void UpdateHashes()
		{
			var root = RootDirectory.FullName;
			foreach (var file in this.Files)
			{
				file.UpdateHash(root);
			};
		}

		public void WriteListFile(string path)
		{
			using (var stream = new StreamWriter(path, false, Encoding.UTF8))
			{
				foreach (var file in this.Files)
				{
					stream.WriteLine(file.ToString());
				}
			}
		}
	}
}