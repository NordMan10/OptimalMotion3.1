using OptimalMotion3._1.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimalMotion3._1.Interfaces
{
    public interface ITable
    {
        void AddRow(TableRow tableData);
        void RemoveRow(int id);
        void UpdateRow(int id, TableRow newRow);
        void Reset();
    }
}
