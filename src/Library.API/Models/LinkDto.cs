namespace Library.API.Models
{
    public class LinkDto
    {
        public LinkDto(string method, string href, string rel)
        {
            Method = method;
            Href = href;
            Rel = rel;
        }

        public string Href { get; private set; }
        public string Rel { get; private set; }
        public string Method { get; private set; }
    }
}
