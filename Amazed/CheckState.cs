namespace DreamAmazon
{
    public abstract class CheckState
    {
        protected readonly StateContext Context;

        protected CheckState(StateContext context)
        {
            Context = context;
        }

        public abstract void Handle(NetHelper nHelper);
    }
}