using System;
using System.Net;
using System.Windows.Forms;
using DreamAmazon.Interfaces;

namespace DreamAmazon
{
    public partial class frmProxies : BaseView, IProxiesView
    {
        public event VoidHandler ImportProxiesRequested;
        public event VoidHandler ClearProxiesRequested;
        public event VoidHandler TestProxiesRequested;

        public frmProxies()
        {
            InitializeComponent();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            OnImportProxiesRequested();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            OnClearProxiesRequested();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            OnTestProxiesRequested();
        }

        public void ClearProxies()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearProxies));
            }
            else
            {
                lvProxies.Items.Clear();
            }
        }

        public void DisplayProxy(Uri proxyAddress)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => DisplayProxy(proxyAddress)));
            }
            else
            {
                var lvi = new ListViewItem(proxyAddress.Host);
                lvi.SubItems.Add(proxyAddress.Port.ToString());
                lvProxies.Items.Add(lvi);
            }
        }

        public void RemoveProxy(Uri proxyAddress)
        {
            ListViewItem item = null;
            foreach (ListViewItem lvi in lvProxies.Items)
            {
                if (lvi.Text == proxyAddress.Host && lvi.SubItems[0].Text == proxyAddress.Port.ToString())
                {
                    item = lvi;
                    break;
                }
            }

            if (item != null)
            {
                lvProxies.Items.Remove(item);
            }
        }

        public void EnableTestProxiesRequest(bool b)
        {
            btnTest.Enabled = b;
        }

        public void EnableImportProxiesRequested(bool b)
        {
            btnImport.Enabled = b;
        }

        protected virtual void OnImportProxiesRequested()
        {
            ImportProxiesRequested?.Invoke();
        }

        protected virtual void OnClearProxiesRequested()
        {
            ClearProxiesRequested?.Invoke();
        }

        protected virtual void OnTestProxiesRequested()
        {
            TestProxiesRequested?.Invoke();
        }
    }
}
