using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;

namespace polossk.Universal.Global
{
    public class ExcelOperator
    {
        public Application ExcelApplication;
        public Workbook ExcelWorkbook;
        public Worksheet ExcelWorksheet;
        public ExcelOperator()
        {
            ExcelApplication = new Application();
            ExcelApplication.Visible = false;
        }

        public void CreateExcel()
        {
            ExcelWorkbook = ExcelApplication.Workbooks.Add(true);
            ExcelWorksheet = ExcelWorkbook.Worksheets[1] as Worksheet;
        }

        public void SaveExcel(string fileName)
        {
            ExcelApplication.DisplayAlerts = false;
            ExcelApplication.AlertBeforeOverwriting = false;
            object FileName = fileName;
            string fileType = System.IO.Path.GetExtension(fileName);
            object FileExtend = fileType == ".xls" ? XlFileFormat.xlExcel8 : XlFileFormat.xlOpenXMLWorkbook;
            object Password = "";
            object WriteResPassword = "";
            object ReadOnlyRecommended = false;
            object CreateBackup = false;
            ExcelWorkbook.SaveAs(FileName, FileExtend, Password, WriteResPassword,
                ReadOnlyRecommended, CreateBackup, XlSaveAsAccessMode.xlNoChange);
        }

        public void QuitExcel()
        {
            ExcelApplication.Quit();
        }

        public Range this[object indexRow, object indexColumn]
        {
            get { return ExcelWorksheet.Cells[indexRow, indexColumn]; }
        }

        public void OpenExcel(string fileName)
        {
            ExcelWorkbook = ExcelApplication.Workbooks.Open(fileName);
            ExcelWorksheet = ExcelWorkbook.Worksheets[1] as Worksheet;
        }

        public static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;
            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }
            return columnName;
        }
    }
}
