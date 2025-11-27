using Application.Common;
using Application.Interfaces;
using AutoMapper;
using Domain.DTOS.User.Request;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Account
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<Admin> _userManager;
        private readonly SignInManager<Admin> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AccountService> _logger;
        private readonly IAppLocalizer _localizer;
        private readonly IMapper _mapper;
        private const string DefaultRole = "User";

        public AccountService(
            UserManager<Admin> userManager,
            SignInManager<Admin> signInManager,
            RoleManager<IdentityRole> roleManager,

            ICurrentUserService currentUserService,
            ILogger<AccountService> logger,
           IAppLocalizer localizer,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;

            _currentUserService = currentUserService;
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<SignInResult> LoginAsync(string login, string password, bool rememberMe)
        {
            var existingUser = await _userManager.Users
                       .FirstOrDefaultAsync(u =>
                           u.Email == login || u.UserName == login || u.PhoneNumber == login);

            if (existingUser == null)
            {
                return SignInResult.Failed;
            }

            var result = await _signInManager.PasswordSignInAsync(existingUser, password, rememberMe, lockoutOnFailure: true);
            return result;
        }
        public async Task<RequestResponse<string>> SignInAsync(
  LoginRequest request,
  CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var user = await FindUserAsync(request.login, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("Login failed for identifier: {Identifier} - User not found or inactive",
                        MaskSensitiveData(request.login));
                    return RequestResponse<string>.Unauthorized(_localizer["InvalidCredentials"]);
                }

                var result = await _signInManager.CheckPasswordSignInAsync(
                    user,
                    request.Password,
                    lockoutOnFailure: true
                );

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Account locked out for user: {UserId}", user.Id);
                    return RequestResponse<string>.Locked(_localizer["AccountLocked"]);
                }

                if (result.Succeeded)
                {
                    // تحديث وقت آخر تسجيل دخول قبل SignIn
                    user.LastLoginDate = DateTime.Now;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to update last login date for user: {UserId}", user.Id);
                        // يمكنك اختيار ما إذا كنت تريد منع Login في حالة فشل التحديث
                        // return RequestResponse<string>.Fail("Failed to update last login date");
                    }

                    await _signInManager.SignInAsync(user, isPersistent: request.RememberMe);

                    _logger.LogInformation("User {UserId} logged in successfully", user.Id);
                    return RequestResponse<string>.Ok(
                        data: user.Id,
                        message: _localizer["LoginSuccessful"]
                    );
                }
                else
                {
                    _logger.LogWarning("Invalid password for user: {UserId}", user.Id);
                    return RequestResponse<string>.Unauthorized(_localizer["InvalidCredentials"]);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Login operation was cancelled for identifier: {Identifier}",
                    MaskSensitiveData(request.login));
                return RequestResponse<string>.Fail("Operation was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for identifier: {Identifier}",
                    MaskSensitiveData(request.login));
                return RequestResponse<string>.InternalServerError(_localizer["SystemError"]);
            }
        }

        public async Task<RequestResponse> SignOutAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_currentUserService.IsAuthenticated || _currentUserService.UserId == null)
                {
                    _logger.LogWarning("Sign-out failed: no authenticated user found.");
                    return RequestResponse.Unauthorized(_localizer["UserNotAuthenticated"]);
                }

                var userId = _currentUserService.UserId;

                cancellationToken.ThrowIfCancellationRequested();

                //var refreshTokenRemoved = await _tokenService.RevokeRefreshTokenAsync(userId, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                await _signInManager.SignOutAsync();

                _logger.LogInformation("User {UserId} signed out successfully", userId);
                return RequestResponse.Ok(message: _localizer["SignOutSuccessful"]);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Sign-out operation was cancelled.");
                return RequestResponse.Fail("Operation was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sign-out.");
                return RequestResponse.InternalServerError(_localizer["SystemError"]);
            }
        }
        //public async Task<RequestResponse<UserRegistrationResponse>> RegisterUser(
        //  RegisterRequest dto,
        //  CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        cancellationToken.ThrowIfCancellationRequested();

        //        if (await _userManager.FindByEmailAsync(dto.Email) != null)
        //        {
        //            return RequestResponse<UserRegistrationResponse>.BadRequest(_localizer["EmailAlreadyExists"]);
        //        }

        //        //var validationResult = await _validationService.ValidateAsync(dto);
        //        //if (!validationResult.IsValid)
        //        //{
        //        //    return RequestResponse<UserRegistrationResponse>.BadRequest(
        //        //        errors: validationResult.Errors.Select(e => e.ErrorMessage).ToList()
        //        //    );
        //        //}

        //        cancellationToken.ThrowIfCancellationRequested();

        //        var user = _mapper.Map<Admin>(dto);

        //        if (string.IsNullOrEmpty(user.ProfilePictureUrl))
        //        {
        //            user.ProfilePictureUrl = GetDefaultProfilePictureUrl(user.Gender);
        //        }

        //        var result = await _userManager.CreateAsync(user, dto.Password);
        //        cancellationToken.ThrowIfCancellationRequested();

        //        if (!result.Succeeded)
        //        {
        //            return RequestResponse<UserRegistrationResponse>.Fail(
        //                "Failed to create user",
        //                result.Errors.Select(e => e.Description).ToList()
        //            );
        //        }

        //        if (result.Succeeded && await _roleManager.RoleExistsAsync(DefaultRole.Trim()))
        //        {
        //            await _userManager.AddToRoleAsync(user, DefaultRole);
        //        }

        //        return RequestResponse<UserRegistrationResponse>.Ok(
        //            new UserRegistrationResponse(user.Id),
        //            "User registered successfully"
        //        );
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        _logger.LogInformation("Registration operation was cancelled for user: {FirstName}", dto.FirstName);
        //        return RequestResponse<UserRegistrationResponse>.Fail("Operation was cancelled");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Registration failed for {Email}", dto.Email);
        //        return RequestResponse<UserRegistrationResponse>.InternalServerError();
        //    }
        //}


        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        private async Task<Admin> FindUserAsync(string identifier, CancellationToken cancellationToken)
        {
            return await _userManager.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == identifier ||
                    u.UserName == identifier ||
                    u.PhoneNumber == identifier);
        }

        private async Task<bool> SetLastLoginDate(Admin user)
        {
            try
            {
                user.LastLoginDate = DateTime.UtcNow;
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update last login date for user: {UserId}", user.Id);
                return false;
            }
        }

        private string MaskSensitiveData(string data)
        {
            if (string.IsNullOrEmpty(data) || data.Length <= 2)
                return "***";

            return data[..2] + new string('*', data.Length - 2);
        }
    }
}

