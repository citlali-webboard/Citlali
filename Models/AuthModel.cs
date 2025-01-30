namespace Citlali.Models;

public class AuthConfirmDto
{
        public string Email { get; set; } = "";
        public string Otp { get; set; } = "";
}

public class AuthLoginDto
{
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
}

public class AuthRegisterDto
{
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
}