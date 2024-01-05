namespace KeyMan
{

    public class TimeDifference
    {
        public int Years;

        public int Months;

        public int Days;

        public int Hours;

        public int Minutes;

        public int Seconds;

        public int[] TimeDifferenceArray;

        public static int TimeDifferenceCmp(TimeDifference timeDifference, TimeDifference timeDifference1)
        {
            int size = timeDifference.TimeDifferenceArray.Length;

            for (int x = 0; x < size; x++)
                if (timeDifference.TimeDifferenceArray[x] > timeDifference1.TimeDifferenceArray[x])
                    return 1;
                else if (timeDifference.TimeDifferenceArray[x] < timeDifference1.TimeDifferenceArray[x])
                    return -1;

            return 0;
        }

        public static TimeDifference GetTimeDifference(DateTime dateTime, DateTime dateTime1)
        {
            TimeDifference timeDifference = null;

            try
            {
                timeDifference = new TimeDifference(
                    dateTime.Year - dateTime1.Year,
                    dateTime.Month - dateTime1.Month,
                    dateTime.Day - dateTime1.Day,
                    dateTime.Hour - dateTime1.Hour,
                    dateTime.Second - dateTime1.Second
                );
            }
            catch (Exception e)
            {
            }

            return timeDifference;
        }

        public DateTime AddTo(DateTime time)
        {
            return time.AddYears(this.Years)
                .AddMonths(this.Months)
                .AddDays(this.Days)
                .AddHours(this.Hours)
                .AddMinutes(this.Minutes)
                .AddSeconds(this.Seconds);
        }

        protected void MapTimeDifferenceArray()
        {
            this.TimeDifferenceArray = new int[]
            {
                this.Years,
                this.Months,
                this.Hours,
                this.Minutes,
                this.Seconds
            };
        }

        protected void MapProperties()
        {
            this.Years = this.TimeDifferenceArray[0];
            this.Months = this.TimeDifferenceArray[1];
            this.Days = this.TimeDifferenceArray[2];
            this.Hours = this.TimeDifferenceArray[3];
            this.Minutes = this.TimeDifferenceArray[4];
            this.Seconds = this.TimeDifferenceArray[5];
        }

        public static implicit operator TimeDifference(int[] timeDifference)
        {
            return new TimeDifference(timeDifference);
        }

        public TimeDifference(int years = 0, int months = 0, int days = 0, int hours = 0, int minutes = 0,
            int seconds = 0)
        {
            this.Years = years;
            this.Months = months;
            this.Days = days;
            this.Hours = hours;
            this.Minutes = minutes;
            this.Seconds = seconds;

            this.MapTimeDifferenceArray();
        }

        public TimeDifference(int[] timeDifferenceArray)
        {
            this.TimeDifferenceArray = new int[6];

            timeDifferenceArray.CopyTo(this.TimeDifferenceArray, 0);

            this.MapProperties();
        }
    }

    public class KeyValidityTime
    {
        public DateTime CreationTime { get; set; }

        public DateTime ExpiryTime { get; set; }

        public TimeDifference Difference;

        public bool IsExpired
        {
            get => this.GetIsExpired();
            set { }
        }

        public void SetTimeDifference(TimeDifference timeDifference)
        {
            this.Difference = timeDifference;
            this.ExpiryTime = this.Difference.AddTo(this.CreationTime);
        }

        private bool GetIsExpired()
        {
            return (DateTime.Now.CompareTo(this.ExpiryTime) > 0);
        }

        protected void AssertValidity()
        {
            int comp;
            
            if ((comp = this.CreationTime.CompareTo(this.ExpiryTime)) >= 0)
                this.ExpiryTime = this.CreationTime;

            this.SetTimeDifference(new TimeDifference());
        }

        public KeyValidityTime ToLocalTime()
        {
            return new KeyValidityTime(this.CreationTime.ToLocalTime(), this.ExpiryTime.ToLocalTime());
        }

        public KeyValidityTime(DateTime creationTime, DateTime expiryTime = new DateTime())
        {
            this.CreationTime = creationTime;
            this.ExpiryTime = expiryTime;
        }

        public KeyValidityTime(DateTime creationTime, TimeDifference timeDifference)
        {
            this.CreationTime = creationTime;
            this.ExpiryTime = this.CreationTime;
            this.SetTimeDifference(timeDifference);
        }
    }
}