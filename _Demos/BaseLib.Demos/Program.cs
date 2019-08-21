using System;
using System.Threading;
using System.Threading.Tasks;
using BaseLib.ConsoleLib;

namespace BaseLib.Demos
{
	public class _Main
	{
		public static async Task Main(string[] args)
		{
			var demo = new Demonstrations();

			//demo.ProgressBarUp();
			//demo.ProgressBarDown();

			demo.ReadPassword();
		}
	}
	public class Demonstrations
	{
		public void ProgressBarUp()
		{
			Console.Write("Performing some task... ");
			using (var progressBar = new ProgressBar())
			{
				for (int i = 0; i <= 100; i++)
				{
					progressBar.Report((double)i / 100);
					Thread.Sleep(20);
				}
			}

			Console.WriteLine("Done.");
		}

		public void ProgressBarDown()
		{
			Console.Write("Performing some task... ");
			using (var progressBar = new ProgressBar())
			{
				for (int i = 100; i > 0; i--)
				{
					progressBar.Report((double)i / 100);
					Thread.Sleep(20);
				}
			}

			Console.WriteLine("Done.");
		}

		public void ReadPassword()
		{
			Console.Write("Type pw:");
			var pw = ConsoleExt.ReadPassword();
			Console.WriteLine();
			Console.WriteLine("pw=" + pw);
		}
	}
}
