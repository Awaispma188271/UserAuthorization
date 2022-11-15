using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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
        public readonly UserContext _context;
        public UserController(IConfiguration config, UserContext context)
        {
            _config = config;
            _context = context;
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
                var userExists = await _context.Registers.Where(u => u.Email == user.Email).FirstOrDefaultAsync();
                if (userExists != null)
                    return Ok("AlreadyExist");

                var dbUser = new Model.RegisterUser()
                {
                    Email = user.Email,
                    CNIC = user.CNIC,
                    Contact_1 = user.Contact_1,
                    Contact_2 = user.Contact_2,
                    District = user.District,
                    DOB = user.DOB,
                    Father_Name = user.Father_Name,
                    Gender = user.Gender,
                    Student_Name = user.Student_Name,
                    //copy other properties

                    Password = user.Password,

                };
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
            var userExists = await _context.Registers.Where(u => u.Email == user.Email && u.Password == user.Password && u.IsApproved).FirstOrDefaultAsync();
            if (userExists != null)
            {
                var userRole = _context.UserRoles.FirstOrDefault(p => p.UserId == userExists.UserId);
                //var userAccept = _context.Registers.FirstOrDefault(p => p. == userExists.UserId);

                return Ok(new jwtService(_config).GenerateToken(
                    userExists.UserId.ToString(),
                    userExists.Student_Name,
                    userExists.Email,

                     userRole != null ? userRole.RoleId : null,
                     userExists.IsApproved

                    )
                   );

            }

            return NotFound();

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
    }





}






