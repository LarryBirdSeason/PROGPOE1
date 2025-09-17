namespace PROGPOE1.Models
{
    public class User
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "lecturer", "coordinator", "manager"
    }
}