using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;

namespace website.Models
{
  // Add profile data for application users by adding properties to the ApplicationUser class
  public class ApplicationUserManager : UserManager<ApplicationUser>
  {
    public ApplicationUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger, IHttpContextAccessor contextAccessor)
      : base(store, optionsAccessor, new UpgradePasswordHasher(passwordHasher), userValidators, passwordValidators, keyNormalizer, errors, services, logger, contextAccessor)
    {

    }
  }

  public class UpgradePasswordHasher : IPasswordHasher<ApplicationUser>
  {
    readonly IPasswordHasher<ApplicationUser> _defaultHasher;

    public UpgradePasswordHasher(IPasswordHasher<ApplicationUser> defaultHasher)
    {
      _defaultHasher = defaultHasher;
    }

    public string HashPassword(ApplicationUser user, string password)
    {
      return _defaultHasher.HashPassword(user, password);
    }

    public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
    {
      var split = hashedPassword.Split('|');
      if (split.Length == 1)
      {
        return _defaultHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
      }

      string hash = split[0];
      string salt = split[1];
      if (string.Equals(SqlEncryptPassword(providedPassword, 1, salt), hash, StringComparison.OrdinalIgnoreCase))
      {
        return PasswordVerificationResult.SuccessRehashNeeded;
      }
      else
      {
        return PasswordVerificationResult.Failed;
      }
    }

    // This is copied from the existing SQL providers and is provided only for back-compat.
    private string SqlEncryptPassword(string pass, int passwordFormat, string salt)
    {
      if (passwordFormat == 0) // MembershipPasswordFormat.Clear
        return pass;

      byte[] bIn = Encoding.Unicode.GetBytes(pass);
      byte[] bSalt = Convert.FromBase64String(salt);
      byte[] bRet = null;

      if (passwordFormat == 1)
      {
        // MembershipPasswordFormat.Hashed 
        HashAlgorithm hm = HashAlgorithm.Create("SHA1");
        if (hm is KeyedHashAlgorithm)
        {
          KeyedHashAlgorithm kha = (KeyedHashAlgorithm)hm;
          if (kha.Key.Length == bSalt.Length)
          {
            kha.Key = bSalt;
          }
          else if (kha.Key.Length < bSalt.Length)
          {
            byte[] bKey = new byte[kha.Key.Length];
            Buffer.BlockCopy(bSalt, 0, bKey, 0, bKey.Length);
            kha.Key = bKey;
          }
          else
          {
            byte[] bKey = new byte[kha.Key.Length];
            for (int iter = 0; iter < bKey.Length;)
            {
              int len = Math.Min(bSalt.Length, bKey.Length - iter);
              Buffer.BlockCopy(bSalt, 0, bKey, iter, len);
              iter += len;
            }
            kha.Key = bKey;
          }
          bRet = kha.ComputeHash(bIn);
        }
        else
        {
          byte[] bAll = new byte[bSalt.Length + bIn.Length];
          Buffer.BlockCopy(bSalt, 0, bAll, 0, bSalt.Length);
          Buffer.BlockCopy(bIn, 0, bAll, bSalt.Length, bIn.Length);
          bRet = hm.ComputeHash(bAll);
        }
      }

      return Convert.ToBase64String(bRet);
    }
  }
}
