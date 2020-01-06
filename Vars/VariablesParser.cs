using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VariableManagement
{
	/// <summary>
	/// Class enabled to parse a vars object from a set
	/// of tokens.
	/// </summary>
	public class VariablesParser
	{
		/// <summary>
		/// The current state of the parser.
		/// </summary>
		private enum State
		{
			SeekingName,
			SeekingType,
			SeekingValue,
			SeekingEnd
		}

		/// <summary>
		/// The active state of the object.
		/// </summary>
		private State _state = State.SeekingName;

		/// <summary>
		/// Runs the parsing algorithm and applies it to the given target.
		/// </summary>
		/// <param name="target">The target object.</param>
		/// <param name="parsedTokens">The parsed token list.</param>
		public void Parse(Variables target, string[] parsedTokens)
		{
			_state = State.SeekingName;

			Stack<VariablesObject> scope = new Stack<VariablesObject>();
			scope.Push(target.Root);

			string previousName = "";

			for (int i = 0; i < parsedTokens.Length; ++i)
			{
				string token = parsedTokens[i];

				switch (_state)
				{
					// State: looking for a name tag, or for the end of an object.
					case State.SeekingName:
					{
						CheckForInvalidTokens(token, i, ";:{");

						if (token == "}")
						{
							scope.Pop();
						}
						else
						{
							previousName = token;
							_state++;
						}
						break;
					}

					// State: seeking an object type, either object ({) or field (:).
					case State.SeekingType:
					{
						CheckForInvalidTokens(token, i, ";}");

						if (token != "{" && token != ":")
						{
							throw ParsingException(token, i);
						}

						if (token == ":")
						{
							_state++;
						}
						else
						{
							scope.Push(scope.Peek().AddObject(previousName));
							_state = State.SeekingName;
						}
						break;
					}

					// State: seeking a value, values can only either be booleans, floats, or integers (for the time being).
					case State.SeekingValue:
					{
						CheckForInvalidTokens(token, i, ";:{}");

						Parse(token, out object parsedToken);
						scope.Peek().AddField(previousName, parsedToken);
						_state++;

						break;
					}

					// State: seeking an ending mark for a field (;).
					case State.SeekingEnd:
					{
						if (token != ";")
						{
							throw ParsingException(token, i);
						}

						_state = State.SeekingName;

						break;
					}
				}

			}
		}

		/// <summary>
		/// Checks for invalid token chars and tests
		/// them against the provided token, raises an
		/// exception if a match is found.
		/// </summary>
		/// <param name="token">The token to check against.</param>
		/// <param name="invalidChars">The invalid char array (string) containing the tokens that are not allowed in this position.</param>
		private void CheckForInvalidTokens(string token, int position, string invalidChars)
		{
			if (IsAny(token, invalidChars))
			{
				throw ParsingException(token, position);
			}
		}

		/// <summary>
		/// Generates a generic parsing exception at the given position
		/// with the given token.
		/// </summary>
		/// <param name="token">The token that caused the error.</param>
		/// <param name="position">Where the error occurred.</param>
		/// <returns>The generated exception object.</returns>
		private Exception ParsingException(string token, int position)
		{
			return new Exception(@"\cf1\b Parsing error:\b0\cf2\i  unexpected token: \" + token + " in position: " + position + @"\i0\cf3");
		}

		/// <summary>
		/// Returns true if the token is any of the values.
		/// </summary>
		/// <param name="token">The token to check.</param>
		/// <param name="values">The values to test against.</param>
		/// <returns>True if the token is any of the provided values, false if it matches none.</returns>
		private bool IsAny(string token, string tokens)
		{
			if (token.Length > 0 || token.Length < 1)
			{
				return false;
			}

			char tokenChar = token[0];

			foreach (char value in tokens)
			{
				if (tokenChar == value)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Attempts to parse a string token to any of the supported
		/// object types.
		/// </summary>
		/// <param name="token">The raw string token.</param>
		/// <param name="parsed">The parsed object type.</param>
		/// <returns>True of the token can be parsed into any of the known object types, false otherwise.</returns>
		public static void Parse(string token, out object parsed)
		{
			if (token == "true" || token == "false")
			{
				parsed = token == "true" ? true : false;
			}
			else if (int.TryParse(token, out int intValue))
			{
				parsed = intValue;
			}
			else if (long.TryParse(token, out long longValue))
			{
				parsed = longValue;
			}
			else if (BigInteger.TryParse(token, out BigInteger bigValue))
			{
				parsed = bigValue;
			}
            else if (float.TryParse(token, out float floatValue))
			{
				parsed = floatValue;
			}
			else // Assume string
			{
				parsed = token;
			}
		}
	}
}
