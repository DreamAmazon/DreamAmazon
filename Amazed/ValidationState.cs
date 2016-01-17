namespace DreamAmazon
{
    public class ValidationState : CheckState
    {
        private string _response;

        public ValidationState(StateContext context) : base(context)
        {
        }

        public override void Init(string response)
        {
            _response = response;
        }

        public override void Handle(NetHelper nHelper)
        {
            var account = Context.CheckParams.Account;

            if (StateContext.IsBadLog(_response))
            {
                Context.Logger.Debug("bad log detected:" + account.Email);
                Context.SetFinishState(CheckResults.Bad);
                return;
            }

            if (StateContext.IsSecurityQuestion(_response))
            {
                Context.Logger.Debug("security question detected:" + account.Email);
                nHelper.GET("http://amazon.com/homepage=true");
                Context.GatherInformation(nHelper, account);
            }
            else if (StateContext.IsCookiesDisabled(_response))
            {
                Context.Logger.Debug("cookies disabled, restart state object:" + account.Email);
                Context.SetLoginState();
                return;
            }
            else if (StateContext.IsCaptchaMsg(_response))
            {
                Context.SetCaptchaState(_response);
                return;
            }
            else if (StateContext.IsAskCredentials(_response))
            {
                Context.Logger.Debug("ask credentials, restart state object:" + account.Email);
            }
            else if (StateContext.IsAnotherDevice(_response))
            {
                Context.Logger.Debug("is another device answer detected:" + account.Email);
            }
            else
            {
                Context.GatherInformation(nHelper, account);
            }

            Context.SetFinishState(CheckResults.Good);
        }
    }
}