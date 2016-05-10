using System;
using System.Net.Http;

using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

using System.Threading.Tasks;

using ModernHttpClient;

namespace Weather
{
	// Simple JSON deserializer. We've implemented serialization for fields
	// we actually require for our purposes (that's why some of them are missing
	// comparing to API response).
	[DataContract]
	public class WeatherForecast {
		[DataMember(Name="cityName")] public string Name { get; set; }
		[DataMember(Name="days")] public IList<WeatherForecastDay> Details { get; set; }
	}

	[DataContract]
	public class WeatherForecastDay {
		[DataMember(Name="sunrise")] public string Sunrise { get; set; }
		[DataMember(Name="sunset")] public string Sunset { get; set; }
		[DataMember(Name="date")] public int Timestamp { get; set; }
		public DateTime Date {
			get {
				return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(this.Timestamp).ToLocalTime();
		}}
		[DataMember(Name="timeOfDay")] public IList<WeatherForecastHour> Hours { get; set; }
		public WeatherForecastHour Current { 
			get {
				Int32 currentTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
				WeatherForecastHour currentForecast = this.Hours [0];
				var currentDifference = Math.Abs (currentTimestamp - currentForecast.Timestamp);

				foreach (WeatherForecastHour hourForecast in this.Hours) {
					var difference = Math.Abs (currentTimestamp - hourForecast.Timestamp);
					if (Math.Abs (currentTimestamp - hourForecast.Timestamp) < currentDifference) {
						currentDifference = difference;
						currentForecast = hourForecast;
					}
				}

				return currentForecast;
			}
		}
		public WeatherForecastHour Forecast { 
			get {
				WeatherForecastHour highestTemperatureForecast = this.Hours [0];
				int highestTemperature = -255;
				foreach (WeatherForecastHour hourForecast in this.Hours) {
					if (hourForecast.Temperature > highestTemperature) {
						highestTemperature = hourForecast.Temperature;
						highestTemperatureForecast = hourForecast;
					}
				}

				return highestTemperatureForecast;
			}
		}
	}

	[DataContract]
	public class WeatherForecastHour {
		[DataMember(Name="temperature")] public int Temperature { get; set; }
		[DataMember(Name="icon")] public string Icon { get; set; }
		[DataMember(Name="precipitation")] public string Precipitation { get; set; }
		[DataMember(Name="pressure")] public int Pressure { get; set; }
		[DataMember(Name="hour")] public int Timestamp { get; set; }
	}

	public class WeatherService {
		public WeatherService() {
		}

		public async Task<WeatherForecast> Fetch(string url) {
			var serializer = new DataContractJsonSerializer(typeof(WeatherForecast));

			// We're using NativeMessageHandler from ModernHttpClient to utilize
			// platform specific connection managers (NSURLSession for iOS and OKHttp for Android).
			HttpClient client = new HttpClient (new NativeMessageHandler());

			Stream stream = await client.GetStreamAsync (url);
			WeatherForecast forecast = (WeatherForecast)serializer.ReadObject (stream);
			return forecast;
		}
	}
}

