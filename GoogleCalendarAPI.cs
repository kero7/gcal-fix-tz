using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace CalendarFix {
	// cf: https://www.nuget.org/packages/Google.Apis.Calendar.v3/
	//
	// Install below package viw NuGet Package Manager
	// PM> Install-Package Google.Apis.Calendar.v3
	class GoogleCalendarAPI {
		// https://console.developers.google.com/ >> APIs and Services >> Credentials, then issue "OAuth 2.0 Client IDs "
		private static readonly string OAUTH_CLIENT_ID = "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@";
		private static readonly string OAUTH_CLIENT_SECRET = "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@";

		CalendarService service;

		public GoogleCalendarAPI() {
			Initialize();
		}

		public void Initialize() {
			var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
				new ClientSecrets { ClientId = OAUTH_CLIENT_ID, ClientSecret = OAUTH_CLIENT_SECRET },
				new[] { CalendarService.Scope.Calendar },
				"user",
				CancellationToken.None,
				new FileDataStore("GoogleCalendarTimezoneFix")).Result; // will be stored at "%APPDATA%\GoogleCalendarTimezoneFix"
			service = new CalendarService(new BaseClientService.Initializer() { HttpClientInitializer = credential, ApplicationName = "Calendar Fix" });
		}

		public IList<CalendarListEntry> GetCalendars() {
			return service.CalendarList.List().Execute().Items;
		}

		public IList<Event> GetCalendarEvents(string calendarId) {
			var results = new List<Event>();
			if(string.IsNullOrWhiteSpace(calendarId)) {
				return results;
			}

			var req = service.Events.List(calendarId);
			do {
				var events = req.Execute();
				results.AddRange(events.Items);
				req.PageToken = events.NextPageToken;
			} while(req.PageToken != null);

			return results;
		}

		public void UpdateCalendarEvent(string calendarId, Event item) {
			if(string.IsNullOrWhiteSpace(calendarId) || item == null) {
				return;
			}

			var req = service.Events.Update(item, calendarId, item.Id);
			try {
				req.Execute();
			} catch(GoogleApiException e) {
				Console.WriteLine(e);
				throw e;
			}
		}
	}
}
