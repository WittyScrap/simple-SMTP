using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

namespace VariableManagement
{
	/// <summary>
	/// Represents a .vars file parsed as an object.
	/// </summary>
	public class Variables : IReadOnlyDictionary<string, object>
	{
		/// <summary>
		/// Initialises the root object.
		/// </summary>
		public Variables()
		{
			Root = new VariablesObject();
		}

		/// <summary>
		/// Accesses a variable through a key.
		/// </summary>
		public object this[string key] {
			get
			{
				Queue<string> components = new Queue<string>(key.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries));
				VariablesObject previous = Root;

				while (components.Count > 1)
				{
					string nextkey = components.Dequeue();
					previous = previous.GetObject(nextkey);

					if (previous == null)
					{
						throw new Exception("Key " + nextkey + " does not exist in object " + this + "!");
					}
				}

				return previous.GetField(components.Dequeue());
			}
			set
			{
				Queue<string> components = new Queue<string>(key.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries));
				VariablesObject previous = Root;

				while (components.Count > 1)
				{
					string nextkey = components.Dequeue();
					previous = previous.GetObject(nextkey);

					if (previous == null)
					{
						throw new Exception("Key " + nextkey + " does not exist in object " + this + "!");
					}
				}

				previous.SetFieldValue(components.Dequeue(), value);
			}
		}

		/// <summary>
		/// Loads a vars object from a file.
		/// </summary>
		public static Variables Load(string fileName)
		{
			if (!File.Exists(fileName))
			{
				throw new Exception("Unable to open file: " + fileName);
			}

			string fileData = File.ReadAllText(fileName);
			string[] segments = fileData.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
			string[] tokens = Tokenify(segments).ToArray();

			Variables varsObject = new Variables();
			varsObject.Parse(tokens);

			return varsObject;
		}

		/// <summary>
		/// Checks that a key exists either as a field or object.
		/// </summary>
		/// <param name="key">The key to check for.</param>
		/// <returns>True if the key exists, false otherwise</returns>
		public bool ContainsKey(string key)
		{
			return Root.GetObject(key) != null || Root.GetField(key) != null;
		}

		/// <summary>
		/// No way to iterate through the object for now.
		/// </summary>
		/// <returns>null</returns>
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return null;
		}

		/// <summary>
		/// Attempts to extract an object first, then a key.
		/// If neither exists, false will be returned.
		/// </summary>
		/// <param name="key">The object/field to search for.</param>
		/// <param name="value">The value that will be retrieved.</param>
		/// <returns>True on succesful retrieval, false on faliure.</returns>
		public bool TryGetValue(string key, out object value)
		{
			value = Root.GetObject(key);
			if (value == null)
			{
				value = Root.GetField(key);
			}
			return value != null;
		}

		/// <summary>
		/// No way to iterate through the object for now.
		/// </summary>
		/// <returns>null</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return null;
		}

		/// <summary>
		/// Populates this class through deserialisation of parsed tokens.
		/// </summary>
		/// <param name="tokens">The tokens to populate the class from.</param>
		private void Parse(string[] tokens)
		{
			VariablesParser parser = new VariablesParser();
			parser.Parse(this, tokens);
		}

		/// <summary>
		/// Parses a tokenised array into smaller atomic components.
		/// </summary>
		private static IEnumerable<string> Tokenify(string[] tokens)
		{
			foreach (string token in tokens)
			{
				foreach (string segment in SplitAround(token, ":;{}".ToCharArray()))
				{
					yield return segment;
				}
			}
		}

		/// <summary>
		/// Splits the string around the given tokens, for instance, with tokens:<br/>
		/// :,{}()<br/>
		/// And a string:<br/>
		/// alpha:bravo(c,d){example}<br/>
		/// An array containing values:<br/>
		/// [alpha][:][bravo][(][c][,][d][)][{][example][}]<br/>
		/// Should be generated.
		/// </summary>
		/// <param name="value">The source string.</param>
		/// <param name="tokens">The tokens to split the source string around.</param>
		private static IEnumerable<string> SplitAround(string value, char[] tokens)
		{
			if (tokens.Length == 0)
			{
				yield return value;
			}
			else
			{
				char delimiter = tokens[0];

				if (value.Length == 1 && value[0] == delimiter)
				{
					yield return value;
				}
				else
				{
					string[] segments = value.Split(delimiter);
					tokens = tokens.Skip(1).ToArray();

					for (int i = 0; i < segments.Length; ++i)
					{
						if (segments[i].Length > 0)
						{
							foreach (string subsegment in SplitAround(segments[i], tokens))
							{
								yield return subsegment;
							}
						}

						if (i < segments.Length - 1)
						{
							yield return delimiter.ToString();
						}
					}
				}
			}
		}

		/* ------------ */
		/* --- Data --- */
		/* ------------ */

		// Root of the variables tree
		public VariablesObject Root { get; }

		/// <summary>
		/// No way to iteratively retrieve keys for now.
		/// </summary>
		public IEnumerable<string> Keys => null;

		/// <summary>
		/// No way to iteratively retrieve values for now.
		/// </summary>
		public IEnumerable<object> Values => null;

		/// <summary>
		/// No way to obtain the total amount of fields/objects for now.
		/// </summary>
		public int Count => -1;
	}
}
