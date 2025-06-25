namespace BlazorAI.Plugins
{
	using Microsoft.SemanticKernel;
	using System.ComponentModel;

	public class WeatherPlugin
	{
		private readonly IHttpClientFactory _httpClientFactory;
		public WeatherPlugin(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

		[KernelFunction("get_weather_forecast")]
		[Description("Gets the forecast for a given latitude, longitude and number of days. Can forecast up to 16 days in the future.")]
		[return: Description("JSON response containing the weather forecast data for the given location and range of days")]
		public async Task<string> GetWeatherForecastForLocationAsync(float latitude, float longitude, int days)
		{
			if (days <= 0 || days > 16) return "Day count is out of bounds. Days should be between 1 and 16";
			using HttpClient httpClient = _httpClientFactory.CreateClient();
			var response = await httpClient.GetStringAsync(
				$"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation,rain,showers,snowfall,weather_code,wind_speed_10m,wind_direction_10m,wind_gusts_10m&hourly=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation_probability,precipitation,rain,showers,snowfall,weather_code,cloud_cover,wind_speed_10m,uv_index&temperature_unit=fahrenheit&wind_speed_unit=mph&precipitation_unit=inch&forecast_days={days}");
			return response;
		}

		[KernelFunction("get_weather_recent")]
		[Description("Gets the weather details for recent previous weather at a given location. This can go a number of days up to 3 months into the past.")]
		[return: Description("JSON response containing the weather data for the given location and timespan in the past.")]
		public async Task<string> GetRecentWeatherForLocationAsync(float latitude, float longitude, int daysInPast)
		{
			using HttpClient httpClient = _httpClientFactory.CreateClient();
			var response = await httpClient.GetStringAsync(
				$"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&daily=weather_code,temperature_2m_max,temperature_2m_min,apparent_temperature_max,apparent_temperature_min,sunrise,sunset,daylight_duration,uv_index_max,precipitation_sum,rain_sum,showers_sum,snowfall_sum,precipitation_hours,wind_speed_10m_max,wind_gusts_10m_max&temperature_unit=fahrenheit&wind_speed_unit=mph&precipitation_unit=inch&past_days={daysInPast}");
			return response;
		}
	}
}
