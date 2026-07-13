using System.Security.Cryptography;
using System.Text;

namespace LOS.Application.CreditScore;

public class CreditScoreService : ICreditScoreProvider
{
    public CreditScoreResult GetScore(string pan, DateOnly dateOfBirth)
    {
        var input = $"{pan.Trim().ToUpperInvariant()}|{dateOfBirth:yyyyMMdd}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var hashInt = BitConverter.ToUInt32(hashBytes, 0);
        var score = 300 + (int)(hashInt % 601);

        var category = score switch
        {
            >= 750 => "Low",
            >= 550 => "Medium",
            _ => "High",
        };

        return new CreditScoreResult(score, category, IsSimulated: true);
    }
}
