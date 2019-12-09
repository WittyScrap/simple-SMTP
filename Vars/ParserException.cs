using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableManagement
{
	/// <summary>
	/// Parsing related exception.
	/// </summary>
	class ParserException : Exception
	{
		/// <summary>
		/// Creates a new parsing exception.
		/// </summary>
		/// <param name="details">Parsing error details.</param>
		public ParserException(string details) : base(@"\cf1\b Parsing error:\b0\cf2\i  " + details)
		{ }
	}
}
