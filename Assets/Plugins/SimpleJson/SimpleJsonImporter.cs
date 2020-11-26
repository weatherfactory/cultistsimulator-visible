/* SimpleJson 1.1                       */
/* April 1, 2015                        */
/* By Orbcreation BV                    */
/* Richard Knol                         */
/* info@orbcreation.com                 */
/* games, components and freelance work */

using System;
using System.Collections;
using System.Collections.Generic;
using OrbCreationExtensions;

public class SimpleJsonImporter
{
	private enum ReadingState
	{
		Key,
		KeyFinished,
		Value,
		ValueFinished
	}

	private static readonly Dictionary<char, char> MatchingChars = new Dictionary<char, char>()
	{
		{'{', '}'},
		{'[', ']'},
		{'}', '{'},
		{']', '['}
	};

	private readonly string _json;

	private readonly bool _caseInsensitive;

	private readonly List<string> _warnings;

	// importing case insensitive will turn all tags into lowercase
	public static Hashtable Import(string json, bool caseInsensitive = false)
	{
		SimpleJsonImporter importer = new SimpleJsonImporter(json, caseInsensitive);
		Hashtable importedData = importer.Read();
		if (importer._warnings.Count > 0)
			throw new SimpleJsonException(importer._warnings);
		return importedData;
	}

	private SimpleJsonImporter(string json, bool caseInsensitive)
	{
		_json = json;
		_caseInsensitive = caseInsensitive;
		_warnings = new List<string>();
	}

	private Hashtable Read()
	{
		int end = _json.Length;
		int begin = 0;
		MoveToNextNode(ref begin, end);
		if (begin >= end)
			return null;
		int nodeEnd = FindMatchingEnd(begin, end);
		switch (_json[begin])
		{
			case '{':
				return ReadHashtable(ref begin, nodeEnd);
			case '[':
			{
				ArrayList list = ReadArrayList(ref begin, nodeEnd);
				if (list != null && list.Count > 0) {
					Hashtable wrapper = new Hashtable {{"SimpleJSON", list}};
					return wrapper;
				}

				break;
			}
			default:
				LogWarning("Unexpected character: found '" + _json[begin] + "', was expecting '{' or '['", begin);
				break;
		}
		return null;
	}

	private Hashtable ReadHashtable(ref int begin, int end)
	{
		Hashtable returnValue = new Hashtable();
		int idx;
		ReadingState readingState = ReadingState.Key;
		string key = "";

		for (idx = begin + 1; idx < end; idx++)
		{
			char c = _json[idx];

			switch (readingState)
			{
				case ReadingState.Key:
					// Skip any whitespace at the beginning
					if (key.Length == 0 && char.IsWhiteSpace(c))
						continue;

					key = ReadString(ref idx, end, ':');
					if (_caseInsensitive)
						key = key.ToLower();
					readingState = ReadingState.KeyFinished;
					break;
				case ReadingState.KeyFinished:
					// Skip any remaining whitespace
					if (char.IsWhiteSpace(c))
						continue;

					if (c == ':')
					{
						if (key.Length == 0)
							LogWarning("Empty hashtable key", idx);
						readingState = ReadingState.Value;
					}
					else
						LogWarning("Unexpected character in hashtable key: found '" + c + "', was expecting ':'", idx);

					break;
				case ReadingState.Value:
					// Skip any whitespace at the beginning
					if (char.IsWhiteSpace(c))
						continue;

					switch (c)
					{
						// Try to read a hashtable, an array list, or a string
						case '{':
							returnValue[key] = ReadHashtable(ref idx, FindMatchingEnd(idx, end));
							break;
						case '[':
							returnValue[key] = ReadArrayList(ref idx, FindMatchingEnd(idx, end));
							break;
						default:
							returnValue[key] = ReadString(ref idx, end, ',');
							break;
					}
					readingState = ReadingState.ValueFinished;
					break;
				case ReadingState.ValueFinished:
					if (char.IsWhiteSpace(c))
						continue;

					if (c != ',')
					{
						LogWarning("Unexpected character in hashtable: found '" + c + "', was expecting ','", idx);
						idx--;  // Go back so that processing can still continue in the new state
					}

					// Start reading the next key if the comma delimiter was encountered
					key = "";
					readingState = ReadingState.Key;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		if (readingState == ReadingState.Value || readingState == ReadingState.KeyFinished)
			LogWarning("Missing value for hashtable key: '" + key + "'", end - 1);

		begin = end;
		return returnValue;
	}

	private ArrayList ReadArrayList(ref int begin, int end)
	{
		ArrayList returnValue = new ArrayList();
		ReadingState readingState = ReadingState.Value;
		int idx;
		for (idx = begin + 1; idx < end; idx++)
		{
			char c = _json[idx];
			if (readingState == ReadingState.Value)
			{
				if (char.IsWhiteSpace(c))
					continue;
				switch (c)
				{
					// Try to read a hashtable, an array list, or a string
					case '{':
						returnValue.Add(ReadHashtable(ref idx, FindMatchingEnd(idx, end)));
						break;
					case '[':
						returnValue.Add(ReadArrayList(ref idx, FindMatchingEnd(idx, end)));
						break;
					default:
						returnValue.Add(ReadString(ref idx, end, ','));
						break;
				}

				readingState = ReadingState.ValueFinished;
			}
			else
			{
				if (char.IsWhiteSpace(c))
					continue;
				if (c != ',')
				{
					LogWarning("Unexpected character in array list: found '" + c + "', was expecting ','", idx);
					idx--;  // Go back so that processing can still continue in the next state
				}

				readingState = ReadingState.Value;
			}
		}

		begin = end;
		return returnValue;
	}

	private string ReadString(ref int begin, int end, char terminator)
	{
		string returnValue = "";
		bool isEscaped = false;
		bool foundEnd = false;
		int idx;

		// Check if we are in a quoted string, which determines whether or not
		// whitespace is allowed
		bool withinQuotes = _json[begin] == '"';
		if (withinQuotes)
			begin++;

		for (idx = begin; idx < end; idx++)
		{
			char c = _json[idx];

			// Check if this is a valid escape character within quotes
			// If the previous character was also an escape character, it
			// escapes this one, which turns into a regular character;
			// otherwise, the next character should be escaped
			if (withinQuotes && c == '\\')
			{
				isEscaped = !isEscaped;
				if (isEscaped)
				{
					continue;
				}
			}

			if (withinQuotes)
			{
				if (!isEscaped && c == '"')
				{
					foundEnd = true;
					break;
				}
				if (isEscaped)
				{
					// Consider valid escape characters
					switch (c)
					{
						case 'n':
							c = '\n';
							break;
						case 't':
							c = '\t';
							break;
						case 'r':
							c = '\r';
							break;
						case '"':
							break;
						default:
							LogWarning("Unknown escaped sequence: '\\" + c + "'");
							returnValue += '\\';
							break;
					}
				}
			}
			else
			{
				if (char.IsWhiteSpace(c) || c == terminator)
				{
					foundEnd = true;
					idx--;  // Go back so that the next state can process this character correctly
					break;
				}
				if (c == '"')
					LogWarning("Found '\"' in unquoted string", idx);
			}

			isEscaped = false;
			returnValue += c;
		}

		if (isEscaped)
			LogWarning("Unterminated escape sequence", idx);
		if (withinQuotes && !foundEnd)
			LogWarning("Missing closing '\"' for string", end);
		else if (!withinQuotes && returnValue.Length == 0)
			LogWarning("Empty unquoted string", idx);
		begin = idx;

		return returnValue.JsonDecode();
	}

	private void MoveToNextNode(ref int begin, int end)
	{
		int idx;
		for (idx = begin; idx < end; idx++)
		{
			char c = _json[idx];
			if (c == '{' || c == '[')
			{
				begin = idx;
				return;
			}
			if (char.IsWhiteSpace(c))
			{
				continue;
			}
			LogWarning("Unexpected character: found '" + c + "', was expecting '{' or '['", idx);
			break;
		}
		begin = end;   // not found
	}

	private int FindMatchingEnd(int begin, int end)
	{
		bool withinQuotes = false;
		Stack<char> levels = new Stack<char>();
		for (int idx = begin; idx < end; idx++) {
			char c = _json[idx];
			if (idx != 0 && _json[idx - 1] == '\\')
				continue;
			switch (c)
			{
				case '"':
					withinQuotes = !withinQuotes;
					break;
				case '{':
				case '[':
					if (withinQuotes)
						continue;
					levels.Push(c);
					break;
				case '}':
				case ']':
				{
					if (withinQuotes)
						continue;
					char openingChar;
					try
					{
						openingChar = levels.Peek();
					}
					catch (InvalidOperationException)
					{
						LogWarning("Unexpected closing character: found '" + c + "', but no '" + MatchingChars[c] + "'", idx);
						openingChar = MatchingChars[c];  // Keep going as if the correct opening character was provided
					}
					char expectedChar = MatchingChars[openingChar];
					if (c != expectedChar)
					{
						LogWarning("Invalid closing character: found '" + c + "', was expecting '" + expectedChar + "'", idx);
					}

					levels.Pop();
					if (levels.Count == 0)
						return idx;   // if we are at the correct nesting level, we found the end
					break;
				}
			}
		}
		// If we arrived at a non-zero nesting level, that means no matching end
		// was found, and the JSON should be considered invalid
		if (levels.Count != 0)
		{
			LogWarning("Missing closing character for '" + levels.Peek() + "'", end);
		}
		return end;
	}

	private void LogWarning(string message, int idx = -1)
	{
		string warning = message;
		if (idx >= 0)
		{
			int[] position = GetLineAndColumnAtIndex(idx);
			warning += " (at line " + position[0] + ", column " + position[1] + ")";
		}
		_warnings.Add(warning);
	}

	private int[] GetLineAndColumnAtIndex(int idx)
	{
		int line = 1;
		int column = 1;
		for (int i = 0; i < idx; i++)
		{
			switch (_json[i])
			{
				case '\n':
					line += 1;
					column = 1;
					break;
				case '\t':
					column += 4;  // Assume a tab width of 4
					break;
				default:
					column += 1;
					break;
			}
		}

		return new[] {line, column};
	}
}
