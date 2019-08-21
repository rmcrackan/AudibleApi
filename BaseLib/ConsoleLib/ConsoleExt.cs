using System;
using System.Collections.Generic;
using System.Text;

namespace BaseLib.ConsoleLib
{
	// https://stackoverflow.com/a/3404522

	/// <summary>Read password typed into console. Password is masked with *</summary>
	public static class ConsoleExt
	{
		public static string ReadPassword()
		{
			var pass = "";

			while (true)
			{
				var key = Console.ReadKey(true);

				if (key.Key == ConsoleKey.Enter)
					return pass;

				if (key.Key == ConsoleKey.Backspace)
				{
					if (pass.Length > 0)
					{
						pass = pass.Substring(0, pass.Length - 1);
						// \b   moves the cursor back but does not delete the *
						// ' '  overwrites the * with a space, progressing the cursor as normal
						// \b   moves the cursor back again
						Console.Write("\b \b");
					}
				}
				else
				{
					pass += key.KeyChar;
					Console.Write("*");
				}
			}
		}
	}
}
