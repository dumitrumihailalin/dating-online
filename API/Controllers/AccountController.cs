using API.Data;
using API.DTO;
using API.Entities;
using API.Intefaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class AccountController : BaseAPIController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }
        
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {

            if (await UserExists(registerDto.Username))
            {
                return BadRequest("Username is taken");
            }

            var key = Encoding.UTF8.GetBytes(registerDto.Password.ToUpper());

            using var hmac = new HMACSHA512(key);
            
            var user = new AppUser
            {
                Username = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password.ToUpper())),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return new UserDto
            {
                Username = user.Username,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Username == loginDto.Username);

            if (user == null) return Unauthorized("Invalid username");

            var key = Encoding.UTF8.GetBytes(loginDto.Password.ToUpper());
            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password.ToUpper()));

            for(int i = 0; i< computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                {
                    return Unauthorized("Invalid Password");
                }
            }

            return new UserDto
            {
                Username = user.Username,
                Token = _tokenService.CreateToken(user),
            };
        }

        private async Task<bool> UserExists(string Username)
        { 
            return await _context.Users.AnyAsync(x => x.Username == Username.ToLower());
        }
    }
}
