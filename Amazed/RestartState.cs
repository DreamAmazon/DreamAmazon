using System;
using System.Text.RegularExpressions;

namespace DreamAmazon
{
    public class RestartState : CheckState
    {
        private readonly TimeSpan _getMetadataTimeout = TimeSpan.FromMinutes(1);
        private readonly Regex _attributesRegex = new Regex(Globals.REGEX, RegexOptions.Multiline | RegexOptions.IgnoreCase);

        public RestartState(StateContext context) : base(context)
        {
        }

        public override void Handle(NetHelper nHelper)
        {
            var metadataFinder = new MetadataFinder(Context.CheckParams.Account);
            string metadata = metadataFinder.Find(_getMetadataTimeout);

            if (Properties.Settings.Default.Mode == 0 || Properties.Settings.Default.Mode == 2)
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

            var loginResponse = nHelper.GET(Globals.LOG_URL);

            if (StateContext.IsError(loginResponse))
            {
                Context.Logger.Debug("error detected, finish state object:" + Context.CheckParams.Account?.Email);
                Context.ProxyManager.RemoveProxy(nHelper.Proxy);
                Context.SetFinishState();
                return;
            }

            var attributes = StateContext.ParseAccountAttributes(Context.CheckParams.Account, metadata);

            foreach (Match match in _attributesRegex.Matches(loginResponse.Value))
            {
                attributes.Add(match.Groups[1].ToString(), match.Groups[2].ToString());
            }

            var responseResult = nHelper.POST(Globals.POST_URL, attributes);

            Context.SetValidationState(responseResult);
        }
    }
}