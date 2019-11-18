using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace Client
{
	/// <summary>
	/// Represents a .vars file parsed as an object.
	/// </summary>
	class Vars : IReadOnlyDictionary<string, object>
	{
		public object this[string key] => throw new NotImplementedException();

		public IEnumerable<string> Keys => throw new NotImplementedException();

		public IEnumerable<object> Values => throw new NotImplementedException();

		public int Count => throw new NotImplementedException();

		/// <summary>
		/// Loads a vars object from a file.
		/// </summary>
		public static Vars Load(string fileName)
		{
			return null;
		}

		public bool ContainsKey(string key)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public bool TryGetValue(string key, out object value)
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		/* ------------ */
		/* --- Data --- */
		/* ------------ */

		// Vars container
		private Dictionary<string, object> _vars = new Dictionary<string, object>();
	}
}
