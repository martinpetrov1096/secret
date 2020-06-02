using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Collections.Generic;
using dotnetapi.Services;
using dotnetapi.Entities;
using dotnetapi.Models.Users;
using dotnetapi.Helpers;

namespace dotnetapi.Controllers 
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class LogController : ControllerBase
    {
        ILogService _logService;
        public LogController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public IActionResult Read() {
            int userId = int.Parse(User.Identity.Name);
            try {
                List<EventLog> logs = _logService.getLogs(userId);
                return Ok();
            }
            catch (AppException e) {
                return BadRequest(new {Title = e.Message});
            }
        }
    } 
}  