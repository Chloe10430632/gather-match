using System.ComponentModel.DataAnnotations;

namespace asp_gather_match.Contracts.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "Email 為必填。")]
    [EmailAddress(ErrorMessage = "Email 格式不正確。")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "密碼為必填。")]
    public string Password { get; init; } = string.Empty;
}
