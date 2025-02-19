﻿using PowerStore.Services.Helpers;
using System;

namespace PowerStore.Web.Areas.Admin.Extensions
{
    public static class DateTimeMappingExtensions
    {
        public static DateTime? ConvertToUserTime(this DateTime? datetime, IDateTimeHelper dateTimeHelper)
        {
            if (datetime.HasValue)
            {
                datetime = dateTimeHelper.ConvertToUserTime(datetime.Value, TimeZoneInfo.Utc, dateTimeHelper.DefaultStoreTimeZone);
            }
            return datetime;
        }

        public static DateTime? ConvertToUtcTime(this DateTime? datetime, IDateTimeHelper dateTimeHelper)
        {
            if (datetime.HasValue)
            {
                datetime = dateTimeHelper.ConvertToUtcTime(datetime.Value, dateTimeHelper.DefaultStoreTimeZone);
            }
            return datetime;
        }

        public static DateTime ConvertToUserTime(this DateTime datetime, IDateTimeHelper dateTimeHelper)
        {
            return dateTimeHelper.ConvertToUserTime(datetime, TimeZoneInfo.Utc, dateTimeHelper.DefaultStoreTimeZone);
        }

        public static DateTime ConvertToUtcTime(this DateTime datetime, IDateTimeHelper dateTimeHelper)
        {
            return dateTimeHelper.ConvertToUtcTime(datetime, dateTimeHelper.DefaultStoreTimeZone);
        }
    }
}