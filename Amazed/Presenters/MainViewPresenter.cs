using DreamAmazon.Events;
using DreamAmazon.Interfaces;
using EventAggregatorNet;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon.Presenters
{
    public class MainViewPresenter : BasePresenter, IListener<BalanceRetrievedMessage>
    {
        private readonly IMainView _view;

        public MainViewPresenter(IMainView view)
        {
            Contracts.Require(view != null);

            _view = view;

            var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            eventAggregator.AddListener(this, true);
        }

        public void Start()
        {
            _view.Show();
        }

        public void Handle(BalanceRetrievedMessage message)
        {
            _view.ShowStatusInfo(string.Format("DeathByCaptcha Balance : ${0}", message.Balance));
        }
    }
}