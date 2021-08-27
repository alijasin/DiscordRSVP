using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordEventSignupBot
{
    static class Util
    {
        public static DateTime ParseDate(string weekday, string time)
        {
            DateTime date;

            if (!DateTime.TryParse(time, out date))
            {
                throw new Exception("No bueno");
            }

            for (int daysIntoFuture = 0; daysIntoFuture < 7; daysIntoFuture++)
            {
                var cand = date + new TimeSpan(daysIntoFuture, 0, 0, 0);
                if (cand.DayOfWeek.ToString().ToLower().StartsWith(weekday.ToLower()))
                {
                    return cand;
                }
            }

            throw new Exception("What day is that bud?");
        }
    }
}
