using System.Windows.Forms;

namespace DreamAmazon
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
            label2.Text = string.Format("Registered to : {0}\n\n{1}", Globals.LicensedName, Globals.LicensedEmail);
        }
    }
}
