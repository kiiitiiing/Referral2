using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Resources;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models;
using Referral2.Models.ViewModels;
using Referral2.Models.ViewModels.Admin;
using Referral2.Models.ViewModels.Support;
using Microsoft.Extensions.Options;

namespace Referral2.Services
{
    public interface IUserService
    {
        Task<(bool, User)> ValidateUserCredentialsAsync(string username, string password);
        Task<(bool, User)> SwitchUserAsync(int id, string password);

        bool ChangePasswordAsync(User user, string newPassword);


        Task<bool> RegisterDoctorAsync(AddSupportViewModel model);

        Task<bool> RegisterSupportAsync(AddDoctorViewModel model, int facilityId);

    }
    public class UserService : IUserService
    {
        private readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        //private readonly ResourceManager Roles = new ResourceManager("Referral2.Roles", Assembly.GetExecutingAssembly());

        public PasswordHasher<User> _hashPassword = new PasswordHasher<User>();

        

        public UserService(ReferralDbContext context, IOptions<ReferralRoles> roles)
        {
            _context = context;
            _roles = roles;
        }

        public Task<(bool, User)> ValidateUserCredentialsAsync(string username, string password)
        {
            User user = null;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return Task.FromResult((false, user));

            try
            {
                user = _context.User.Single(x => x.Username.Equals(username));
            }catch(InvalidOperationException e)
            { }

            if (user == null)
                return Task.FromResult((false, user));

            var result = _hashPassword.VerifyHashedPassword(user, user.Password, password);


            if (result.Equals(PasswordVerificationResult.Success))
            {
                user.LoginStatus = "login";
                user.LastLogin = DateTime.Now;
                user.UpdatedAt = DateTime.Now;
                user.Status = "active";
                _context.Update(user);
                return Task.FromResult((true, user));
            }

            else
                return Task.FromResult((false, user));
        }

        public bool ChangePasswordAsync(User user, string newPassword)
        {
            var hashedPassword = _hashPassword.HashPassword(user, newPassword);

            user.Password = hashedPassword;
            user.UpdatedAt = DateTime.Now;

            _context.Update(user);
            _context.SaveChangesAsync();

            return true;
        }

        public Task<bool> RegisterDoctorAsync(AddSupportViewModel model)
        {
            if (_context.User.Any(x => x.Username.Equals(model.Username)))
            {
                return Task.FromResult(false);
            }
            else
            {
                var facility = _context.Facility.First(x => x.Id.Equals(model.FacilityId));
                User newUser = new User();
                string hashedPass = _hashPassword.HashPassword(newUser, model.Password);
                newUser.Firstname = model.Firstname;
                newUser.Middlename = model.Middlename;
                newUser.Lastname = model.Lastname;
                newUser.Contact = model.ContactNumber;
                newUser.Email = model.Email;
                newUser.FacilityId = model.FacilityId;
                newUser.Designation = model.Designation;
                newUser.Username = model.Username;
                newUser.Password = hashedPass;
                newUser.Level = _roles.Value.SUPPORT;
                newUser.DepartmentId = null;
                newUser.Title = null;
                newUser.BarangayId = facility.BarangayId;
                newUser.MuncityId = facility.MuncityId;
                newUser.ProvinceId = facility.ProvinceId;
                newUser.Designation = model.Designation;
                newUser.Status = "active";
                newUser.LastLogin = default;
                newUser.LoginStatus = "logout";
                newUser.CreatedAt = DateTime.Now;
                newUser.UpdatedAt = DateTime.Now;
                _context.Add(newUser);
                _context.SaveChanges();
                return Task.FromResult(true);
            }
        }

        public Task<bool> RegisterSupportAsync(AddDoctorViewModel model, int facilityId)
        {
            if (_context.User.Any(x => x.Username.Equals(model.Username)))
            {
                return Task.FromResult(false);
            }
            else
            {
                var facility = _context.Facility.First(x => x.Id.Equals(facilityId));
                User newUser = new User();
                string hashedPass = _hashPassword.HashPassword(newUser, model.Password);
                newUser.Firstname = GlobalFunctions.FixName(model.Firstname);
                newUser.Middlename = GlobalFunctions.FixName(model.Middlename);
                newUser.Lastname = GlobalFunctions.FixName(model.Lastname);
                newUser.Contact = model.ContactNumber;
                newUser.Email = model.Email;
                newUser.FacilityId = facilityId;
                newUser.Designation = model.Designation;
                newUser.Username = model.Username;
                newUser.Password = hashedPass;
                newUser.Level = model.Level;
                newUser.DepartmentId = model.Department;
                newUser.Title = null;
                newUser.BarangayId = facility.BarangayId;
                newUser.MuncityId = facility.MuncityId;
                newUser.ProvinceId = facility.ProvinceId;
                newUser.Designation = model.Designation;
                newUser.Status = "active";
                newUser.LastLogin = default;
                newUser.LoginStatus = "logout";
                newUser.CreatedAt = DateTime.Now;
                newUser.UpdatedAt = DateTime.Now;
                _context.Add(newUser);
                _context.SaveChanges();
                return Task.FromResult(true);
            }
        }

        Task<(bool, User)> IUserService.SwitchUserAsync(int id, string password)
        {
            var user = _context.User.Find(id);
            if (user == null)
            {
                return Task.FromResult((false, user));
            }

            var result = _hashPassword.VerifyHashedPassword(user, user.Password, password);


            if (result.Equals(PasswordVerificationResult.Success))
            {
                user.LoginStatus = "login";
                user.LastLogin = DateTime.Now;
                user.UpdatedAt = DateTime.Now;
                user.Status = "active";
                _context.Update(user);
                return Task.FromResult((true, user));
            }

            else
                return Task.FromResult((false, user));
        }
    }
}
