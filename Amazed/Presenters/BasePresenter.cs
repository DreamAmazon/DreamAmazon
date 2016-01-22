using System.Windows.Forms;

namespace DreamAmazon.Presenters
{
    public abstract class BasePresenter
    {
        protected static bool IsViewActive(string viewName)
        {
            return Application.OpenForms[viewName] == null || Application.OpenForms[viewName].Visible == false;
        }
    }
}