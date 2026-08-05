using System;
using System.ComponentModel;
using System.IO;
using OfficeOpenXml;

namespace CC.Agents.ExcelAgents;

public static class ExcelLayoutAgentFunctions
{
    [Description("Set the width of a column (e.g., A, B).")]
    public static string SetColumnWidth(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Column letter, e.g., A")] string columnLetter,
        [Description("Column width in characters")] double width)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";
        try
        {
            int col = 0;
            string upper = columnLetter.ToUpperInvariant();
            foreach (char c in upper)
            {
                if (c < 'A' || c > 'Z')
                    throw new ArgumentException($"Invalid column letter: {columnLetter}");
                col = col * 26 + (c - 'A' + 1);
            }

            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            sheet.Column(col).Width = width;
            package.Save();
            return $"Column {columnLetter} width set to {width} in sheet '{sheetName}'.";
        }
        catch (Exception ex)
        {
            return $"Error setting column width: {ex.Message}";
        }
    }

    [Description("Set the height of a row by row number (1‑based).")]
    public static string SetRowHeight(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Row number, 1‑based")] int rowIndex,
        [Description("Row height in points")] double height)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";
        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            sheet.Row(rowIndex).Height = height;
            package.Save();
            return $"Row {rowIndex} height set to {height} in sheet '{sheetName}'.";
        }
        catch (Exception ex)
        {
            return $"Error setting row height: {ex.Message}";
        }
    }

    [Description("Merge the specified cell range in the worksheet. Provide range as top‑left:bottom‑right, e.g., A1:B2.")]
    public static string MergeCells(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Range, e.g., A1:B2")] string rangeAddress)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";
        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            sheet.Cells[rangeAddress].Merge = true;
            package.Save();
            return $"Range {rangeAddress} merged in sheet '{sheetName}'.";
        }
        catch (Exception ex)
        {
            return $"Error merging cells: {ex.Message}";
        }
    }

    [Description("Unmerge the specified cell range in the worksheet.")]
    public static string UnmergeCells(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Range, e.g., A1:B2")] string rangeAddress)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";
        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            sheet.Cells[rangeAddress].Merge = false;
            package.Save();
            return $"Range {rangeAddress} unmerged in sheet '{sheetName}'.";
        }
        catch (Exception ex)
        {
            return $"Error unmerging cells: {ex.Message}";
        }
    }

    [Description("Auto‑fit column widths for a range or the entire used range of the worksheet. Leave range empty to autofit all columns.")]
    public static string AutoFitColumns(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Optional range (e.g., A1:D10) or leave empty for all")] string rangeAddress = "")
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";
        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            if (string.IsNullOrWhiteSpace(rangeAddress))
                sheet.Cells.AutoFitColumns();
            else
                sheet.Cells[rangeAddress].AutoFitColumns();

            package.Save();
            return $"Auto-fit columns done for sheet '{sheetName}'.";
        }
        catch (Exception ex)
        {
            return $"Error auto‑fitting columns: {ex.Message}";
        }
    }

    [Description("Auto‑fit row heights for the specified range or the entire used range. Leave range empty to autofit all rows.")]
    public static string AutoFitRows(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Optional range (e.g., A1:D10) or leave empty for all")] string rangeAddress = "")
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";
        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            int startRow, endRow;
            if (string.IsNullOrWhiteSpace(rangeAddress))
            {
                if (sheet.Dimension == null)
                    return "No data in worksheet; nothing to auto-fit.";
                startRow = sheet.Dimension.Start.Row;
                endRow = sheet.Dimension.End.Row;
            }
            else
            {
                var range = sheet.Cells[rangeAddress];
                startRow = range.Start.Row;
                endRow = range.End.Row;
            }

            // 取消手动设置的行高，让 Excel 根据内容自动计算行高
            for (int r = startRow; r <= endRow; r++)
            {
                sheet.Row(r).CustomHeight = false;
            }

            package.Save();
            return $"Auto-fit rows done for sheet '{sheetName}'.";
        }
        catch (Exception ex)
        {
            return $"Error auto‑fitting rows: {ex.Message}";
        }
    }
}
