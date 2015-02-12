using System;
using System.Web.UI;

namespace Bitrix.CommunitySite
{
	public partial class UtilsDummy : UserControl
	{
	
	}

	public static class Utils
	{
		public static string GetNumericCase(int number, string nominative, string genitiveCase, string prepositional)
		{
			if (number < 0)
				number = 0 - number;

			number %= 100;
			if (number >= 5 && number <= 20)
				return prepositional;

			number %= 10;
			if (number == 1)
				return nominative;

			if (number >= 2 && number <= 4)
				return genitiveCase;

			return prepositional;
		}

		public static string GetTimePeriod(TimeSpan timespan)
		{
			string duration = "0 seconds";
			if (timespan.Days > 365)
			{
				int years = timespan.Days / 365;
				duration = String.Concat(years, " ", GetNumericCase(years, "year", "years", "years"));
			}
			else if (timespan.Days > 31)
			{
				int months = timespan.Days / 31;
				duration = String.Concat(months, " ", GetNumericCase(months, "month", "months", "months"));
			}
			else if (timespan.Days > 0)
			{
				int days = (int)timespan.TotalDays;
				duration = String.Concat(days, " ", GetNumericCase(days, "day", "days", "days"));
			}
			else if (timespan.Hours > 0)
			{
				duration = String.Concat(timespan.Hours, " ", GetNumericCase(timespan.Hours, "hour", "hours", "hours"));
			}
			else if (timespan.Minutes > 0)
			{
				duration = String.Concat(timespan.Minutes, " ", GetNumericCase(timespan.Minutes, "minute", "minutes", "minutes"));
			}
			else if (timespan.Seconds > 0)
			{
				duration = String.Concat(timespan.Seconds, " ", GetNumericCase(timespan.Seconds, "second", "seconds", "seconds"));
			}

			return duration;
		}
	}
}