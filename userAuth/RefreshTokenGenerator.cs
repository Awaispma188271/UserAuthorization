using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using userAuth.Model;

namespace userAuth
{
    public class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        private readonly UserContext _context;

        public RefreshTokenGenerator(UserContext context)
        {
            _context = context;
        }
        public string GenerateToken(string Role)
        {
            var randomnumber = new byte[32];
            using (var randomnumbergenerator = RandomNumberGenerator.Create())
            {
                randomnumbergenerator.GetBytes(randomnumber);
                string RefreshToken = Convert.ToBase64String(randomnumber);
                var _user = _context.tblRefreshTokens.FirstOrDefault(o => o.UserId == Role);
                if (_user != null)
                {
                    _user.RefreshToken = RefreshToken;
                    _context.SaveChanges();
                }
                else
                {
                    var tblRefreshtoken = new TblRefreshToken()
                    {
                        UserId = Role,
                        TokenId = new Random().Next().ToString(),
                        RefreshToken = RefreshToken,
                        IsActive = true
                    };
                    _context.tblRefreshTokens.Add(tblRefreshtoken);
                    _context.SaveChanges();
                }
                return RefreshToken;
            }
        }
    }
}
