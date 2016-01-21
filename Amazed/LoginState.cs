namespace DreamAmazon
{
    public class LoginState : CheckState
    {
        private string _response;

        public LoginState(StateContext context) : base(context)
        {
        }

        public override void Init(string resposne)
        {
            _response = resposne;
        }

        public override void Handle(NetHelper nHelper)
        {
            string metadata = Context.MetadataFinder.QueryMetadata(Context.CheckParams.Account);

            Contracts.Require(metadata != null);

            if (Properties.Settings.Default.Mode == (int)SettingMode.DuoMode || Properties.Settings.Default.Mode == (int)SettingMode.ProxiesMode)
            {
                var proxy = Context.ProxyManager.GetProxy();

                if (proxy == null)
                {
                    //Context.State = null;
                    //throw new ApplicationException("proxy not found");
                }
                else
                {
                    nHelper.Proxy = proxy;
                }
            }

            // should we use _response here ??

            var loginResponse = nHelper.GET(Globals.LOG_URL);

            if (Context.IsError(loginResponse))
            {
                Context.Logger.Debug("error detected, finish state object:" + Context.CheckParams.Account.Email);
                Context.ProxyManager.RemoveProxy(nHelper.Proxy);
                Context.SetFinishState(CheckResults.Bad);
                return;
            }

            if (StateContext.IsRoboCheck(loginResponse.Value))
            {
                Context.Logger.Debug("robocheck detected:" + Context.CheckParams.Account.Email);
                Context.SetRoboState(loginResponse.Value);
                return;
            }

            var attributes = StateContext.ParseAccountAttributes(loginResponse.Value, Context.CheckParams.Account, metadata);

            var postResult = nHelper.POST(Globals.POST_URL, attributes);

            if (Context.IsError(postResult))
            {
                Context.SetFinishState(CheckResults.Bad);
                return;
            }

            Context.SetValidationState(postResult.Value);
        }
    }
}