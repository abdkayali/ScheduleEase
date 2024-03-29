﻿using Azure.Identity;
using Microsoft.Graph.Models;
using Microsoft.Graph;
using System;
using Microsoft.Datasync.Client;
using Microsoft.Identity.Client;
using System.Diagnostics;
using ScheduleEase.Helpers;
using Azure.Core;
using Microsoft.Kiota.Abstractions.Authentication;

namespace ScheduleEase.Services
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

        private async void Initialize()
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
                AuthenticationService authenticationService = new AuthenticationService();
                _client = authenticationService.GraphClient;
            }


        }

        public async Task AddEventToCalendar(string subject, DateTimeTimeZone start, DateTimeTimeZone end, string body)
        {

            Event newEvent = new Event
            {
                Subject = subject,
                Start = start,
                End = end,
                Body = new ItemBody { ContentType = BodyType.Text, Content = body }
            };
            await _client.Me.Calendar.Events.PostAsync(newEvent);
        }
        public async Task<User> AddEventToCalendar(string eventName, int month, int day, int hour, int minute, int Duration, string body = "")
        {
            try
            {

                DateTimeOffset eventStart = new DateTimeOffset(DateTime.Now.Year, month, day
                    , hour, minute, 0, TimeSpan.FromHours(1)); // UTC+1
                DateTimeOffset eventEnd = new DateTimeOffset(eventStart.Year, eventStart.Month, eventStart.Day
                    , eventStart.Hour + Duration, eventStart.Minute, 0, TimeSpan.FromHours(1)); // UTC+1

                Event newEvent = new Event
                {
                    Subject = eventName,
                    Start = new DateTimeTimeZone { DateTime = eventStart.ToString("o"), TimeZone = "UTC+1" },
                    End = new DateTimeTimeZone { DateTime = eventEnd.ToString("o"), TimeZone = "UTC+1" },
                    Body = new ItemBody { ContentType = BodyType.Text, Content = body }

                };

                var result = await _client.Me.Calendar.Events.PostAsync(newEvent);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user details: {ex}");
                return null;
            }
        }
        public async Task<User> GetMyDetailsAsync()
        {
            try
            {
                return await _client.Me.GetAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error loading user details: {ex}");
                return null;
            }
        }
    }
}
