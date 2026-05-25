using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using LaptopAZ.DTO;
using LaptopAZ.Helpers;
using LaptopAZ.Models;
using LaptopAZ.Repository;

namespace LaptopAZ.BLL
{
    public class AuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            var user = _unitOfWork.Users.Query()
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.IsActive);

            if (user == null)
                return false;

            // Verify password using PasswordHelper which supports BCrypt and the fallback seeds!
            if (PasswordHelper.VerifyPassword(password, user.PasswordHash))
            {
                // Set Session
                SessionHelper.CurrentUserId = user.UserId;
                SessionHelper.CurrentUsername = user.Username;
                SessionHelper.CurrentFullName = user.FullName;
                SessionHelper.CurrentRole = user.Role?.RoleName ?? "SalesStaff";
                SessionHelper.CurrentEmail = user.Email;

                // Log login history to file
                try
                {
                    string logLine = $"{DateTime.Now:dd/MM/yyyy HH:mm:ss} | {user.Username} | {user.FullName} | {SessionHelper.CurrentRole}";
                    System.IO.File.AppendAllLines("login_history.txt", new[] { logLine });
                }
                catch { }

                return true;
            }

            return false;
        }

        public void Logout()
        {
            SessionHelper.Clear();
        }

        public List<UserDTO> GetAllUsers()
        {
            return _unitOfWork.Users.Query()
                .Include(u => u.Role)
                .OrderBy(u => u.RoleId)
                .ThenBy(u => u.FullName)
                .Select(u => new UserDTO
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    Email = u.Email,
                    RoleId = u.RoleId,
                    RoleName = u.Role.RoleName,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                }).ToList();
        }

        public bool CreateUser(string username, string plainPassword, string fullName, string phone, string email, int roleId)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(plainPassword) || string.IsNullOrWhiteSpace(fullName))
                return false;

            // Check if username already exists
            bool exists = _unitOfWork.Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (exists)
                return false;

            var user = new User
            {
                Username = username,
                PasswordHash = PasswordHelper.HashPassword(plainPassword),
                FullName = fullName,
                Phone = phone,
                Email = email,
                RoleId = roleId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _unitOfWork.Users.Add(user);
            return _unitOfWork.SaveChanges() > 0;
        }

        public bool UpdateUser(int userId, string fullName, string phone, string email, int roleId, bool isActive, string newPassword = null)
        {
            var user = _unitOfWork.Users.GetById(userId);
            if (user == null)
                return false;

            user.FullName = fullName;
            user.Phone = phone;
            user.Email = email;
            user.RoleId = roleId;
            user.IsActive = isActive;

            if (!string.IsNullOrWhiteSpace(newPassword) && newPassword != "********")
            {
                user.PasswordHash = PasswordHelper.HashPassword(newPassword);
            }

            _unitOfWork.Users.Update(user);
            return _unitOfWork.SaveChanges() > 0;
        }

        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var user = _unitOfWork.Users.GetById(userId);
            if (user == null)
                return false;

            if (!PasswordHelper.VerifyPassword(oldPassword, user.PasswordHash))
                return false;

            user.PasswordHash = PasswordHelper.HashPassword(newPassword);
            _unitOfWork.Users.Update(user);
            return _unitOfWork.SaveChanges() > 0;
        }

        public List<Role> GetRoles()
        {
            return _unitOfWork.Roles.GetAll().OrderBy(r => r.RoleId).ToList();
        }

        /// <summary>
        /// Soft-delete: vô hiệu hóa tài khoản nhân viên (đặt IsActive = false).
        /// </summary>
        public bool DeleteUser(int userId)
        {
            var user = _unitOfWork.Users.GetById(userId);
            if (user == null) return false;
            user.IsActive = false;
            _unitOfWork.Users.Update(user);
            return _unitOfWork.SaveChanges() > 0;
        }
    }
}
