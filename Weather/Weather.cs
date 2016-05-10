using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Weather
{
	public class App : Application
	{
		WeatherService service;

		Label cityName;
		Label dayName;

		Label temperature;
		Label pressure;
		Label precipitation;

		RelativeLayout footerLayout;

		public App ()
		{
			this.service = new WeatherService();

			// The root page of your application
			MainPage = this.GetMainPage ();
		}

		public Page GetMainPage ()
		{
			RelativeLayout mainLayout = new RelativeLayout ();

			mainLayout.Children.Add (new Image() {
					Source = ImageSource.FromResource("Weather.Images.bg.png"),
					Aspect = Aspect.AspectFill
				}, 
				Constraint.Constant (0),
				Constraint.Constant (0),
				Constraint.RelativeToParent ((parent) => { return parent.Width; }),
				Constraint.RelativeToParent ((parent) => { return parent.Height; })
			);

			// Header layout
			RelativeLayout headerLayout = new RelativeLayout ();
			mainLayout.Children.Add (headerLayout,
				Constraint.Constant (0),
				Constraint.Constant (20),
				Constraint.RelativeToParent ((parent) => { return parent.Width; }),
				Constraint.Constant (100));

			headerLayout.Children.Add (new Image() {
					Source = ImageSource.FromResource("Weather.Images.cloud.png"),
				}, 
				Constraint.RelativeToParent ((parent) => { return parent.Width / 2 - 80; }),
				Constraint.RelativeToParent ((parent) => { return parent.Height / 2 - 22; }),
				Constraint.Constant (57),
				Constraint.Constant (44));

			this.cityName = new Label () {
				Text = "WARSZAWA",
				FontSize = 16,
				TextColor = Color.White
			};

			headerLayout.Children.Add (this.cityName, 
				Constraint.RelativeToParent ((parent) => { return parent.Width / 2 - 10; }),
				Constraint.RelativeToParent ((parent) => { return parent.Height / 2 - 11; }),
				Constraint.Constant (100),
				Constraint.Constant (18));

			this.dayName = new Label () {
				Text = "ŚRODA, 4 MAJ",
				FontSize = 8,
				TextColor = Color.White
			};

			headerLayout.Children.Add (this.dayName, 
				Constraint.RelativeToParent ((parent) => { return parent.Width / 2 - 10; }),
				Constraint.RelativeToParent ((parent) => { return parent.Height / 2 + 7; }),
				Constraint.Constant (100),
				Constraint.Constant (10));
			// >

			// Footer layout
			this.footerLayout = new RelativeLayout ();

			mainLayout.Children.Add (this.footerLayout,
				Constraint.Constant (0),
				Constraint.RelativeToParent ((parent) => { return parent.Height - 120; }),
				Constraint.RelativeToParent ((parent) => { return parent.Width; }),
				Constraint.Constant (120));
			// >

			// Degrees and central area layout
			this.temperature = new Label () {
				Text = "--°",
				FontSize = 200,
				TextColor = Color.White,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			};

			RelativeLayout degreesLayout = new RelativeLayout ();
			mainLayout.Children.Add (degreesLayout,
				Constraint.Constant (0),
				Constraint.RelativeToView(headerLayout, (parent, sibling) => {
					return sibling.Y + sibling.Height;
				}), 
				Constraint.RelativeToParent ((parent) => { return parent.Width; }),
				Constraint.RelativeToView(footerLayout, (parent, sibling) => {
					return sibling.Y - sibling.Height;
				}));

			degreesLayout.Children.Add (this.temperature,
				Constraint.Constant (0),
				Constraint.Constant (0),
				Constraint.RelativeToParent ((parent) => { return parent.Width; }),
				Constraint.RelativeToParent ((parent) => { return parent.Height / 1.5; }));
			// >

			// Platform specific actions
			Device.OnPlatform (
				iOS: () => {
					this.temperature.FontFamily = "HelveticaNeue-UltraLight";
					this.cityName.FontFamily = "HelveticaNeue-Light";
				},
				Android: () => {
					this.temperature.FontFamily = "Sans-Serif-Thin";
					this.cityName.FontFamily = "Sans-Serif-Light";
				}
			);
			// >

			return new ContentPage {
				Content = mainLayout,
			};
		}

		private async void refreshForecast() {
			var today = DateTime.Now;
			var dayOfWeekName = today.DayOfWeek.ToString ().ToUpper ();
			var monthName = today.ToString("MMMM").ToUpper ();
			this.dayName.Text = $"{dayOfWeekName}, {today.Day} {monthName}";

			// Currently, as an example, we're using hardcoded Warsaw location
			var forecast = await this.service.Fetch ("https://api.o2.pl/weather/api/o2/weather?cid=1201290&days=4");
			var currentForecast = forecast.Details [0].Current;
			this.temperature.Text = $"{currentForecast.Temperature}°";
			this.cityName.Text = forecast.Name.ToUpper ();

			// Forecast for coming days
			var offset = 0;
			List<string> cellColors = new List<string>(new string[] {
				"#d9a9ce", "#d29cc7", "#c586ba", "ba75b1"
			});
			var iterationOffset = (int)this.footerLayout.Width / cellColors.Count;
			for (int i = 0; i < cellColors.Count; i++) {
				var dayForecast = forecast.Details [i];
				var cellLayout = new RelativeLayout ();
				cellLayout.BackgroundColor = Color.FromHex (cellColors [i]);

				this.footerLayout.Children.Add (cellLayout,
					Constraint.Constant (offset),
					Constraint.Constant (0),
					Constraint.Constant (iterationOffset),
					Constraint.RelativeToParent ((parent) => {
						return parent.Height;
					}));

				var dayName = new Label () {
					Text = dayForecast.Date.DayOfWeek.ToString ().ToUpper (),
					FontSize = 8,
					TextColor = Color.White,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center
				};

				cellLayout.Children.Add (dayName, 
					Constraint.Constant (0),
					Constraint.Constant (18),
					Constraint.RelativeToParent ((parent) => { return parent.Width; }),
					Constraint.Constant (10));

				var dayTemperature = new Label () {
					Text = $"{dayForecast.Forecast.Temperature}°",
					FontSize = 22,
					TextColor = Color.White,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center
				};

				cellLayout.Children.Add (dayTemperature, 
					Constraint.Constant (3),
					Constraint.RelativeToParent ((parent) => { return parent.Height - 37; }),
					Constraint.RelativeToParent ((parent) => { return parent.Width; }),
					Constraint.Constant (18));

				cellLayout.Children.Add (new Image() {
						Source = ImageSource.FromResource("Weather.Images.small_cloud.png"),
					}, 
					Constraint.RelativeToParent ((parent) => { return parent.Width / 2 - 12; }),
					Constraint.RelativeToParent ((parent) => { return parent.Height / 2 - 15; }),
					Constraint.Constant (24),
					Constraint.Constant (22));

				// Platform specific actions
				Device.OnPlatform (
					iOS: () => {
						dayTemperature.FontFamily = "HelveticaNeue-UltraLight";
						dayName.FontFamily = "HelveticaNeue-Light";
					},
					Android: () => {
						dayTemperature.FontFamily = "Sans-Serif-Thin";
						dayName.FontFamily = "Sans-Serif-Light";
					}
				);
				// >

				offset += iterationOffset;
			}
			// >
		}

		protected override void OnStart ()
		{
			this.refreshForecast ();
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
			this.refreshForecast ();
		}
	}
}

