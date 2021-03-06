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
    /// <summary>
    /// Главная форма (окно) проекта
    /// </summary>
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

        /// <summary>
        /// Окно ввода параметров
        /// </summary>
        private Form parametersForm;
        private ITable table;

        // Объекты разметки для окон
        private TableLayoutPanel mainLayout;
        private TableLayoutPanel topLayout;
        private TableLayoutPanel inputDataTableLayout;

        private Dictionary<string, Tuple<Label, MaskedTextBox>> dataTableItems;

        private DataGridView tableDataGridView;


        private Button StartButton { get; set; }
        private Button ParametersButton { get; set; }
        private Button ParametersFormOkButton { get; set; }
        private Button ResetButton { get; set; }

        private void InitModel(int runwayCount, int specialPlaceCount)
        {
            model = new Model(runwayCount, specialPlaceCount, table);
        }


        #region Initializations
        /// <summary>
        /// Инициализация Таблицы
        /// </summary>
        private void InitTable()
        {
            tableDataGridView = new DataGridView();

            tableDataGridView.Dock = DockStyle.Fill;
            tableDataGridView.Font = new Font("Roboto", 14f, FontStyle.Bold, GraphicsUnit.Pixel);
            tableDataGridView.DefaultCellStyle.Font = new Font("Roboto", 14F, GraphicsUnit.Pixel);
            tableDataGridView.ColumnHeadersHeight = 30;

            tableDataGridView.DataBindingComplete += GraphicBaseOnDataBindingComplete;

            table = new Table(tableDataGridView);
        }

        /// <summary>
        /// Инициализация окна ввода параметров
        /// </summary>
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

            mainLayout.RowCount = 2;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 75F));

            mainLayout.Dock = DockStyle.Fill;
            mainLayout.TabIndex = 0;
            mainLayout.Name = "mainLayout";

            mainLayout.Controls.Add(topLayout, 0, 0);
            mainLayout.Controls.Add(tableDataGridView, 0, 1);
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

            topLayout.Name = "topLayout";
            topLayout.RowCount = 1;
            topLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            topLayout.Dock = DockStyle.Fill;
            topLayout.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;

            topLayout.Controls.Add(StartButton, 0, 0);
            topLayout.Controls.Add(ParametersButton, 1, 0);
            topLayout.Controls.Add(ResetButton, 2, 0);

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

            inputDataTableLayout.RowCount = 4;
            inputDataTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));
            inputDataTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));
            inputDataTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));
            inputDataTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));

            inputDataTableLayout.Name = "inputDataTableLayout";
            inputDataTableLayout.Dock = DockStyle.Fill;

            AddInputsToControls();

            inputDataTableLayout.Controls.Add(ParametersFormOkButton, 2, 3);
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
            };

            foreach (var item in dataTableItems)
            {
                item.Value.Item1.AutoSize = true;
                item.Value.Item1.Font = new Font("Roboto", 10f, FontStyle.Regular);
            }
        }

        

        private void InitButtons()
        {
            InitStartButton();
            InitParametersButton();
            InitParametersFormOkButton();
            InitResetButton();
        }

        private void InitStartButton()
        {
            StartButton = new Button();

            StartButton.Text = "Start";
            StartButton.Font = new Font("Roboto", 16f, FontStyle.Bold, GraphicsUnit.Pixel);
            StartButton.Size = new Size(90, 40);
            StartButton.BackColor = Color.LimeGreen;
            StartButton.FlatStyle = FlatStyle.Flat;

            StartButton.Click += StartButtonOnClick;

            Controls.Add(StartButton);
        }

        private void InitParametersButton()
        {
            ParametersButton = new Button();

            ParametersButton.Text = "Задать параметры";
            ParametersButton.Font = new Font("Roboto", 14f, FontStyle.Bold, GraphicsUnit.Pixel);
            ParametersButton.AutoSize = true;
            ParametersButton.BackColor = Color.White;
            ParametersButton.FlatStyle = FlatStyle.Flat;

            ParametersButton.Click += ParametersButton_Click;
        }

        private void InitResetButton()
        {
            ResetButton = new Button();

            ResetButton.Text = "Очистить таблицу";
            ResetButton.Font = new Font("Roboto", 14f, FontStyle.Bold, GraphicsUnit.Pixel);
            ResetButton.AutoSize = true;
            ResetButton.BackColor = Color.White;
            ResetButton.FlatStyle = FlatStyle.Flat;

            ResetButton.Click += ResetButton_Click;
        }

        private void InitParametersFormOkButton()
        {
            ParametersFormOkButton = new Button();

            ParametersFormOkButton.Text = "OK";
            ParametersFormOkButton.Font = new Font("Roboto", 14f, GraphicsUnit.Pixel);
            ParametersFormOkButton.AutoSize = true;
            ParametersFormOkButton.BackColor = Color.White;
            ParametersFormOkButton.FlatStyle = FlatStyle.Flat;

            ParametersFormOkButton.Click += HandleRunwayCountInputValue;
            ParametersFormOkButton.Click += HandleSPCountInputValue;
            ParametersFormOkButton.Click += HandleProcessingTimeInputValue;

            ParametersFormOkButton.Click += ParametersFormOkButton_Click;
        }

        #endregion


        #region EventHandlers

        /// <summary>
        /// Обработчик события Таблицы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            model.FillTableByTakingOffAircraftsData();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            table.Reset();

            CommonInputData.InputTakingOffMoments.ResetLastPlannedTakingOffMomentIndex();
            CommonInputData.InputTakingOffMoments.ResetLastPermittedTakingOffMomentIndex();

            model.ResetRunways();
            model.ResetSpecialPlaces();
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

        #endregion
    }
}
