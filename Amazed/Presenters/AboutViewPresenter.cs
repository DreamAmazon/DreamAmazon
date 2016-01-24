using DreamAmazon.Interfaces;

namespace DreamAmazon.Presenters
{
    public class AboutViewPresenter : BasePresenter
    {
        private readonly IAboutView _view;

        public AboutViewPresenter(IAboutView view)
        {
            Contracts.Require(view != null);

            _view = view;
        }

        public void Start()
        {
            if (IsViewActive("frmSettings"))
            {
                _view.Show();
            }
        }
    }
}