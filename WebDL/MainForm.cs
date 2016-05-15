using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebDL
{
    public partial class MainForm : Form
    {
        private List<string> urls = new List<string>();
        private List<string> urls_fw = new List<string>();
        private bool append = true;
        private bool no_fw = true;

        public MainForm()
        {
            InitializeComponent();
            navigate(textBoxURL.Text);
        }

        private void navigate(string url, bool append = true, bool no_fw = true)
        {
            url = url.Trim();
            int i = url.IndexOf("http://"), j = url.IndexOf("https://");
            if ((i != 0) && (j != 0))
                url = "http://" + url;
            try
            {
                this.append = append;
                this.no_fw = no_fw;
                webBrowser.Url = new Uri(url);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Laufzeitfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void back()
        {
            if (urls.Any())
            {
                navigate(urls.Last(), false);
            }
        }

        private void forward()
        {
            if (urls_fw.Any())
            {
                navigate(urls_fw.Last(), false, false);
            }
        }

        private void rebuildContextMenuBack()
        {
            ToolStripMenuItem t;
            List<string> l = new List<string>(urls);
            l.Reverse();
            int c = 0;
            contextMenuStripBack.Items.Clear();
            foreach (string i in l)
            {
                t = new ToolStripMenuItem();
                t.Name = "urls_item" + c++;
                t.Size = new System.Drawing.Size(95, 22);
                t.Text = i;
                t.Tag = i;
                t.Click += new EventHandler(urls_item_Click);
                contextMenuStripBack.Items.Add(t);
            }
        }

        private void downloadFile(string url)
        {
            HttpClient c;
            Task<Stream> t;
            FileStream fs;
            string ext;
            webBrowser.Stop();
            ext = Path.GetExtension(url).Trim('.');
            if (ext.Length == 0)
            {
                saveFileDialog.Filter = "";
                saveFileDialog.DefaultExt = "";
            }
            else
            {
                saveFileDialog.Filter = "\"" + ext + "\" File|*." + ext;
                saveFileDialog.DefaultExt = ext;
            }
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (fs = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (c = new HttpClient())
                        {
                            t = c.GetStreamAsync(url);
                            t.Wait();
                            t.Result.CopyTo(fs);
                            t.Result.Dispose();
                        }
                    }
                }
                catch (Exception _e)
                {
                    MessageBox.Show(_e.Message, "Laufzeitfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void updateButtons()
        {
            rebuildContextMenuBack();
            buttonBack.Enabled = urls.Any();
            buttonForward.Enabled = urls_fw.Any();
            buttonRefresh.Enabled = urls.Any();
        }

        private void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            back();
        }

        private void buttonForward_Click(object sender, EventArgs e)
        {
            forward();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            webBrowser.Refresh();
        }

        private void webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            string url = webBrowser.Url.ToString();
            textBoxURL.Text = url;
            if (urls.Any())
            {
                if (urls.Last() != url)
                {
                    if (append)
                    {
                        if (no_fw)
                        {
                            urls_fw.Clear();
                            no_fw = false;
                        }
                        else
                        {
                            if (urls_fw.Any())
                                urls_fw.RemoveAt(urls_fw.Count - 1);
                        }
                        urls.Add(url);
                        append = false;
                    }
                    else
                    {
                        //urls_fw.Add(urls.Last());
                        //urls.RemoveAt(urls.Count - 1);
                    }
                }
            }
            else
            {
                if (append)
                {
                    if (no_fw)
                    {
                        urls_fw.Clear();
                        no_fw = false;
                    }
                    else
                    {
                        if (urls_fw.Any())
                            urls_fw.RemoveAt(urls_fw.Count - 1);
                    }
                    urls.Add(url);
                    append = false;
                }
            }
            updateButtons();
        }

        private void toolStripMenuItemDownload_Click(object sender, EventArgs e)
        {
            downloadFile(textBoxURL.Text);
        }

        private void textBoxURL_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                navigate(textBoxURL.Text);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            webBrowser.Stop();
        }

        private void urls_item_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem t;
            if (sender is ToolStripMenuItem)
            {
                t = (ToolStripMenuItem)sender;
                navigate((string)(t.Tag), false);
            }
        }
    }
}
