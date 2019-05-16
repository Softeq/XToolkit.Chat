// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Globalization;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Common.Extensions;

namespace Softeq.XToolkit.Chat.Services
{
    public class FormatService : IFormatService
    {
        private const string ShortTimeFormat = "HH:mm";
        private const string MonthDateFormat = "MMM d";
        private const string FullDateFormat = "M/d/yyyy";

        public string PluralizeWithQuantity(int count, string plural, string singular)
        {
            return count + " " + (count == 1 ? singular : plural);
        }

        public string ToChatDateTimeFormat(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                var date = dateTime.Value;

                var format = date.IsToday()
                    ? ShortTimeFormat
                    : date.Year == DateTime.Today.Year
                        ? MonthDateFormat
                        : FullDateFormat;

                return date.ToString(format, CultureInfo.InvariantCulture);
            }

            return string.Empty;
        }

        public string ToMessageDateTimeFormat(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return dateTime?.ToString(ShortTimeFormat, CultureInfo.InvariantCulture);
            }

            return string.Empty;
        }

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
            return date.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
