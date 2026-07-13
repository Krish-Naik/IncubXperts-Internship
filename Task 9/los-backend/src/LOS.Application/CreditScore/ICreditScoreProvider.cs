namespace LOS.Application.CreditScore;

public record CreditScoreResult(int Score, string RiskCategory, bool IsSimulated);

public interface ICreditScoreProvider
{
    CreditScoreResult GetScore(string pan, DateOnly dateOfBirth);
}
