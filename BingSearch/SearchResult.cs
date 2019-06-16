namespace BingSearch
{
    internal class SearchResult
    {
        public string Text { get; }
        public string Url { get; }
        public string Description { get; }

        public SearchResult(string text, string url, string description)
        {
            Text = text;
            Url = url;
            Description = description;
        }
    }
}