using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics; 

namespace BMSLib
{

	public class JudgeGrade
	{

		public JudgeGrade()
		{
		}

		private static long totalKnockDistance;
		private static int totalJudgedNotes;
		private static bool consoleInitialized;

		public static int ToAccEx { get; private set; }

		public static int ToAccExact { get; private set; }

		public static int ToAccGreat { get; private set; }

		public static int ToAccRight { get; private set; }

		public static int TotalEx { get; private set; }

		public static int TotalExact { get; private set; }

		public static int TotalGreat { get; private set; }

		public static int TotalRight { get; private set; }

		public static int TotalMiss { get; private set; }

		public static int TotalCombo { get; private set; }

		public static long AutoDelay
		{
			get
			{
				return 75000L;
			}
		}

		public static void Reset()
		{
			InitializeAndConfigureConsole();
			baseAccuracy = 0f;
			exAccuracy = 0f;
			exactAccuracy = 0f;
			greatAccuracy = 0f;
			rightAccuracy = 0f;
			TotalEx = 0;
			TotalExact = 0;
			TotalGreat = 0;
			TotalRight = 0;
			TotalMiss = 0;
			TotalCombo = 0;
			uploadScore = new float[6];
			totalKnockDistance = 0L;
			totalJudgedNotes = 0;
			UpdateConsoleDisplay();
		}

		public static float GetJudgeGradeScore(long knockDistance)
		{
			float num = 0f;
			bool flag = knockDistance < 0L;
			knockDistance = Math.Abs(knockDistance);
			if (knockDistance < 450000L)
			{
				num = 1.05f + 0.1f * (1f - (float)knockDistance / 450000f);
			}
			else if (knockDistance < 900000L)
			{
				num = 1f;
			}
			else if (knockDistance < 1500000L)
			{
				num = 0.7f + 0.2f * (1f - (float)(knockDistance - 900000L) / 600000f);
			}
			else if (knockDistance < 2500000L)
			{
				if (!flag)
				{
					num += 0.1f + 0.5f * (1f - (float)(knockDistance - 900000L) / 1600000f);
				}
			}
			else if (!flag)
			{
				num = 0f;
			}
			return num;
		}

		public static int GetJudgeGrade(long knockDistance)
		{
			if (Math.Abs(knockDistance) < 2500000L)
			{
				totalKnockDistance += knockDistance;
				totalJudgedNotes++;
			}
			int result = -1;
			bool flag = knockDistance < 0L;
			long num = Math.Abs(knockDistance);
			if (num < 450000L)
			{
				result = 0;
				baseAccuracy += 1f;
				exAccuracy += 0.05f + 0.1f * (1f - (float)num / 450000f);
				exactAccuracy += 1f;
				TotalEx++;
				TotalExact++;
			}
			else if (num < 900000L)
			{
				result = 1;
				baseAccuracy += 1f;
				exactAccuracy += 1f;
				TotalExact++;
			}
			else if (num < 1500000L)
			{
				result = 2;
				baseAccuracy += 0.7f + 0.2f * (1f - (float)(num - 900000L) / 600000f);
				greatAccuracy += 0.7f + 0.2f * (1f - (float)(num - 900000L) / 600000f);
				TotalGreat++;
			}
			else if (num < 2500000L)
			{
				if (!flag)
				{
					result = 3;
					baseAccuracy += 0.1f + 0.5f * (1f - (float)(num - 900000L) / 1600000f);
					rightAccuracy += 0.1f + 0.5f * (1f - (float)(num - 900000L) / 1600000f);
					TotalRight++;
				}
			}
			else if (!flag)
			{
				result = 4;
				TotalMiss++;
			}
			UpdateConsoleDisplay();
			return result;
		}

		public static int GetRankGrade(int synNumber)
		{
			int result = 4;
			if (synNumber >= 11700)
			{
				result = 0;
			}
			else if (synNumber >= 11000)
			{
				result = 1;
			}
			else if (synNumber >= 9500)
			{
				result = 2;
			}
			else if (synNumber >= 7500)
			{
				result = 3;
			}
			return result;
		}

		private static int GetRatacc()
		{
			int num = TotalExact + TotalGreat + TotalRight + TotalMiss;
			if (num < 1)
			{
				return 0;
			}
			int num2 = (int)(baseAccuracy / (float)num * 10000f);
			uploadScore[1] = baseAccuracy;
			uploadScore[3] = (float)num;
			num2 = ((num2 <= 10000) ? num2 : 10000);
			baseAccuracy = 0f;
			return num2;
		}

		private static int GetRatadd()
		{
			int num = TotalExact + TotalGreat + TotalRight + TotalMiss;
			if (num < 1)
			{
				return 0;
			}
			int num2 = (int)(exAccuracy / (float)num * 100000f);
			ToAccEx = (int)(exAccuracy / (float)num * 10000f);
			ToAccExact = (int)(exactAccuracy / (float)num * 10000f);
			ToAccGreat = (int)(greatAccuracy / (float)num * 10000f);
			ToAccRight = (int)(rightAccuracy / (float)num * 10000f);
			uploadScore[2] = exAccuracy;
			uploadScore[3] = (float)num;
			num2 = ((num2 <= 15000) ? num2 : 15000);
			exAccuracy = 0f;
			exactAccuracy = 0f;
			greatAccuracy = 0f;
			rightAccuracy = 0f;
			return num2;
		}

		public static int GetRatseq(int maxCombo, int totalCombo)
		{
			if (totalCombo < 1)
			{
				return 0;
			}
			int num = (int)((float)maxCombo / (float)totalCombo * 10000f);
			uploadScore[4] = (float)maxCombo;
			uploadScore[5] = (float)totalCombo;
			if (num > 10000)
			{
				return 10000;
			}
			return num;
		}

		public static void AddTotalCount(int addCombo)
		{
			TotalCombo += addCombo;
		}

		public static int GetSyncNumber(int maxCombo)
		{
			DateTime now = DateTime.Now;
			GetRatacc();
			int num = GetRatadd() / 10;
			int num2 = GetRatseq(maxCombo, TotalCombo) / 10;
			return (int)(GetUploadSyncNumber()[0] * 10000f);
		}

		public static float[] GetUploadSyncNumber()
		{
			uploadScore[0] = Math.Min(uploadScore[1] / uploadScore[3], 1f);
			uploadScore[0] += Math.Min(uploadScore[2] / uploadScore[3], 0.15f);
			uploadScore[0] += Math.Min(uploadScore[4] / uploadScore[5] / 10f, 0.1f);
			return uploadScore;
		}

		public static string GetRankGeadeString(int synNumber)
		{
			string result = "--";
			if (synNumber >= 11700)
			{
				result = "EX";
			}
			else if (synNumber >= 11000)
			{
				result = "S";
			}
			else if (synNumber >= 9500)
			{
				result = "A";
			}
			else if (synNumber >= 7500)
			{
				result = "B";
			}
			return result;
		}

		static JudgeGrade()
		{
		}

		public const long EX = 450000L;

		public const long EXACT = 900000L;

		public const long GREAT = 1500000L;

		public const long RIGHT = 2500000L;

		private static float baseAccuracy;

		private static float exAccuracy;

		private static float exactAccuracy;

		private static float greatAccuracy;

		private static float rightAccuracy;

		private static float[] uploadScore;

		public static long ScriptDelay = 180000L;

		private static void InitializeAndConfigureConsole()
		{
			if (consoleInitialized)
			{
				return;
			}
			NativeMethods.AllocConsole();
			Thread.Sleep(50);
			IntPtr consoleWindow = NativeMethods.GetConsoleWindow();
			if (consoleWindow != IntPtr.Zero)
			{

				int style = NativeMethods.GetWindowLong(consoleWindow, NativeMethods.GWL_STYLE);
				int newStyle = style & ~(NativeMethods.WS_CAPTION | NativeMethods.WS_THICKFRAME | NativeMethods.WS_SYSMENU | NativeMethods.WS_HSCROLL | NativeMethods.WS_VSCROLL);
				NativeMethods.SetWindowLong(consoleWindow, NativeMethods.GWL_STYLE, newStyle);

				uint flags = NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE;
				NativeMethods.SetWindowPos(consoleWindow, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, flags);
			}

			IntPtr stdHandle = NativeMethods.GetStdHandle(-11);
			SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
			FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
			StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8) { AutoFlush = true };
			Console.SetOut(writer);

			try
			{
				Console.SetWindowSize(30, 11);
				Console.SetBufferSize(30, 11);
			}
			catch {}

			IntPtr stdHandleFont = NativeMethods.GetStdHandle(-11);
			NativeMethods.CONSOLE_FONT_INFOEX fontInfo = new NativeMethods.CONSOLE_FONT_INFOEX
			{
				cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.CONSOLE_FONT_INFOEX))
			};
			NativeMethods.GetCurrentConsoleFontEx(stdHandleFont, false, ref fontInfo);
			fontInfo.dwFontSize.Y = 16;
			fontInfo.FontWeight = 700;
			fontInfo.FaceName = "Consolas";
			NativeMethods.SetCurrentConsoleFontEx(stdHandleFont, false, ref fontInfo);
			consoleInitialized = true;
		}

		private static void UpdateConsoleDisplay()
		{
			if (!consoleInitialized)
			{
				return;
			}
			Console.SetCursorPosition(0, 0);
			float num = (totalJudgedNotes == 0) ? 0f : ((float)totalKnockDistance / (float)totalJudgedNotes / 10000f);
			Console.Write("                            \n"); 
			Console.Write("       -- Hit Stats --      \n");
			Console.Write(" ---------------------------\n");
			Console.Write(string.Format("  Ex Exact :  {0,-13}\n", TotalEx));
			Console.Write(string.Format("  Exact    :  {0,-13}\n", TotalExact - TotalEx));
			Console.Write(string.Format("  Great    :  {0,-13}\n", TotalGreat));
			Console.Write(string.Format("  Right    :  {0,-13}\n", TotalRight));
			Console.Write(string.Format("  Miss     :  {0,-13}\n", TotalMiss));
			Console.Write(" ---------------------------\n");
			Console.Write(string.Format("  Avg Delay:  {0,-8:F2} ms\n", num));
		}

		private static class NativeMethods
		{

			public const int GWL_STYLE = -16;
			public const int WS_CAPTION = 0x00C00000;
			public const int WS_THICKFRAME = 0x00040000;
			public const int WS_SYSMENU = 0x00080000;
			public const int WS_HSCROLL = 0x00100000;
			public const int WS_VSCROLL = 0x00200000;
			public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
			public const uint SWP_NOMOVE = 0x0002;
			public const uint SWP_NOSIZE = 0x0001;
			public const uint SWP_NOACTIVATE = 0x0010;

			[DllImport("kernel32.dll")]
			public static extern bool AllocConsole();

			[DllImport("kernel32.dll")]
			public static extern IntPtr GetStdHandle(int nStdHandle);

			[DllImport("kernel32.dll")]
			public static extern IntPtr GetConsoleWindow();

			[DllImport("user32.dll")]
			public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

			[DllImport("user32.dll")]
			public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

			[DllImport("user32.dll")]
			public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

			[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
			public static extern bool GetCurrentConsoleFontEx(IntPtr h, bool b, ref CONSOLE_FONT_INFOEX c);

			[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
			public static extern bool SetCurrentConsoleFontEx(IntPtr h, bool b, ref CONSOLE_FONT_INFOEX c);

			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
			public struct CONSOLE_FONT_INFOEX
			{

				public uint cbSize;

				public uint nFont;

				public COORD dwFontSize;

				public int FontFamily;

				public int FontWeight;

				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
				public string FaceName;
			}

			public struct COORD
			{

				public short X;

				public short Y;
			}
		}
	}
}