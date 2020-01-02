using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Referral2.Models;
using Referral2.Data;

namespace Referral2.Helpers
{
    public static class CurrentUser
    {
        private static User _currentUser;

        public static User user { get { return _currentUser; } set { _currentUser = value; } }
    }
}
