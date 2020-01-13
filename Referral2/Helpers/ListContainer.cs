using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Helpers
{
    public partial class ListContainer
    {
        public static List<string> PhicStatus
        {
            get
            {
                return new List<string>
                {
                    "None",
                    "Memeber",
                    "Dependent"
                };
            }
        }

        public static List<string> CivilStatus 
        {
            get
            {
                return new List<string>
                {
                    "Single",
                    "Married",
                    "Divorced",
                    "Separated",
                    "WIdowed"
                };
            }
        }

        public static List<string> Sex
        {
            get
            {
                return new List<string>
                {
                    "Male",
                    "Female"
                };
            }
        }

        public static List<string> Status
        {
            get
            {
                return new List<string>
                {
                    "active",
                    "inactive"
                };
            }
        }

        

        public static bool computeHours(DateTime time)
        {
            var hr = time.AddDays(3);

            if (hr == DateTime.Now)
                return true;
            else
                return false;
        }

    }
}
