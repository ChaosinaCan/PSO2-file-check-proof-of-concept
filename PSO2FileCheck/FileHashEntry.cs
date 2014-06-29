using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PSO2FileCheck
{
	public class FileHashEntry
	{
		private static readonly char[] SplitCharacters = new char[] { '\t' };

		// Cache values to improve performance if asked to check hashes multiple times
		// Probably not necessary, but this saves us from having to recompute the file
		// hash if we call HasFileChanged() followed by UpdateHash()
		private DateTime? _updatedTime = null;

		private byte[] _updatedHash = null;

		public string FileName { get; set; }

		public DateTime LastModified { get; set; }

		public byte[] Hash { get; set; }

		public FileHashEntry(string fileName, DateTime lastModified, byte[] hash)
		{
			this.FileName = fileName;
			this.LastModified = lastModified;
			this.Hash = hash;
		}

		/// <summary>
		/// Create from a line of a cache file
		/// </summary>
		/// <param name="line"></param>
		public FileHashEntry(string line)
		{
			var parts = line.Split(SplitCharacters, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 3)
			{
				throw new ArgumentException(String.Format("Couldn't parse line: {0}", line));
			}

			this.FileName = parts[0];
			this.LastModified = DateTime.FromFileTime(long.Parse(parts[1]));
			this.Hash = Util.FromHexString(parts[2]);
		}

		/// <summary>
		/// Create from the current info of an existing file
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static FileHashEntry FromFile(string path)
		{
			return FromFile(new FileInfo(path));
		}

		/// <summary>
		/// Create from the current info of an existing file
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static FileHashEntry FromFile(FileInfo file)
		{
			return new FileHashEntry(file.Name, file.LastWriteTime, _computeHash(file));
		}

		private static byte[] _computeHash(FileInfo file)
		{
			using (var hasher = new MD5CryptoServiceProvider())
			{
				using (var stream = file.OpenRead())
				{
					return hasher.ComputeHash(stream);
				}
			}
		}

		/// <summary>
		/// Returns a string such that new FileHashEntry(entry.ToString()) == entry
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Join("\t", FileName, LastModified.ToFileTime(), Util.ToHexString(Hash));
		}

		/// <summary>
		/// Returns true if the file's current hash does not match this entry's cached hash
		/// </summary>
		/// <param name="root"></param>
		/// <returns></returns>
		public bool HasFileChanged(string root)
		{
			var file = new FileInfo(Path.Combine(root, FileName));
			if (!file.Exists)
			{
				// If the file doesn't exist any more, consider it changed.
				return true;
			}
			else if (file.LastWriteTime == LastModified)
			{
				// File hasn't been modified since the last time we saved a hash.
				// Assume it hasn't changed.
				return false;
			}
			else if (file.LastWriteTime == _updatedTime)
			{
				// File hasn't been modified since the last time we computed a hash.
				// Assume the hash is still valid.
				return !Util.ArraysEqual(Hash, _updatedHash);
			}
			else
			{
				// File has been modified since the last time we hashed it.
				// Recompute its hash and compare.
				var newHash = _computeHash(file);

				_updatedHash = newHash;
				_updatedTime = file.LastWriteTime;

				return !Util.ArraysEqual(Hash, newHash);
			}
		}

		/// <summary>
		/// Updates this entry's cached hash to match the current state of the file
		/// </summary>
		/// <param name="root"></param>
		public void UpdateHash(string root)
		{
			var file = new FileInfo(Path.Combine(root, FileName));
			if (file.LastWriteTime == LastModified)
			{
				// File hasn't been modified since the last time we saved a hash.
				// Assume it hasn't changed.
			}
			else if (file.LastWriteTime == _updatedTime)
			{
				// File hasn't been modified since the last time we computed a hash.
				// Assume the hash is still valid.
				Hash = _updatedHash;
				LastModified = file.LastWriteTime;
			}
			else
			{
				Hash = _computeHash(file);
				LastModified = file.LastWriteTime;
			}

			_updatedHash = null;
			_updatedTime = null;
		}
	}
}