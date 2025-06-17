using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class PasswordValidatorAttribute : ValidationAttribute
{
    public PasswordValidatorAttribute()
    {
        ErrorMessage = "Некорректный пароль";
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var password = value as string ?? "";

        if (string.IsNullOrWhiteSpace(password))
            return new ValidationResult("Введите пароль");

        if (password.Length < 6 || password.Length > 32)
            return new ValidationResult("Пароль должен содержать от 6 до 32 символов");

        if (!Regex.IsMatch(password, @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[\W_]).+$"))
            return new ValidationResult("Пароль должен содержать букву, цифру и специальный символ");

        return ValidationResult.Success!;
    }
}
