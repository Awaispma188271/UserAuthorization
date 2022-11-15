using System.ComponentModel.DataAnnotations;

namespace userAuth.Model
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        public string RollName { get; set; }
    }
}
