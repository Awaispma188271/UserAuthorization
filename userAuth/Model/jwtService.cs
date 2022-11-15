using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace userAuth.Model
{
    public class jwtService
    {
        public string SecretKey { get; set; }
        public long TokenDuration { get; set; }
        private readonly IConfiguration _config;
        public jwtService(IConfiguration config)
        {
            _config = config;
            this.SecretKey = config.GetSection("jwtConfig").GetSection("Key").Value;
          
            //this.TokenDuration = Int32.Parse(config.GetSection("jwtConfig").GetSection("Key").Value);
        }
        public string GenerateToken(string id,string UserName,string Email, int? roleId, bool IsAppoved)
           
        {
            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.SecretKey));
            var signature = new SigningCredentials(Key,SecurityAlgorithms.HmacSha256);
            var payLoad = new[]
            {
                new Claim("id",id),
                new Claim("UserName",UserName),
                new Claim("Email",Email),
               
                new Claim("RoleId", roleId != null? roleId.ToString(): ""),
                new Claim("ApproveUser",IsAppoved !=null ? IsAppoved.ToString(): ""),
            };
            var jwtToken = new JwtSecurityToken(
                issuer : "localhost",
                audience: "localhost",
                claims: payLoad,
              //  expires: DateTime.Now.AddMinutes(TokenDuration),

                signingCredentials: signature


                );
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
    }
    
}
