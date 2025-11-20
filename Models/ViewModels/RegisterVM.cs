using Microsoft.AspNetCore.Mvc;

namespace FitLog.Models.ViewModels
{
    public class RegisterVM {
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public double HeightCm { get; set; }
    public double WeightKg { get; set; }

    }


}
