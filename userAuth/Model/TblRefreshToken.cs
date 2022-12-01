using System.ComponentModel.DataAnnotations;

namespace userAuth.Model
{
    public class TblRefreshToken
    {
        [Key]
        public string UserId { get; set; }
        public string TokenId { get; set; }
        public string RefreshToken { get; set; }
        public bool? IsActive { get; set; }
    }
}
