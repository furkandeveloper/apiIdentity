using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebLayer.Identity;
using WebLayer.Models;

namespace WebLayer.Controllers
{
    [Route("v1/")]
    [ApiController]
    public class DefaultController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<ApplicationRole> _roleManager;

        public DefaultController(UserManager<ApplicationUser> userManager , RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        [Route("User")]
        public async Task<ActionResult> UserCreate([FromBody] UserCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser()
                {
                    UserName = model.UserName.Trim(),
                    Email = model.Email.Trim()
                };
                IdentityResult result = await _userManager.CreateAsync(user, model.Password.Trim());
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync("User").Result)
                    {
                        ApplicationRole role = new ApplicationRole()
                        {
                            Name="User"
                        };
                        IdentityResult roleResult = await _roleManager.CreateAsync(role);
                        if (roleResult.Succeeded)
                        {
                            _userManager.AddToRoleAsync(user, "User").Wait();
                        }
                    }
                    _userManager.AddToRoleAsync(user, "User").Wait();
                    return Ok();
                }
                String errorMessage = String.Empty;
                foreach (var item in result.Errors)
                {
                    errorMessage += item.Description;
                }
                return BadRequest(errorMessage);
            }
            return BadRequest("Bilgilerinizi kontrol ediniz. İstenilen formatta değil");
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult> Login([FromBody]LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByNameAsync(model.UserName.Trim());
                if (user!=null && await _userManager.CheckPasswordAsync(user,model.Password.Trim()))
                {
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub,user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                    };

                    var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySuperSecureKey"));

                    var token = new JwtSecurityToken(
                        issuer:"http://google.com",
                        audience:"http://google.com",
                        expires:DateTime.UtcNow.AddHours(1),
                        claims:claims,
                        signingCredentials:new Microsoft.IdentityModel.Tokens.SigningCredentials(signinKey,SecurityAlgorithms.HmacSha256)
                        );
                    return Ok(new {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
                }
                else
                {
                    return BadRequest("Giriş bilgilerinizi kontrol edin hatalı gözüküyor.");
                }
            }
            return BadRequest("Veriler uygun değil");
        }
    }
}