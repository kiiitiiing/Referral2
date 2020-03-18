using Referral2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace Referral2
{
    public static class GlobalFunctions
    {
        private static string FullName;
        public static string FixName(this string input)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }

        public static string FirstToUpper(this string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Trim().ToLower();

                text = text.First().ToString().ToUpper() + text.Substring(1);

                return text;
            }
            else
                return "";
        }

        public static string NameToUpper(this string name)
        {
            if(!string.IsNullOrEmpty(name))
            {
                string[] names = name.Split(null);
                string fullname = "";
                foreach (var item in names)
                {
                    fullname += item.FirstToUpper() + " ";
                }
                return fullname;
            }
            else
            {
                return "";
            }
        }

        public static string RemoveParenthesis(this string name)
        {
            var stack = new Stack<char>();

            var par = false;

            foreach(var letter in name)
            {
                stack.Push(letter);
                if(letter == '(')
                {
                    par = true;
                    stack.Pop();
                }
                if(par)
                {
                    if(letter == ')')
                    {
                        par = false;
                        stack.Pop();
                    }
                    else
                    {
                        stack.Pop();
                    }
                }
            }
            var asa = string.Join<char>("", stack.Reverse());
            return asa;
        }

        public static string ComputeTimeFrame(this double minutes)
        {
            var min = Math.Floor(minutes);
            var minute = min == 0 ? "" : min + "m";
            var sec = Math.Round((minutes - min) * 60);
            var seconds = sec == 0 ? "" : sec + "s";
            var total = minute + " " + seconds;
            return total;
        }

        public static int ComputeAge(this DateTime dob)
        {
            var today = DateTime.Today;

            var age = today.Year - dob.Year;

            if (dob.Date > today.AddYears(-age))
                age--;

            return age;
        }

        public static int ComputeAge(this DateTime? dob)
        {
            var realDob = (DateTime)dob;
            var today = DateTime.Today;

            var age = today.Year - realDob.Year;

            if (realDob.Date > today.AddYears(-age))
                age--;

            return age;
        }

        public static double ArchivedTime(DateTime date)
        {
            return Convert.ToInt32(DateTime.Now.Subtract(date).TotalMinutes);
        }

        public static string GetDate(this DateTime date, string format)
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

        public static string GetDate(this DateTime? date, string format)
        {
            var realDate = (DateTime)date;

            if (realDate != default)
            {
                if (!format.Contains("tt"))
                {
                    return realDate.ToString(format);
                }
                else
                    return realDate.ToString(format, CultureInfo.InvariantCulture);
            }
            else
                return "";
        }


        public static string GetAddress(this Facility facility)
        {
            string address = string.IsNullOrEmpty(facility.Address) ? "" : facility.Address + ", ";
            string barangay = facility.BarangayId == null ? "" : facility.Barangay.Description + ", ";
            string muncity = facility.MuncityId == null ? "" : facility.Muncity.Description + ", ";
            string province = facility.Province == null ? "" : facility.Province.Description;

            return address + barangay + muncity + province;
        }

        public static string GetAddress( this Patient patient)
        {
            string barangay = patient.Barangay == null ? "" : patient.Barangay.Description + ", ";
            string muncity = patient.Muncity == null ? "" : patient.Muncity.Description + ", ";
            string province = patient.Province == null ? "" : patient.Province.Description;

            return barangay + muncity + province;
        }

        public static string GetFullName(this Patient patient)
        {
            if (patient != null)
                return patient.FirstName.CheckName() + " " + patient.MiddleName.CheckName() + " " + patient.LastName.CheckName();
            else
                return "";
        }

        public static string GetFullLastName(User user)
        {
            if (user != null)
                return user.Lastname.CheckName() + ", " + user.Firstname.CheckName() + " " + user.Middlename.CheckName();
            else
                return "";
        }
        public static string GetFullLastName(this Patient patient)
        {
            if (patient != null)
                return patient.LastName.CheckName() + ", " + patient.FirstName.CheckName() + " " + patient.MiddleName.CheckName();
            else
                return "";
        }
        public static string GetFullName(this User user)
        {
            if (user != null)
                return user.Firstname.CheckName() + " " + user.Middlename.CheckName() + " " + user.Lastname.CheckName();
            else
                return "";
        }

        public static string GetMDFullName(this User doctor)
        {
            if (doctor != null)
                FullName = "Dr. " + doctor.Firstname.CheckName() + " " + doctor.Middlename.CheckName() + " " + doctor.Lastname.CheckName();
            else
                FullName = "";

            return FullName;
        }

        public static string CheckName(this string name)
        {
            return string.IsNullOrEmpty(name) ? "" : name;
        }
    }
}
