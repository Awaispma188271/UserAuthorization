namespace userAuth.Model.ViewModels
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string? Student_Name { get; set; }
        public string? Father_Name { get; set; }
        public string? Gender { get; set; }
        public string? DOB { get; set; }
        public string? CNIC { get; set; }
        public string? Contact_1 { get; set; }
        public string? Contact_2 { get; set; }
        public string? Registration_No { get; set; }
        public string? Email { get; set; }
        public string? District { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public bool IsStudent { get; set; }
    }
}
