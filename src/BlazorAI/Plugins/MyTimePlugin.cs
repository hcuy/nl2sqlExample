using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace BlazorAI.Plugins
{
	public class MyTimePlugin
	{
		/// <summary>
		/// Returns the current DateTime.
		/// </summary>
		[KernelFunction("get_current_datetime")]
		[Description("Returns the current DateTime.")]
		public DateTime GetCurrentDateTime()
		{
			return DateTime.Now;
		}

		/// <summary>
		/// Returns the Year for a given DateTime.
		/// </summary>
		/// <param name="date">A valid DateTime object.</param>
		[KernelFunction("get_year")]
		[Description("Returns the Year for a given DateTime.")]
		public int GetYear(DateTime date)
		{
			return date.Year;
		}

		/// <summary>
		/// Returns the Month for a given DateTime.
		/// </summary>
		/// <param name="date">A valid DateTime object.</param>
		[KernelFunction("get_month")]
		[Description("Returns the Month for a given DateTime.")]
		public int GetMonth(DateTime date)
		{
			return date.Month;
		}

		/// <summary>
		/// Returns the Day of Week for a given DateTime.
		/// </summary>
		/// <param name="date">A valid DateTime object.</param>
		[KernelFunction("get_day_of_week")]
		[Description("Returns the Day of Week for a given DateTime.")]
		public DayOfWeek GetDayOfWeek(DateTime date)
		{
			return date.DayOfWeek;
		}
	}
}
