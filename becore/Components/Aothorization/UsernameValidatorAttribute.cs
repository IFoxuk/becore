using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class UsernameValidatorAttribute : ValidationAttribute
{
    public UsernameValidatorAttribute()
    {
        ErrorMessage = "Некорректный логин";
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var username = value as string ?? "";

        if (string.IsNullOrWhiteSpace(username))
            return new ValidationResult("Укажите логин");

        if (username.Length < 3 || username.Length > 32)
            return new ValidationResult("Логин должен содержать от 3 до 32 символов");

        if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_\.]+$"))
            return new ValidationResult("Логин может содержать только латинские буквы, цифры, подчёркивания и точки");

        return ValidationResult.Success!;
    }
}
