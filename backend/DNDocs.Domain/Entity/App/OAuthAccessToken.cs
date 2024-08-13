namespace DNDocs.Domain.Entity.App
{
    public class OAuthAccessToken : Entity
    {
        public int UserId { get; set; }
        public string AccessToken { get; set; }
        public string Scope { get; set; }
        public string TokenType { get; set; }
        public DateTime CreatedOn { get; set; }

        public OAuthAccessToken(int userId,
            string accessToken,
            string scope,
            string tokenType)
        {
            CreatedOn = DateTime.UtcNow;
            UserId = userId;
            AccessToken = accessToken;
            Scope = scope;
            TokenType = tokenType;
        }
    }
}
