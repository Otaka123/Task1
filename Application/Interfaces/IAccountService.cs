using Application.Common;
using Domain.DTOS.User.Request;
using Domain.DTOS.User.Response;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAccountService
    {

        Task<SignInResult> LoginAsync(string login, string password, bool rememberMe);
        Task<RequestResponse<string>> SignInAsync(LoginRequest request, CancellationToken cancellationToken = default);


        Task<RequestResponse> SignOutAsync(CancellationToken cancellationToken = default);
        //Task<RequestResponse<UserProfileResponse>> RegisterUser(RegisterRequest dto, CancellationToken cancellationToken = default);

    }
}
