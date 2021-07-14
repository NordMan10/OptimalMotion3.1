using OptimalMotion3._1.Domain;
using OptimalMotion3._1.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OptimalMotion3._1
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            DoubleBuffered = true;

            InitializeComponent();

            table = GetTable();

            InitButtons();

            topLayout = GetTopLayout();
            mainLayout = GetMainLayout();
            Controls.Add(mainLayout);

            WindowState = FormWindowState.Maximized;

            model = new Model(1, 1, table);
        }

        private readonly Model model;
        private readonly ITable table;
        private readonly TableLayoutPanel mainLayout = new TableLayoutPanel();
        private readonly TableLayoutPanel topLayout = new TableLayoutPanel();
        private readonly DataGridView tableDataGridView = new DataGridView();

        public Button StartButton { get; private set; }

        private ITable GetTable()
        {
            tableDataGridView.Dock = DockStyle.Fill;
            tableDataGridView.Font = new Font("Roboto", 14f, FontStyle.Bold, GraphicsUnit.Pixel);
            tableDataGridView.DefaultCellStyle.Font = new Font("Roboto", 14F, GraphicsUnit.Pixel);
            tableDataGridView.ColumnHeadersHeight = 30;

            return new Table(tableDataGridView);
        }

        private void TableDataGridViewOnCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            for (var i = 0; i < tableDataGridView.Columns.Count - 1; i++)
            {
                tableDataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        private void TableDataGridView_DataSourceChanged(object sender, EventArgs e)
        {
            for (var i = 0; i < tableDataGridView.Columns.Count - 1; i++)
            {
                tableDataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        private TableLayoutPanel GetMainLayout()
        {
            mainLayout.ColumnCount = 1;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(topLayout, 0, 0);
            mainLayout.Controls.Add(tableDataGridView, 0, 1);
            mainLayout.Name = "mainLayout";
            mainLayout.RowCount = 2;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 75F));
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.TabIndex = 0;

            return mainLayout;
        }

        private TableLayoutPanel GetTopLayout()
        {
            topLayout.ColumnCount = 4;
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            topLayout.Controls.Add(StartButton, 0, 0);
            topLayout.Name = "topLayout";
            topLayout.RowCount = 1;
            topLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            topLayout.Dock = DockStyle.Fill;

            return topLayout;
        }

        private void InitButtons()
        {
            InitStartButton();
        }

        private void InitStartButton()
        {
            StartButton = new Button();
            StartButton.Text = "Start";
            StartButton.Click += StartButtonOnClick;
            StartButton.Font = new Font("Roboto", 16f, FontStyle.Bold, GraphicsUnit.Pixel);
            StartButton.Size = new Size(90, 40);
            StartButton.BackColor = Color.LimeGreen;
            StartButton.FlatStyle = FlatStyle.Flat;

            Controls.Add(StartButton);
        }

        private void StartButtonOnClick(object sender, EventArgs e)
        {
            //model.ChangeStage(ModelStages.Started);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space)
                Close();
        }
    }
}
