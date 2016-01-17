namespace DreamAmazon
{
    public class EmptyState : CheckState
    {
        private static EmptyState _instance;

        public static EmptyState Create()
        {
            if (_instance == null)
            {
                _instance = new EmptyState();
            }
            return _instance;
        }

        protected EmptyState() : base(null)
        {
        }

        public override void Handle(NetHelper nHelper)
        {
            
        }

        public override void Init(string resposne)
        {
            
        }
    }
}