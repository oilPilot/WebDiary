using System.ComponentModel.DataAnnotations;

namespace WebDiary.Model;

public class VerifyMasterPasswordModel
{
    [Required]
    public string MasterPassword { get; set; } = "";
}

public class SetMasterPasswordModel
{
    [Required]
    public string MasterPassword { get; set; } = "";
}