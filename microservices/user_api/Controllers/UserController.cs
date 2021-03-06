using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

using dotnetapi.Services;
using dotnetapi.Entities;
using dotnetapi.Models.Users;
using dotnetapi.Helpers;

namespace dotnetapi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _AppSettings;

        public UserController(IUserService userService, IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _mapper = mapper;
            _AppSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]UserAuthenticateModel model)
        {
            var user = _userService.Authenticate(model);
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            // Issue Auth Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_AppSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            return Ok(new{Token = tokenString});
        }

        [AllowAnonymous]
        [HttpPost("new")]
        public IActionResult New([FromBody]UserCreateModel model)
        {
            try
            {
                var user = _mapper.Map<User>(model);
                string privateKey = _userService.Create(_mapper.Map<User>(model), model.Password, "User");
                return Ok(privateKey);
            }
            catch (AppException ex)
            {
                return BadRequest(new { Title = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Read()
        {
            var currentUserId = int.Parse(User.Identity.Name);
            var user =  _userService.Read(currentUserId);            
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost]
        public IActionResult Update([FromBody]UserUpdateModel model)
        {   
            model.Id = int.Parse(User.Identity.Name);

            try {
                _userService.Update(_mapper.Map<User>(model), model.Password);
                return Ok();
            }
            catch (AppException ex) {
                return BadRequest(new { Title = ex.Message });
            }
        }
    }
}
