namespace BlazorApp1.Model
{
    public class UserSession 
    { 
        public string UserId { get; set; } 
        public string Role { get; set; } 
    }

    public class UserConfig 
    { 
        public string UserId { get; set; } 
        public string Password { get; set; } 
        public string Role { get; set; } 
    }
}
