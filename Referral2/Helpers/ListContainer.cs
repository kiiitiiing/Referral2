using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Helpers
{
    public partial class ListContainer
    {
        public static List<KeyValuePair<string, string>> PhicStatus
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("none","None"),
                    new KeyValuePair<string, string>("member","Member"),
                    new KeyValuePair<string, string>("dependent","Dependent")
                };
            }
        }

        public static List<KeyValuePair<string, string>> IncomingStatus
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("referred","New Referral"),
                    new KeyValuePair<string, string>("accepted","Accepted")
                };
            }
        }

        public static List<KeyValuePair<string, string>> CivilStatus 
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("single","Single"),
                    new KeyValuePair<string, string>("married","Married"),
                    new KeyValuePair<string, string>("divorced","Divorced"),
                    new KeyValuePair<string, string>("separated","Separated"),
                    new KeyValuePair<string, string>("widowed","Widowed")
                };
            }
        }

        public static List<KeyValuePair<string, string>> Sex
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("male","Male"),
                    new KeyValuePair<string, string>("female","Female")
                };
            }
        }

        public static List<KeyValuePair<int, string>> Status
        {
            get
            {
                return new List<KeyValuePair<int, string>>
                {
                    new KeyValuePair<int, string>(1,"Active"),
                    new KeyValuePair<int, string>(0,"Inactive")
                };
            }
        }

        public static List<KeyValuePair<string, string>> UserStatus
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("active","Active"),
                    new KeyValuePair<string, string>("inactive","Inactive")
                };
            }
        }

        public static List<KeyValuePair<int, string>> HospitalLevel
        {
            get
            {
                return new List<KeyValuePair<int, string>>()
                {
                    new KeyValuePair<int, string>(1, "Level 1"),
                    new KeyValuePair<int, string>(2, "Level 2"),
                    new KeyValuePair<int, string>(3, "Level 3")
                };
            }
        }

        public static List<KeyValuePair<string, string>> HospitalType
        {
            get
            {
                return new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("government", "Government"),
                    new KeyValuePair<string, string>("private", "Private")
                };
            }
        }

        public static List<KeyValuePair<string, string>> ListStatus
        {
            get
            {
                return new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("referred", "Referred"),
                    new KeyValuePair<string, string>("seen", "Seen"),
                    new KeyValuePair<string, string>("accepted", "Accepted"),
                    new KeyValuePair<string, string>("arrived", "Arrived"),
                    new KeyValuePair<string, string>("admitted", "Admitted"),
                    new KeyValuePair<string, string>("discharged", "Discharged"),
                    new KeyValuePair<string, string>("cancelled", "Cancelled")
                };
            }
        }
        public static string[] TranspoMode
        {
            get
            {
                return new string[]
                {
                    "Ambulance (Level 1)",
                    "Ambulance (Level 2)",
                    "Patient Transport Vehicle",
                    "Personal Transportation",
                    "Others..."
                };
            }
        }
    }
}
