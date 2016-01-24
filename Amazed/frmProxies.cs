using System;
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

        public void DisplayProxy(Proxy proxy)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => DisplayProxy(proxy)));
            }
            else
            {
                var lvi = new ListViewItem(proxy.IpAddress);
                lvi.SubItems.Add(proxy.Port.ToString());
                lvProxies.Items.Add(lvi);
            }
        }

        public void RemoveProxy(Proxy proxy)
        {
            ListViewItem item = null;
            foreach (ListViewItem lvi in lvProxies.Items)
            {
                if (lvi.Text == proxy.IpAddress && lvi.SubItems[0].Text == proxy.Port.ToString())
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
