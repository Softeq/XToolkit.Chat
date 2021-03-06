﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.XToolkit.Chat.Models.Interfaces
{
    public interface IFormatService
    {
        string PluralizeWithQuantity(int count, string plural, string singular);

        string ToChatDateTimeFormat(DateTime? dateTime);

        string ToMessageDateTimeFormat(DateTime? dateTime);

        string Humanize(DateTimeOffset date, string today, string yesterday);
    }
}
