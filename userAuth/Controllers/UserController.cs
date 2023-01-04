using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using userAuth.Enums;

using userAuth.Model;
using userAuth.Model.ViewModels;

namespace userAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly jwtService setting;
        public readonly UserContext _context;
        private readonly IRefreshTokenGenerator tokenGenerator;
        public UserController(IConfiguration config, UserContext context, IRefreshTokenGenerator _refreshToken)
        {
            _config = config;
            _context = context;
            
            tokenGenerator = _refreshToken;
        }

       

        [HttpGet("RegisterRole")]
        public async Task<IActionResult> GetRegisterRole()
        {
            return Ok(_context.Roles.ToList());
        }
       
        [AllowAnonymous]
        [HttpPost("createRegisterUser")]

        public async Task<IActionResult> CreateRegister(UserViewModel user)
        {
            var response = new SurveyResponse();
            try
            {
                var userExists = await _context.Registers.Where(u => u.PersonalEmail == user.PersonalEmail).FirstOrDefaultAsync();
                if (userExists != null)
                    return Ok("AlreadyExist");

                var dbUser = new Model.RegisterUser()
                {
                    PersonalEmail = user.PersonalEmail,
                    CNIC = user.CNIC,
                    Contact_1 = user.Contact_1,
                    Contact_2 = user.Contact_2,
                    District = user.District,
                    DOB = user.DOB,
                    Father_Name = user.Father_Name,
                    Gender = user.Gender,
                    Student_Name = user.Student_Name,
                   
                   // Role = user.Role,
                    //copy other properties

                    Password = user.Password,

                };
                if (user.IsStudent)
                {
                    dbUser.Role = "Student";
                }
                else
                {
                    dbUser.Role = "Employer";
                }
                await _context.Registers.AddAsync(dbUser);
                int countOfChanges = await _context.SaveChangesAsync();

                if (countOfChanges > 0)
                {
                    //user has been successfully created in database
                    int userId = dbUser.UserId;

                    //now, we need to add this user's role in database
                    if (user.IsStudent)
                    {
                        int stuentRoleId = (int)UserRoles.Student;
                        await _context.UserRoles.AddAsync(new UserRole()
                        {
                            
                            UserId = userId,
                            RoleId = stuentRoleId
                        });
                    }
                    else
                    {
                        var updateUser = new Model.RegisterUser();
                        await _context.UserRoles.AddAsync(new UserRole()
                        {
                            UserId = userId,
                            RoleId = (int)UserRoles.Employer
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                response.Success = true;
                return Ok(response);
            }
            catch (Exception exception)
            {
                response.Success = false;
                response.ErrorMessage = exception.Message;
                return Ok(response);
            }
        }

        [AllowAnonymous]
        [HttpGet("getUser")]
        public async Task<IActionResult> GetUser()
        {            
            await _context.SaveChangesAsync();
            return Ok(await _context.Registers.ToListAsync());
        }      
        

        [AllowAnonymous]
        [HttpPost("loginUser")]       
        public async Task<IActionResult> Login(Login user)
        {
            CookieOptions cookieOptions = new CookieOptions()
            {
               
            Expires = DateTimeOffset.UtcNow.AddDays(1),
            IsEssential = true, 
            HttpOnly = false,
            Secure = false,
        };
            var userExists = await _context.Registers.Where(u => u.PersonalEmail == user.Email && u.Password == user.Password && u.IsApproved).FirstOrDefaultAsync();
            if (userExists != null)
            {
                var userRole = _context.UserRoles.FirstOrDefault(p => p.UserId == userExists.UserId);                
                var finaltoken = (new jwtService(_config,tokenGenerator).GenerateToken(
                    userExists.UserId.ToString(),
                    userExists.Student_Name,
                    userExists.PersonalEmail,
                    userExists.Role,
                     userRole != null ? userRole.RoleId : null,
                     userExists.IsApproved                     
                    ));

                //var response = new HttpResponseMessage(HttpStatusCode.OK);
                

                HttpContext.Response.Cookies.Append("AccessToken", finaltoken.ToString(), cookieOptions);
                return Ok(finaltoken);

            }

            return NotFound();

        }
        [NonAction]
        public TokenResponse Authenticate(string Role, Claim[] claims)
        {
            TokenResponse tokenResponse = new TokenResponse();
            var tokenkey = Encoding.UTF8.GetBytes(_config["jwtConfig:Key"]);
            var tokenhandler = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddSeconds(15),
                 signingCredentials: new SigningCredentials(new SymmetricSecurityKey(tokenkey), SecurityAlgorithms.HmacSha256)

                );
            tokenResponse.JWTToken = new JwtSecurityTokenHandler().WriteToken(tokenhandler);
            tokenResponse.RefreshToken = tokenGenerator.GenerateToken(Role);

            return tokenResponse;
        }

        [AllowAnonymous]
        [HttpPost("Refresh")]
        public IActionResult Refresh([FromBody] TokenResponse token)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            //var key = Encoding.UTF8.GetBytes("jwtConfig:Key");
             var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token.JWTToken);
            //SecurityToken securityToken;

            var Role = securityToken.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;


           // var _token = securityToken as JwtSecurityToken;
            
           


            //var Role = principal.Identity.Name;
            var _reftable = _context.tblRefreshTokens.FirstOrDefault(o => o.UserId == Role ||  o.RefreshToken == token.RefreshToken);
            if(_reftable == null)
            {
                return Unauthorized();
            }
            TokenResponse _result = Authenticate(Role, securityToken.Claims.ToArray());
            return Ok(_result);
        }



        [AllowAnonymous]
        [HttpPost("studentAccept/{Id}")]

        public async Task<IActionResult> studentAccept([FromRoute] int Id)
        {
            var newUser = new RegisterUser();
            try
            {

                var newUser1 = await _context.Registers.FindAsync(Id);

                if (newUser1 == null)
                {
                    return NotFound();
                }
                newUser1.IsApproved = true;
                 _context.Registers.Update(newUser1);

                await _context.SaveChangesAsync();

                return Ok(newUser1);

            }


            catch (Exception)
            {

                throw;
            }
        }
        [AllowAnonymous]
        [HttpDelete("studentDelete/{Id}")]

        public async Task<IActionResult> studentDelete([FromRoute] int Id)
        {
            try
            {
                var newUser1 = await _context.Registers.FindAsync(Id);
                 _context.Registers.Remove(newUser1);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}






