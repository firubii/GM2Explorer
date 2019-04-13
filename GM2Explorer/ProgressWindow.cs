using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GM2Explorer
{
    public partial class ProgressWindow : Form
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }

        public void SetText(string text)
        {
            this.progressText.Text = text;
        }

        public void SetProgressMax(int maximum)
        {
            this.progressBar.Maximum = maximum;
        }

        public void SetProgressValue(int value)
        {
            this.progressBar.Value = value;
        }
    }
}
