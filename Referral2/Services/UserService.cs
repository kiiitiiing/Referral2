﻿using System;
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

namespace Referral2.Services
{
    public interface IUserService
    {
        Task<(bool, User)> ValidateUserCredentialsAsync(string Username, string Password);

        Task<bool> ChangePasswordAsync(User user, string newPassword);

        Task<bool> RegisterDoctorAsync(AddSupportViewModel model);

        Task<bool> RegisterSupportAsync(AddDoctorViewModel model, int facilityId);

    }
    public class UserService : IUserService
    {
        private readonly ReferralDbContext _context;
        private readonly ResourceManager Roles = new ResourceManager("Referral2.Roles", Assembly.GetExecutingAssembly());

        public PasswordHasher<User> _hashPassword = new PasswordHasher<User>();

        

        public UserService(ReferralDbContext context)
        {
            _context = context;
        }

        public Task<(bool, User)> ValidateUserCredentialsAsync(string Username, string Password)
        {
            User user = null;
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                return Task.FromResult((false, user));

            try
            {
                user = _context.User.Single(x => x.Username.Equals(Username));
            }catch(InvalidOperationException e)
            { }

            if (user == null)
                return Task.FromResult((false, user));

            var result = _hashPassword.VerifyHashedPassword(user, user.Password, Password);


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

        public Task<bool> ChangePasswordAsync(User user, string newPassword)
        {
            var hashedPassword = _hashPassword.HashPassword(user, newPassword);

            user.Password = hashedPassword;
            user.UpdatedAt = DateTime.Now;

            _context.Update(user);
            _context.SaveChangesAsync();

            return Task.FromResult(true);
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
                newUser.Level = Roles.GetString("SUPPORT");
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
                newUser.Firstname = model.Firstname;
                newUser.Middlename = model.Middlename;
                newUser.Lastname = model.Lastname;
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
    }
}
