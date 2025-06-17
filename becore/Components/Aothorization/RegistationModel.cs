using System.ComponentModel.DataAnnotations;

namespace becore.Components.Aothorization
{

    public class RegistrationModel
    {
        [UsernameValidator]
        public string Username { get; set; } = "";
        [PasswordValidator]
        public string Password { get; set; } = "";
    }
}
