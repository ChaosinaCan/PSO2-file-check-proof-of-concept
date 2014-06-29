using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSO2FileCheck
{
	public static class Util
	{
		public static bool ArraysEqual<T>(T[] a1, T[] a2)
		{
			if (ReferenceEquals(a1, a2))
			{
				return true;
			}

			if (a1 == null || a2 == null)
			{
				return false;
			}

			if (a1.Length != a2.Length)
			{
				return false;
			}

			var comparer = EqualityComparer<T>.Default;
			for (int i = 0; i < a1.Length; i++)
			{
				if (!comparer.Equals(a1[i], a2[i]))
				{
					return false;
				}
			}

			return true;
		}

		// http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa/14333437#14333437
		public static string ToHexString(byte[] bytes)
		{
			char[] c = new char[bytes.Length * 2];
			int b;
			for (int i = 0; i < bytes.Length; i++)
			{
				b = bytes[i] >> 4;
				c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
				b = bytes[i] & 0xF;
				c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
			}
			return new string(c);
		}

		// http://hexstring.codeplex.com

		// values for '\0' to 'f' where 255 indicates invalid input character
		// starting from '\0' and not from '0' costs 48 bytes but results 0 subtructions and less if conditions
		static readonly byte[] fromHexTable = new byte[] {
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 0, 1,
			2, 3, 4, 5, 6, 7, 8, 9, 255, 255,
			255, 255, 255, 255, 255, 10, 11, 12, 13, 14, 
			15, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 10, 11, 12,
			13, 14, 15
		};

		// same as above but valid values are multiplied by 16
		static readonly byte[] fromHexTable16 = new byte[] {
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 0, 16,
			32, 48, 64, 80, 96, 112, 128, 144, 255, 255,
			255, 255, 255, 255, 255, 160, 176, 192, 208, 224, 
			240, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 160, 176, 192,
			208, 224, 240
		};

		public unsafe static byte[] FromHexString(string source)
		{
			// return an empty array in case of null or empty source
			if (string.IsNullOrEmpty(source))
				return new byte[0]; // you may change it to return null
			if (source.Length % 2 == 1) // source length must be even
				throw new ArgumentException();
			int
				index = 0, // start position for parsing source
				len = source.Length >> 1; // initial length of result
			fixed (char* sourceRef = source) // take the first character address of source
			{
				if (*(int*)sourceRef == 7864368) // source starts with "0x"
				{
					if (source.Length == 2) // source must not be just a "0x")
						throw new ArgumentException();
					index += 2; // start position (bypass "0x")
					len -= 1; // result length (exclude "0x")
				}
				byte add = 0; // keeps a fromHexTable value
				byte[] result = new byte[len]; // initialization of result for known length
				fixed (byte* hiRef = fromHexTable16) // freeze fromHexTable16 position in memory
				fixed (byte* lowRef = fromHexTable) // freeze fromHexTable position in memory
				fixed (byte* resultRef = result) // take the first byte address of result
				{
					char* s = (char*)&sourceRef[index]; // take first parsing position of source - allow inremental memory position
					byte* r = resultRef; // take first byte position of result - allow incremental memory position
					while (*s != 0) // source has more characters to parse
					{
						// check for non valid characters in pairs
						// you may split it if you don't like its readbility
						if (
							*s > 102 ||
							(*r = hiRef[*s++]) == 255 || // assign source value to current result position and increment source position
							*s > 102 ||
							(add = lowRef[*s++]) == 255 // assign source value to "add" parameter and increment source position
							)
							throw new ArgumentException();
						*r++ += add; // set final value of current result byte and move pointer to next byte
					}
					return result;
				}
			}
		}
	}
}
