using System.ComponentModel.DataAnnotations;

namespace asp_gather_match.Contracts.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Email 為必填。")]
    [EmailAddress(ErrorMessage = "Email 格式不正確。")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "密碼為必填。")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "顯示名稱為必填。")]
    [StringLength(50, ErrorMessage = "顯示名稱不可超過 50 個字。")]
    public string DisplayName { get; init; } = string.Empty;
}
