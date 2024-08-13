namespace DNDocs.Domain.ValueTypes
{
    public class OAuthAccessTokenDto
    {
        public int UserId { get; set; }
        public string AccessToken { get; set; }
        public string Scope { get; set; }
        public string TokenType { get; set; }
        public DateTime CreatedOn { get; set; }

        public OAuthAccessTokenDto(string accessToken,
            string scope,
            string tokenType)
        {
            AccessToken = accessToken;
            Scope = scope;
            TokenType = tokenType;
        }
    }
}
