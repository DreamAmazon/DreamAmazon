namespace DreamAmazon
{
    public class SelectFileResult
    {
        public static readonly SelectFileResult Empty = new SelectFileResult();

        public string FileName { get; }

        protected SelectFileResult()
        { }

        public SelectFileResult(string fileName)
        {
            Contracts.Require(!string.IsNullOrEmpty(fileName));

            FileName = fileName;
        }
    }
}