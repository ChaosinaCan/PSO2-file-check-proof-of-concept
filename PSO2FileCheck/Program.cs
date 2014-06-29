using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSO2FileCheck
{
	internal class Program
	{
		private const string PSO2Directory = @"C:\Program Files (x86)\SEGA\PHANTASYSTARONLINE2\pso2_bin\data\win32";
		private const string CacheFile = "hashlist.txt";

		private static void Main(string[] args)
		{
			// See how long it takes to hash each file
			BenchmarkHashCompute();

			// See how long it takes to
			BenchmarkCachedHashCheck();

			Console.WriteLine("Done. Press any key.");
			Console.ReadLine();
		}

		private static void BenchmarkHashCompute()
		{
			Console.WriteLine("Computing hashes of all files...");

			var start = DateTime.Now;
			var list = FileHashList.FromDirectory(PSO2Directory);

			Console.WriteLine("Completed in {0} seconds", (DateTime.Now - start).TotalSeconds);

			list.WriteListFile(CacheFile);
		}

		private static void BenchmarkCachedHashCheck()
		{
			Console.WriteLine("Computing hashes using cache...");

			var start = DateTime.Now;
			var list = FileHashList.FromListFile(PSO2Directory, CacheFile);
			list.UpdateHashes();

			Console.WriteLine("Completed in {0} seconds", (DateTime.Now - start).TotalSeconds);
		}
	}
}