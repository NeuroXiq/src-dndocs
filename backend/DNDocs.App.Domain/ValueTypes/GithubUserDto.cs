namespace DNDocs.Domain.ValueTypes
{
    public class GithubUserDto
    {
        public string PrimaryEmail { get; set; }
        public string Id { get; set; }
        public string Login { get; set; }
        public string ReposUrl { get; set; }
        public string Url { get; set; }
        public string HtmlUrl { get; set; }
        public string AvatarUrl { get; set; }
        public string Type { get; set; }

        public GithubUserDto(
            string primaryemail,
            string id,
            string login,
            string reposurl,
            string url,
            string htmlurl,
            string avatarurl,
            string type)
        {
            PrimaryEmail = primaryemail;
            Id = id;
            Login = login;
            ReposUrl = reposurl;
            HtmlUrl = htmlurl;
            AvatarUrl = avatarurl;
            Type = type;
        }
    }
}
