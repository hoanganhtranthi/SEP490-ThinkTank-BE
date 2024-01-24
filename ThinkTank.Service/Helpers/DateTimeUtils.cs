using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.Helpers
{
    public static class DateTimeUtils
    {
        public static DateTime GetStartOfDate(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0);
        }

        public static DateTime GetEndOfDate(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, 23, 59, 59);
        }

        public static (DateTime, DateTime) GetLastAndFirstDateInCurrentMonth()
        {
            var now = DateTime.Now;
            var first = new DateTime(now.Year, now.Month, 1);
            var last = first.AddMonths(1).AddDays(-1);
            return (first, last);
        }
        public static DateTime GetCurrentDate()
        {
            return DateTime.UtcNow.AddHours(7);
        }
        public static int GetQuarter(this DateTime dt)
            {
                return (dt.Month - 1) / 3 + 1;
             }
    }
}
