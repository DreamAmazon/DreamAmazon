using DreamAmazon.Interfaces;

namespace DreamAmazon
{
    public partial class frmAbout : BaseView, IAboutView
    {
        public frmAbout()
        {
            InitializeComponent();
            label2.Text = string.Format("Registered to : {0}\n\n{1}", Globals.LicensedName, Globals.LicensedEmail);
        }
    }
}
