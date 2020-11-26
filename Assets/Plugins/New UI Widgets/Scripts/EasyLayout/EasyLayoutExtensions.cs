namespace EasyLayoutNS
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Extensions.
	/// </summary>
	public static class EasyLayoutExtensions
	{
		/// <summary>
		/// Convert the specified input with converter.
		/// </summary>
		/// <param name="input">Input.</param>
		/// <param name="converter">Converter.</param>
		/// <typeparam name="TInput">The 1st type parameter.</typeparam>
		/// <typeparam name="TOutput">The 2nd type parameter.</typeparam>
		/// <returns>List with converted items.</returns>
		public static List<TOutput> Convert<TInput, TOutput>(this List<TInput> input, Converter<TInput, TOutput> converter)
		{
			#if NETFX_CORE
			var output = new List<TOutput>(input.Count);
			for (int i = 0; i < input.Count; i++)
			{
				output.Add(converter(input[i]));
			}
			
			return output;
			#else
			return input.ConvertAll<TOutput>(converter);
			#endif
		}

		#if NETFX_CORE
		static public void ForEach<T>(this List<T> list, Action<T> action)
		{
			for (int i = 0; i < list.Count; i++)
			{
				action(list[i]);
			}
		}
		#endif
	}
}