using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using OptimalMotion3._1.Interfaces;

namespace OptimalMotion3._1.Domain
{
    /// <summary>
    /// Класс, представляющий объект Таблицы и предоставляющий интерфейс управления ею
    /// </summary>
    public class Table : ITable
    {
        public Table(DataGridView graphicBase)
        {
            this.graphicBase = graphicBase;
            data = new BindingList<TableRow>();

            this.graphicBase.DataSource = data;
        }

        private readonly DataGridView graphicBase;
        private readonly BindingList<TableRow> data;

        /// <summary>
        /// Добавление строки
        /// </summary>
        /// <param name="rowCreationData"></param>
        public void AddRow(TableRow tableRow)
        {
            data.Add(tableRow);
        }

        /// <summary>
        /// Удаление строки
        /// </summary>
        /// <param name="id"></param>
        public void RemoveRow(int id)
        {
            // Получаем индекс строки таблицы в списке
            var rowIndex = GetTableRowIndexById(id);

            // Удаляем строку из списка
            data.RemoveAt(rowIndex);
        }

        /// <summary>
        /// Изменение значения строки
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newRow"></param>
        public void UpdateRow(int id, TableRow newRow)
        {
            // Принимаем новую строку с новыми значениями
            var updatedRow = newRow;

            // Получаем индекс строки таблицы в списке
            var rowIndex = GetTableRowIndexById(id);

            // Удаляем старую строку
            data.RemoveAt(rowIndex);

            // Заменяем старую строку на новую
            data.Insert(rowIndex, updatedRow);
        }

        /// <summary>
        /// Очищает таблицу, удаляя все сохраненные значения
        /// </summary>
        public void Reset()
        {
            data.Clear();
        }

        /// <summary>
        /// Возвращает индекс строки по Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int GetTableRowIndexById(int id)
        {
            // Возвращаем Id -1;
            return id - 1;
        }
    }
}
