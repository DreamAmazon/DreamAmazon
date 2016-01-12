using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DreamAmazon
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            this.label2.Text = String.Format("Registered to : {0}\n\n{1}", Globals.LicensedName, Globals.LicensedEmail);
        }
    }
}
