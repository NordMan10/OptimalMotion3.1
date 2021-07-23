using OptimalMotion3._1.Domain;
using OptimalMotion3._1.Interfaces;
using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using OptimalMotion3._1.Domain.Static;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace OptimalMotion3._1
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            DoubleBuffered = true;
            WindowState = FormWindowState.Maximized;
            

            InitializeComponent();

            InitTable();

            InitButtons();

            InitTopLayout();
            InitMainLayout();

            Controls.Add(mainLayout);

            InitParametersForm();
            parametersForm.Show();
        }

        private Model model;

        private Form parametersForm;
        private ITable table;

        private TableLayoutPanel mainLayout;
        private TableLayoutPanel topLayout;
        private TableLayoutPanel inputDataTableLayout;

        private Dictionary<string, Tuple<Label, MaskedTextBox>> dataTableItems;

        private DataGridView tableDataGridView;


        private Button StartButton { get; set; }
        private Button ParametersButton { get; set; }
        private Button ParametersFormOkButton { get; set; }
        private Button ResetButton { get; set; }
        private Button AddDepurtureButton { get; set; }

        private void InitModel(int runwayCount, int specialPlaceCount)
        {
            model = new Model(runwayCount, specialPlaceCount, table);
        }

        private void InitTable()
        {
            tableDataGridView = new DataGridView();

            tableDataGridView.Dock = DockStyle.Fill;
            tableDataGridView.Font = new Font("Roboto", 14f, FontStyle.Bold, GraphicsUnit.Pixel);
            tableDataGridView.DefaultCellStyle.Font = new Font("Roboto", 14F, GraphicsUnit.Pixel);
            tableDataGridView.ColumnHeadersHeight = 30;

            tableDataGridView.DataBindingComplete += GraphicBaseOnDataBindingComplete;
            //tableDataGridView.ColumnHeaderMouseClick += TableDataGridView_ColumnHeaderMouseClick;

            table = new Table(tableDataGridView);
        }

        //private void TableDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        //{
        //    tableDataGridView.Sort(tableDataGridView.Columns[e.ColumnIndex], ListSortDirection.Ascending);
        //}

        private void GraphicBaseOnDataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (var property in typeof(TableRow).GetProperties())
            {
                var displayNameAttribute = property.GetCustomAttribute(typeof(DisplayNameAttribute));
                if (displayNameAttribute != null)
                {
                    var propDisplayName = (displayNameAttribute as DisplayNameAttribute).DisplayName;
                    tableDataGridView.Columns[property.Name].HeaderText = propDisplayName;
                }

                tableDataGridView.Columns[property.Name].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                
            }
        }

        private void InitParametersForm()
        {
            parametersForm = new ParametersForm();
            parametersForm.Controls.Add(inputDataTableLayout);
        }

        private void InitMainLayout()
        {
            mainLayout = new TableLayoutPanel();

            mainLayout.ColumnCount = 1;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(topLayout, 0, 0);
            mainLayout.Controls.Add(tableDataGridView, 0, 1);
            mainLayout.Name = "mainLayout";
            mainLayout.RowCount = 2;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 75F));
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.TabIndex = 0;

        }

        private void InitTopLayout()
        {
            topLayout = new TableLayoutPanel();
            InitInputDataTableLayout();

            topLayout.ColumnCount = 4;
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            topLayout.Controls.Add(StartButton, 0, 0);
            topLayout.Controls.Add(ParametersButton, 1, 0);
            topLayout.Controls.Add(ResetButton, 2, 0);
            //topLayout.Controls.Add(AddDepurtureButton, 3, 0);
            topLayout.Name = "topLayout";
            topLayout.RowCount = 1;
            topLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            topLayout.Dock = DockStyle.Fill;
            topLayout.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
        }

        private void InitInputDataTableLayout()
        {
            inputDataTableLayout = new TableLayoutPanel();

            InitDataTableItems();

            inputDataTableLayout.ColumnCount = 3;
            inputDataTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            inputDataTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            inputDataTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            inputDataTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            inputDataTableLayout.Name = "inputDataTableLayout";

            inputDataTableLayout.RowCount = 4;
            inputDataTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));
            inputDataTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));
            inputDataTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));
            inputDataTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));
            inputDataTableLayout.Dock = DockStyle.Fill;

            AddInputsToControls();

            inputDataTableLayout.Controls.Add(ParametersFormOkButton, 2, 3);

            //inputDataTableLayout.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
        }

        private void AddInputsToControls()
        {
            for (var row = 1; row < inputDataTableLayout.RowCount; row += 2)
            {
                for (var column = 0; column < inputDataTableLayout.ColumnCount; column++)
                {
                    var itemIndex = column + (int)Math.Floor((double)row / 2) * inputDataTableLayout.ColumnCount;
                    if (itemIndex >= dataTableItems.Count) break;

                    inputDataTableLayout.Controls.Add(dataTableItems.Values.ToList()[itemIndex].Item1, column, row - 1);
                    inputDataTableLayout.Controls.Add(dataTableItems.Values.ToList()[itemIndex].Item2, column, row);
                }
            }
        }

        private void InitDataTableItems()
        {
            var runwayCountInput = new MaskedTextBox() { Text = "2", TabIndex = 0 };
            var SPCount = new MaskedTextBox() { Text = "2", TabIndex = 1 };
            var processingTime = new MaskedTextBox() { Text = "240", TabIndex = 2 };
            var takingOffStep = new MaskedTextBox() { Text = "180", TabIndex = 3 };

            dataTableItems = new Dictionary<string, Tuple<Label, MaskedTextBox>>
            {
                {
                    "runwayCount", 
                    Tuple.Create(new Label() { Text = "Кол-во ВПП:" }, runwayCountInput)
                },
                {
                    "SPCount",
                    Tuple.Create(new Label() { Text = "Кол-во Спец. площадок:" }, SPCount) 
                },
                {
                    "processingTime", 
                    Tuple.Create(new Label() { Text = "Среднее время обработки:" }, processingTime) 
                },
                //{
                //    "takingOffStep", 
                //    Tuple.Create(new Label() { Text = "Интервал моментов взлета:" }, takingOffStep) 
                //},
            };

            foreach (var item in dataTableItems)
            {
                item.Value.Item1.AutoSize = true;
                item.Value.Item1.Font = new Font("Roboto", 10f, FontStyle.Regular);
            }
        }

        private void HandleRunwayCountInputValue(object sender, EventArgs e)
        {
            var input = dataTableItems["runwayCount"].Item2;
            if (input.Text != "")
                CommonInputData.RunwayCount = int.Parse(input.Text);
        }

        private void HandleSPCountInputValue(object sender, EventArgs e)
        {
            var input = dataTableItems["SPCount"].Item2;
            if (input.Text != "")
                CommonInputData.SpecialPlaceCount = int.Parse(input.Text);
        }

        private void HandleProcessingTimeInputValue(object sender, EventArgs e)
        {
            var input = dataTableItems["processingTime"].Item2;
            if (input.Text != "")
                ConstantTakingOffCreationIntervals.ProcessingTime = int.Parse(input.Text);
        }

        //private void HandleTakingOffStepInputValue(object sender, EventArgs e)
        //{
        //    var input = dataTableItems["takingOffStep"].Item2;
        //    if (input.Text != "")
        //        ModellingParameters.TakingOffMomentStep = int.Parse(input.Text);
        //}

        private void InitButtons()
        {
            InitStartButton();
            InitParametersButton();
            InitParametersFormOkButton();
            InitResetButton();
            //InitAddDepurtureButton();
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

        private void InitParametersButton()
        {
            ParametersButton = new Button();
            ParametersButton.Text = "Задать параметры";
            ParametersButton.Click += ParametersButton_Click;
            ParametersButton.Font = new Font("Roboto", 14f, FontStyle.Bold, GraphicsUnit.Pixel);
            //ParametersButton.Size = new Size(150, 40);
            ParametersButton.AutoSize = true;
            ParametersButton.BackColor = Color.White;
            ParametersButton.FlatStyle = FlatStyle.Flat;
        }

        private void InitResetButton()
        {
            ResetButton = new Button();
            ResetButton.Text = "Очистить таблицу";
            ResetButton.Click += ResetButton_Click; ;
            ResetButton.Font = new Font("Roboto", 14f, FontStyle.Bold, GraphicsUnit.Pixel);
            //ResetTableButton.Size = new Size(150, 40);
            ResetButton.AutoSize = true;
            ResetButton.BackColor = Color.White;
            ResetButton.FlatStyle = FlatStyle.Flat;
        }

        private void InitParametersFormOkButton()
        {
            ParametersFormOkButton = new Button();
            ParametersFormOkButton.Text = "OK";

            ParametersFormOkButton.Font = new Font("Roboto", 14f, GraphicsUnit.Pixel);
            //ParametersButton.Size = new Size(50, 40);
            ParametersFormOkButton.AutoSize = true;
            ParametersFormOkButton.BackColor = Color.White;
            ParametersFormOkButton.FlatStyle = FlatStyle.Flat;

            ParametersFormOkButton.Click += HandleRunwayCountInputValue;
            ParametersFormOkButton.Click += HandleSPCountInputValue;
            ParametersFormOkButton.Click += HandleProcessingTimeInputValue;
            //ParametersFormOkButton.Click += HandleTakingOffStepInputValue;

            ParametersFormOkButton.Click += ParametersFormOkButton_Click;
        }

        private void InitAddDepurtureButton()
        {
            AddDepurtureButton = new Button();
            AddDepurtureButton.Text = "Добавить вылет";

            AddDepurtureButton.Font = new Font("Roboto", 14f, FontStyle.Bold, GraphicsUnit.Pixel);
            AddDepurtureButton.AutoSize = true;
            AddDepurtureButton.BackColor = Color.White;
            AddDepurtureButton.FlatStyle = FlatStyle.Flat;

            AddDepurtureButton.Click += StartButtonOnClick;
        }

        

        private void ParametersFormOkButton_Click(object sender, EventArgs e)
        {
            if (model is null)
                InitModel(CommonInputData.RunwayCount, CommonInputData.SpecialPlaceCount);
            else
                model.UpdateModel(CommonInputData.RunwayCount, CommonInputData.SpecialPlaceCount);

            parametersForm.Hide();
        }

        private void ParametersButton_Click(object sender, EventArgs e)
        {
            parametersForm.Show();
            parametersForm.Controls.Add(inputDataTableLayout);
        }

        private void StartButtonOnClick(object sender, EventArgs e)
        {
            model.InvokeAddTakingOffAircrafts();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            table.Reset();

            CommonInputData.InputTakingOffMoments.ResetLastPlannedTakingOffMomentIndex();
            CommonInputData.InputTakingOffMoments.ResetLastPermittedMomentIndex();

            model.ResetRunways();
            model.ResetSpecialPlaces();
        }
    }
}
