using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ScheduleEase
{
    internal class GraphService
    {
        private readonly string[] _scopes = new[] { "User.Read", "Calendars.ReadWrite" };
        private const string ClientId = "34aea1ef-ac95-4474-b079-205b0da5308c";
        private const string TenantId = "common";
        private GraphServiceClient _client;

        public GraphService()
        {
            Initialize();
        }

        private void Initialize()
        {
            // assume Windows for this sample
            if (OperatingSystem.IsWindows())
            {
                var options = new InteractiveBrowserCredentialOptions
                {
                    ClientId = ClientId,
                    TenantId = TenantId,
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                    RedirectUri = new Uri("https://login.microsoftonline.com/common/oauth2/nativeclient"),
                };

                InteractiveBrowserCredential interactiveCredential = new(options);
                _client = new GraphServiceClient(interactiveCredential, _scopes);
            }
            else
            {
                // TODO: Add iOS/Android support
            }


        }

        public async Task<User> AddEventToCalendar(string eventName,int month,int day,int hour,int minute,int Duration,string body="" )
        {
            try
            {

                DateTimeOffset eventStart = new DateTimeOffset(DateTime.Now.Year, month, day
                    , hour, minute, 0, TimeSpan.FromHours(1)); // UTC+1

                DateTimeOffset eventEnd = new DateTimeOffset(eventStart.Year, eventStart.Month, eventStart.Day
                    , eventStart.Hour+Duration, eventStart.Minute, 0, TimeSpan.FromHours(1)); // UTC+1

                Event newEvent = new Event
                {
                    Subject = eventName,
                    Start = new DateTimeTimeZone { DateTime = eventStart.ToString("o"), TimeZone = "UTC+1" },
                    End = new DateTimeTimeZone { DateTime = eventEnd.ToString("o"), TimeZone = "UTC+1" },
                    Body = new ItemBody { ContentType = BodyType.Text, Content = body }
                    
                };

              var result=  await _client.Me.Calendar.Events.PostAsync(newEvent);
                return null;
                    

                Console.WriteLine("Event added to your calendar!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user details: {ex}");
                return null;
            }
        }
    }
}
