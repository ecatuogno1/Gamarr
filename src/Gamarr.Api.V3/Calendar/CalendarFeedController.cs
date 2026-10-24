using System;
using System.Collections.Generic;
using System.Linq;
using Gamarr.Http;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Games;
using NzbDrone.Core.Tags;

namespace Gamarr.Api.V3.Calendar
{
    [V3FeedController("calendar")]
    public class CalendarFeedController : Controller
    {
        private readonly IGameService _gameService;
        private readonly ITagService _tagService;

        public CalendarFeedController(IGameService gameService, ITagService tagService)
        {
            _gameService = gameService;
            _tagService = tagService;
        }

        [HttpGet("Gamarr.ics")]
        public Results<ContentHttpResult, NoContent> GetCalendarFeed(int pastDays = 7, int futureDays = 28, string tags = "", bool unmonitored = false, IReadOnlyCollection<CalendarReleaseType> releaseTypes = null)
        {
            var start = DateTime.Today.AddDays(-pastDays);
            var end = DateTime.Today.AddDays(futureDays);
            var parsedTags = new List<int>();

            if (tags.IsNotNullOrWhiteSpace())
            {
                parsedTags.AddRange(tags.Split(',').Select(_tagService.GetTag).Select(t => t.Id));
            }

            var games = _gameService.GetGamesBetweenDates(start, end, unmonitored);
            var calendar = new Ical.Net.Calendar
            {
                ProductId = "-//Gamarr//Gamarr//EN"
            };

            var calendarName = "Gamarr Games Calendar";
            calendar.AddProperty(new CalendarProperty("NAME", calendarName));
            calendar.AddProperty(new CalendarProperty("X-WR-CALNAME", calendarName));

            foreach (var game in games.OrderBy(v => v.Added))
            {
                if (parsedTags.Any() && parsedTags.None(game.Tags.Contains))
                {
                    continue;
                }

                // Games have a single release date (on the game's platform), releaseTypes is ignored
                CreateEvent(calendar, game);
            }

            var serializer = new SerializerFactory().Build(calendar.GetType(), new SerializationContext()) as IStringSerializer;
            var icalendar = serializer?.SerializeToString(calendar);

            return icalendar is null ? TypedResults.NoContent() : TypedResults.Content(icalendar, "text/calendar");
        }

        private static void CreateEvent(Ical.Net.Calendar calendar, Game game)
        {
            var date = game.GetReleaseDate();

            if (!date.HasValue)
            {
                return;
            }

            var metadata = game.GameMetadata.Value;

            var occurrence = calendar.Create<CalendarEvent>();
            occurrence.Uid = "Gamarr_game_" + game.Id + "_release";
            occurrence.Status = metadata.Status == GameStatusType.Released ? EventStatus.Confirmed : EventStatus.Tentative;

            occurrence.Start = new CalDateTime(date.Value, false);
            occurrence.End = occurrence.Start;

            occurrence.Description = metadata.Overview;
            occurrence.Categories = metadata.Developers.Concat(metadata.Publishers).Distinct().ToList();

            occurrence.Summary = game.Platform == null ? metadata.Title : $"{metadata.Title} ({game.Platform.ShortName})";
        }
    }
}
