using System;

namespace BMSLib
{

	public class JudgeGrade
	{

		public JudgeGrade()
		{
		}

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
			JudgeGrade.baseAccuracy = 0f;
			JudgeGrade.exAccuracy = 0f;
			JudgeGrade.exactAccuracy = 0f;
			JudgeGrade.greatAccuracy = 0f;
			JudgeGrade.rightAccuracy = 0f;
			JudgeGrade.TotalEx = 0;
			JudgeGrade.TotalExact = 0;
			JudgeGrade.TotalGreat = 0;
			JudgeGrade.TotalRight = 0;
			JudgeGrade.TotalMiss = 0;
			JudgeGrade.TotalCombo = 0;
			JudgeGrade.uploadScore = new float[6];
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
			int result = -1;
			bool flag = knockDistance < 0L;
			knockDistance = Math.Abs(knockDistance);
			if (knockDistance < 450000L)
			{
				result = 0;
				JudgeGrade.baseAccuracy += 1f;
				JudgeGrade.exAccuracy += 0.05f + 0.1f * (1f - (float)knockDistance / 450000f);
				JudgeGrade.exactAccuracy += 1f;
				JudgeGrade.TotalEx++;
				JudgeGrade.TotalExact++;
			}
			else if (knockDistance < 900000L)
			{
				result = 1;
				JudgeGrade.baseAccuracy += 1f;
				JudgeGrade.exactAccuracy += 1f;
				JudgeGrade.TotalExact++;
			}
			else if (knockDistance < 1500000L)
			{
				result = 2;
				JudgeGrade.baseAccuracy += 0.7f + 0.2f * (1f - (float)(knockDistance - 900000L) / 600000f);
				JudgeGrade.greatAccuracy += 0.7f + 0.2f * (1f - (float)(knockDistance - 900000L) / 600000f);
				JudgeGrade.TotalGreat++;
			}
			else if (knockDistance < 2500000L)
			{
				if (!flag)
				{
					result = 3;
					JudgeGrade.baseAccuracy += 0.1f + 0.5f * (1f - (float)(knockDistance - 900000L) / 1600000f);
					JudgeGrade.rightAccuracy += 0.1f + 0.5f * (1f - (float)(knockDistance - 900000L) / 1600000f);
					JudgeGrade.TotalRight++;
				}
			}
			else if (!flag)
			{
				result = 4;
				JudgeGrade.TotalMiss++;
			}
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
			int num = JudgeGrade.TotalExact + JudgeGrade.TotalGreat + JudgeGrade.TotalRight + JudgeGrade.TotalMiss;
			if (num < 1)
			{
				return 0;
			}
			int num2 = (int)(JudgeGrade.baseAccuracy / (float)num * 10000f);
			JudgeGrade.uploadScore[1] = JudgeGrade.baseAccuracy;
			JudgeGrade.uploadScore[3] = (float)num;
			num2 = ((num2 <= 10000) ? num2 : 10000);
			JudgeGrade.baseAccuracy = 0f;
			return num2;
		}

		private static int GetRatadd()
		{
			int num = JudgeGrade.TotalExact + JudgeGrade.TotalGreat + JudgeGrade.TotalRight + JudgeGrade.TotalMiss;
			if (num < 1)
			{
				return 0;
			}
			int num2 = (int)(JudgeGrade.exAccuracy / (float)num * 100000f);
			JudgeGrade.ToAccEx = (int)(JudgeGrade.exAccuracy / (float)num * 10000f);
			JudgeGrade.ToAccExact = (int)(JudgeGrade.exactAccuracy / (float)num * 10000f);
			JudgeGrade.ToAccGreat = (int)(JudgeGrade.greatAccuracy / (float)num * 10000f);
			JudgeGrade.ToAccRight = (int)(JudgeGrade.rightAccuracy / (float)num * 10000f);
			JudgeGrade.uploadScore[2] = JudgeGrade.exAccuracy;
			JudgeGrade.uploadScore[3] = (float)num;
			num2 = ((num2 <= 15000) ? num2 : 15000);
			JudgeGrade.exAccuracy = 0f;
			JudgeGrade.exactAccuracy = 0f;
			JudgeGrade.greatAccuracy = 0f;
			JudgeGrade.rightAccuracy = 0f;
			return num2;
		}

		public static int GetRatseq(int maxCombo, int totalCombo)
		{
			if (totalCombo < 1)
			{
				return 0;
			}
			int num = (int)((float)maxCombo / (float)totalCombo * 10000f);
			JudgeGrade.uploadScore[4] = (float)maxCombo;
			JudgeGrade.uploadScore[5] = (float)totalCombo;
			return (num <= 10000) ? num : 10000;
		}

		public static void AddTotalCount(int addCombo)
		{
			JudgeGrade.TotalCombo += addCombo;
		}

		public static int GetSyncNumber(int maxCombo)
		{
			DateTime now = DateTime.Now;
			int num = JudgeGrade.GetRatacc() + JudgeGrade.GetRatadd() / 10 + JudgeGrade.GetRatseq(maxCombo, JudgeGrade.TotalCombo) / 10;
			return (int)(JudgeGrade.GetUploadSyncNumber()[0] * 10000f) + num * 0;
		}

		public static float[] GetUploadSyncNumber()
		{
			JudgeGrade.uploadScore[0] = Math.Min(JudgeGrade.uploadScore[1] / JudgeGrade.uploadScore[3], 1f);
			JudgeGrade.uploadScore[0] += Math.Min(JudgeGrade.uploadScore[2] / JudgeGrade.uploadScore[3], 0.15f);
			JudgeGrade.uploadScore[0] += Math.Min(JudgeGrade.uploadScore[4] / JudgeGrade.uploadScore[5] / 10f, 0.1f);
			return JudgeGrade.uploadScore;
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
	}
}