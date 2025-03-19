using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace Informatics.Appetite.Models;

[Table("AppUser")]
public class AppUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("AppUserID")]
    public int Id { get; set; }

    [Required]
    [Column("Username")]
    [MaxLength(50)]
    public string? Username { get; set; }

    [Required]
    [Column("PasswordHash")]
    public byte[] PasswordHash { get; set; }

    [Required]
    [Column("Salt")]
    public byte[] Salt { get; set; } 

    public ICollection<UserIngredient> UserIngredients { get; set; } = new List<UserIngredient>();

    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    public void SetPassword(string password)
    {
        byte[] saltBytes = GenerateSalt();
        Salt = saltBytes;
        PasswordHash = HashPassword(password, saltBytes);
    }

    public bool VerifyPassword(string enteredPassword)
    {
        var hashedInput = HashPassword(enteredPassword, Salt);
        return StructuralComparisons.StructuralEqualityComparer.Equals(PasswordHash, hashedInput);
    }

    public static byte[] GenerateSalt()
    {
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }

    public static byte[] HashPassword(string password, byte[] salt)
    {
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
        {
            return pbkdf2.GetBytes(HashSize);
        }
    }
}
