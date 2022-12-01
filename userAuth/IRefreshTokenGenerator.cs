namespace userAuth
{
    public interface IRefreshTokenGenerator
    {
        string GenerateToken(string Role);
    }
}
