using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WebDiary.Frontend;

public class AuthOptions
{
    public const string issuer = "DiaryServer";
    public const string audience = "UserName?";
    const string key = "TestingSecretKeyTestingSecretKey2";
    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
}
