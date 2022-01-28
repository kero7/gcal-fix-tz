using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Calendar.v3.Data;

namespace CalendarFix {
	class Program {
		static void Main(string[] args) {
			Console.WriteLine("============================================================");
			Console.WriteLine("  Google Calendar Timezone Fix");
			Console.WriteLine("  (C) Kerosoft 2022");
			Console.WriteLine("============================================================");
			Console.WriteLine();
			Console.WriteLine("Requesting access token... Please login to your account on your browser.");
			Console.WriteLine("Once authenticated, the token will be stored on your harddrive.");
			Console.WriteLine();

			Console.WriteLine("Your calendars are:");
			var gc = new GoogleCalendarAPI();
			var calendars = gc.GetCalendars();
			for(int i = 0; i < calendars.Count; i++) {
				Console.WriteLine($"({i + 1:00}) {calendars[i].Summary}");
			}

			Console.WriteLine();
			string userInput;
			int calendarIndex;
			do {
				Console.WriteLine("Select a calendar number above to fix timezone in entries. Enter -1 to exit here.");
				userInput = Console.ReadLine();
				int.TryParse(userInput, out calendarIndex);
				if(calendarIndex == -1) {
					return;
				} else if(calendarIndex > 0 && calendarIndex <= calendars.Count) {
					break;
				}
			} while(calendarIndex == 0);

			Console.WriteLine();
			Console.WriteLine($"Selected calendar is `{calendars[calendarIndex - 1].Summary}`");
			var calendarId = calendars[calendarIndex - 1].Id;

			Console.WriteLine();
			Console.WriteLine("Retrieving calendar events... (this may take a while depend on your item counts.)");
			var events = gc.GetCalendarEvents(calendarId);
			Console.WriteLine($"{events.Count} events were downloaded.");

			Console.WriteLine();
			Console.WriteLine("Searching UTC based events...");
			var affectedItems = new List<Event>();
			foreach(var item in events) {
				if(item.Start.DateTime == null || item.End.DateTime == null) {
					// Skip; this is all-day event
					continue;
				}
				if(item.Start.TimeZone == "UTC" || item.End.TimeZone == "UTC") {
					affectedItems.Add(item);
				}
			}

			// Sort in ascending order by start time
			affectedItems.Sort((a, b) => {
				// It is safe to cast from DateTime? to DateTime, since events which have TimeZone have "Time"
				return DateTime.Compare((DateTime)a.Start.DateTime, (DateTime)b.Start.DateTime);
			});

			Console.WriteLine();
			if(affectedItems.Count == 0) {
				Console.WriteLine("There seems no UTC entries... Good bye!");
				Console.ReadLine();
				return;
			}

			var cnt = 0;
			foreach(var item in affectedItems) {
				cnt++;
				Console.WriteLine($"#{cnt:00000} {item.Start.DateTime:yyyy/MM/dd HH:mm:ss} - {item.End.DateTime:yyyy/MM/dd HH:mm:ss} {item.Summary}");
			}

			Console.WriteLine($"Are you sure to update {affectedItems.Count} item(s)? Answer YES to continue...");
			userInput = Console.ReadLine();
			if(string.Compare(userInput, "yes", true) != 0) {
				Console.WriteLine("Ok, good bye!");
				Console.ReadLine();
				return;
			}

			Console.WriteLine();
			cnt = 0;
			Console.WriteLine("Updating calendar events...");
			foreach(var item in affectedItems) {
				item.Start.TimeZone = "Asia/Tokyo";
				item.End.TimeZone = "Asia/Tokyo";
				cnt++;
				Console.WriteLine($"#{cnt:00000} {item.Start.DateTime:yyyy/MM/dd HH:mm:ss} - {item.End.DateTime:yyyy/MM/dd HH:mm:ss} {item.Summary}");
				gc.UpdateCalendarEvent(calendarId, item);
			}
			Console.WriteLine();

			Console.WriteLine("All set. Good bye!");
			Console.ReadLine();
		}
	}
}
