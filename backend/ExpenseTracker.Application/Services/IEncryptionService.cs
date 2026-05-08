using ExpenseTracker.Application.Enums;

namespace ExpenseTracker.Application.Services;

public interface IEncryptionService
{
    byte[] Encrypt(byte[] data, CryptoPurpose purpose);
    byte[] Decrypt(byte[] encryptedData, CryptoPurpose purpose);
}
