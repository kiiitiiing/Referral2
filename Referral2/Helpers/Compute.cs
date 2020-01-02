using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Helpers
{
    public interface ICompute
    {
        public int GetAge(DateTime dateOfBirth);
    }
    public class Compute : ICompute
    {
        public int GetAge(DateTime dateOfBirth)
        {
            int age = DateTime.Now.Year - dateOfBirth.Year;

            return age;
        }
    }
}
