using Referral2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace Referral2
{
    public partial class GlobalFunctions
    {
        public static string FullName;
        public static string FixName(string input)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }

        public static int ComputeAge(DateTime dob)
        {
            var today = DateTime.Today;

            var age = today.Year - dob.Year;

            if (dob.Date > today.AddYears(-age))
                age--;

            return age;
        }

        public static string GetDate(DateTime date, string format)
        {
            if (date != default)
            {
                if (!format.Contains("tt"))
                {
                    return date.ToString(format);
                }
                else
                    return date.ToString(format, CultureInfo.InvariantCulture);
            }
            else
                return "";
        }
        public static string GetAddress(Facility facility)
        {
            string address = string.IsNullOrEmpty(facility.Address) ? "" : facility.Address + ", ";
            string barangay = facility.Barangay == null ? "" : facility.Barangay.Description + ", ";
            string muncity = facility.Muncity == null ? "" : facility.Muncity.Description + ", ";
            string province = facility.Province == null ? "" : facility.Province.Description;

            return address + barangay + muncity + province;
        }

        public static string GetAddress(Patient patient)
        {
            string barangay = patient.Barangay == null ? "" : patient.Barangay.Description + ", ";
            string muncity = patient.Muncity == null ? "" : patient.Muncity.Description + ", ";
            string province = patient.Province == null ? "" : patient.Province.Description;

            return barangay + muncity + province;
        }

        public static string GetFullName(Patient patient)
        {
            if (patient != null)
                return CheckName(patient.FirstName) + " " + CheckName(patient.MiddleName) + " " + CheckName(patient.LastName);
            else
                return "";
        }

        public static string GetFullLastName(User user)
        {
            if (user != null)
                return CheckName(user.Lastname) + ", " + CheckName(user.Firstname) + " " + CheckName(user.Middlename);
            else
                return "";
        }
        public static string GetFullName(User user)
        {
            if (user != null)
                return CheckName(user.Firstname) + " " + CheckName(user.Middlename) + " " + CheckName(user.Lastname);
            else
                return "";
        }

        public static string GetMDFullName(User doctor)
        {
            if (doctor != null)
                FullName = "Dr. " + CheckName(doctor.Firstname) + " " + CheckName(doctor.Middlename) + " " + CheckName(doctor.Lastname);
            else
                FullName = "";

            return FullName;
        }

        public static string CheckName(string name)
        {
            return string.IsNullOrEmpty(name) ? "" : name;
        }
    }
}
