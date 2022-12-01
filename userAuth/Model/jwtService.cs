using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using userAuth.Model.ViewModels;

namespace userAuth.Model
{
    public class jwtService
    {
        public string SecretKey { get; set; }
        public long TokenDuration { get; set; }

        private readonly IConfiguration _config;
        private readonly IRefreshTokenGenerator tokenGenerator;
        public jwtService(IConfiguration config , IRefreshTokenGenerator _refreshToken)
        {
            _config = config;
            tokenGenerator = _refreshToken;

            this.SecretKey = config.GetSection("jwtConfig").GetSection("Key").Value;
          
            this.TokenDuration = int.Parse(config.GetSection("jwtConfig").GetSection("Duration").Value);
        }
        public TokenResponse GenerateToken(string id,string UserName,string Email,string Role, int? roleId, bool IsAppoved)
           
        {
            TokenResponse tokenResponse = new TokenResponse();
            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.SecretKey));
            var signature = new SigningCredentials(Key,SecurityAlgorithms.HmacSha256);
            var payLoad = new[]
            {
                new Claim("id",id),
                new Claim("UserName",UserName),
                new Claim("Email",Email),
                new Claim("Role",Role),
                new Claim("RoleId", roleId != null? roleId.ToString(): ""),
                new Claim("ApproveUser",IsAppoved !=null ? IsAppoved.ToString(): ""),
            };
            var jwtToken = new JwtSecurityToken(
                issuer : "localhost",
                audience: "localhost",
                claims: payLoad,
                expires: DateTime.Now.AddMinutes(TokenDuration),

                signingCredentials: signature


                );
             tokenResponse.JWTToken= new JwtSecurityTokenHandler().WriteToken(jwtToken);
            tokenResponse.RefreshToken = tokenGenerator.GenerateToken(Role);

            return tokenResponse;
        }
    }
    
}
