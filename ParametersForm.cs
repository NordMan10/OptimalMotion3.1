using System;
using System.Windows.Forms;

namespace OptimalMotion3._1
{
    public partial class ParametersForm : Form
    {
        public ParametersForm()
        {
            InitializeComponent();

            Width = 450;
            Height = 180;

            TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void ParametersForm_Load(object sender, EventArgs e)
        {
            ControlBox = false;
        }
    }
}
