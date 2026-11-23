using System.ComponentModel.DataAnnotations;

namespace FabricX.Application.DTOs.Auth;

public class LoginDto
{
    [Required, StringLength(200)]
    public string? Email { get; set; }

    [Required, StringLength(100)]
    public string? Senha { get; set; }
}
