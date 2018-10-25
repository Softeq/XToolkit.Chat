// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Chat.Models.Interfaces;

namespace Softeq.XToolkit.Chat.Services
{
    public class FormatService : IFormatService
    {
        private const string ShortTimeFormat = "HH:mm";

        public string PluralizeWithQuantity(int count, string plural, string singular)
        {
            return count + " " + (count == 1 ? singular : plural);
        }

        public string ToShortTimeFormat(DateTime? dateTime) => dateTime?.ToString(ShortTimeFormat) ?? string.Empty;

        public string Humanize(DateTimeOffset date, string today, string yesterday)
        {
            if (date.Date == DateTime.Today)
            {
                return today;
            }
            if (date.Date == DateTime.Today.AddDays(-1))
            {
                return yesterday;
            }
            var format = date.Year == DateTime.Today.Year ? "dd MMM" : "dd MMM yyyy";
            return date.ToString(format);
        }
    }
}
