using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using ETicaretAPI.Application.Abstractions.Services;
using ETicaretAPI.Application.Abstractions.Token;
using ETicaretAPI.Application.DTOs;
using ETicaretAPI.Application.Exceptions;
using ETicaretAPI.Application.Features.Commands.AppUser.LoginUser;
using ETicaretAPI.Domain.Entities.Identity;
using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ETicaretAPI.Persistence.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<Domain.Entities.Identity.AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenHandler _tokenHandler;
        private readonly SignInManager<Domain.Entities.Identity.AppUser> _signInManager;
        private readonly IUserService _userService;
        public AuthService(UserManager<AppUser> userManager, IConfiguration configuration, ITokenHandler tokenHandler, SignInManager<AppUser> signInManager, IUserService userService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _tokenHandler = tokenHandler;
            _signInManager = signInManager;
            _userService = userService;
        }

        public async Task<Token> GoogleLoginAsync(string idToken, int accessTokenLifeTime)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { _configuration["ExternalLoginSettings:Google:Client_ID"] }
            };
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            var info = new UserLoginInfo("GOOGLE", payload.Subject, "GOOGLE");
            Domain.Entities.Identity.AppUser user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            bool result = user != null;
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(payload.Email);
                if (user == null)
                {
                    user = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = payload.Email,
                        UserName = payload.Email,
                        NameSurname = payload.Name
                    };
                    var identityResult = await _userManager.CreateAsync(user);
                    result = identityResult.Succeeded;
                }
            }

            if (result)
            {
                await _userManager.AddLoginAsync(user, info);
                Token token = _tokenHandler.CreateAccessToken(accessTokenLifeTime);
                await _userService.UpdateRefreshToken(token.RefreshToken, user, token.Expiration, 15);
                return token;
            }
            throw new Exception("Invalid external authentication.");
        }


        public async Task<Token> LoginAsync(string userNameOrEmail, string password, int accessTokenLifeTime)
        {
            Domain.Entities.Identity.AppUser user = await _userManager.FindByNameAsync(userNameOrEmail);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(userNameOrEmail);
            }
            if (user == null)
            {
                throw new UserNotFoundException();
            }
            SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (result.Succeeded)
            {
                Token token = _tokenHandler.CreateAccessToken(accessTokenLifeTime);
                await _userService.UpdateRefreshToken(token.RefreshToken, user, token.Expiration, 15);
                return token;
            }
            throw new AuthenticationFailedException();
        }

        public async Task<Token> RefreshTokenLoginAsync(string refreshToken)
        {
            AppUser? user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (user != null && user?.RefreshTokenEndDate > DateTime.UtcNow)
            {
                Token token = _tokenHandler.CreateAccessToken(15);
                await _userService.UpdateRefreshToken(token.RefreshToken, user, token.Expiration, 15);
                return token;
            }
            else
                throw new UserNotFoundException();
        }
    }
}
