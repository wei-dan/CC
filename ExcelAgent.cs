using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using LicenseContext = OfficeOpenXml.LicenseContext;

public static class ExcelAgent
{
    static ExcelAgent()
    {
        // Set LicenseContext to NonCommercial if you do not have a valid license.
        // For production use, obtain and set a license key.
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    [Description("Open the Excel file at the given path and return all worksheet names, separated by commas.")]
    public static string OpenExcelWorkbook(
        [Description("Full path to the Excel file (e.g., C:\\data.xlsx)")] string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return "Error: file path is empty.";
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";

        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheets = package.Workbook.Worksheets.Select(s => s.Name);
            return string.Join(", ", sheets);
        }
        catch (Exception ex)
        {
            return $"Error opening workbook: {ex.Message}";
        }
    }

    [Description("Read the text value of a single cell (e.g., A1) in the specified worksheet.")]
    public static string ReadExcelCell(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Cell address, e.g., A1")] string cellAddress)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";

        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            var cell = sheet.Cells[cellAddress];
            if (cell == null)
                return $"Error: cell '{cellAddress}' not found.";

            return cell.Text;
        }
        catch (Exception ex)
        {
            return $"Error reading cell: {ex.Message}";
        }
    }

    [Description("Write a string value to a specific cell in the worksheet and save the file immediately.")]
    public static string WriteExcelCell(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Cell address")] string cellAddress,
        [Description("Value to write")] string value)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";

        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            sheet.Cells[cellAddress].Value = value;
            package.Save();
            return $"Successfully wrote '{value}' to cell {cellAddress} in sheet '{sheetName}'.";
        }
        catch (Exception ex)
        {
            return $"Error writing cell: {ex.Message}";
        }
    }

    [Description("Read a rectangular range (e.g., A1:C3) from a worksheet. Each row is returned as comma‑separated values, rows separated by ' ; '.")]
    public static string ReadExcelRange(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Top‑left cell (e.g., A1)")] string fromCell,
        [Description("Bottom‑right cell (e.g., C3)")] string toCell)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";

        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            var range = sheet.Cells[$"{fromCell}:{toCell}"];
            if (range == null)
                return $"Error: range '{fromCell}:{toCell}' invalid.";

            var rows = new List<string>();
            for (int row = range.Start.Row; row <= range.End.Row; row++)
            {
                var cellsInRow = new List<string>();
                for (int col = range.Start.Column; col <= range.End.Column; col++)
                {
                    cellsInRow.Add(sheet.Cells[row, col].Text);
                }
                rows.Add(string.Join(", ", cellsInRow));
            }
            return string.Join(" ; ", rows);
        }
        catch (Exception ex)
        {
            return $"Error reading range: {ex.Message}";
        }
    }

    [Description("Return the address of the used range (e.g., A1:D10) in the specified worksheet, or 'None (empty sheet)'.")]
    public static string GetExcelUsedRange(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";

        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            if (sheet.Dimension == null)
                return "None (empty sheet)";

            return sheet.Dimension.Address;
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [Description("Append a row to the end of the specified worksheet. Provide values separated by commas. Returns the new row number.")]
    public static string AppendExcelRow(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Comma‑separated values, e.g., hello,world,123")] string values)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";

        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            int lastRow = sheet.Dimension?.End.Row ?? 0;
            int newRow = lastRow + 1;
            var parts = values.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                sheet.Cells[newRow, i + 1].Value = parts[i].Trim();
            }
            package.Save();
            return $"Appended to row {newRow} in sheet '{sheetName}'.";
        }
        catch (Exception ex)
        {
            return $"Error appending row: {ex.Message}";
        }
    }

    [Description("Create a new Excel workbook with a single sheet named 'Sheet1'. Returns the path of the saved file.")]
    public static string CreateExcelWorkbook(
        [Description("Full path including filename, e.g., C:\\new.xlsx")] string filePath)
    {
        try
        {
            if (File.Exists(filePath))
                return $"Error: file already exists: {filePath}";

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Sheet1");
            package.SaveAs(new FileInfo(filePath));
            return $"Workbook created and saved at '{filePath}'.";
        }
        catch (Exception ex)
        {
            return $"Error creating workbook: {ex.Message}";
        }
    }

    [Description("Add a new worksheet to an existing Excel workbook.")]
    public static string AddExcelSheet(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Name for the new sheet")] string sheetName)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";

        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            if (package.Workbook.Worksheets[sheetName] != null)
                return $"Error: sheet '{sheetName}' already exists.";

            package.Workbook.Worksheets.Add(sheetName);
            package.Save();
            return $"Sheet '{sheetName}' added successfully.";
        }
        catch (Exception ex)
        {
            return $"Error adding sheet: {ex.Message}";
        }
    }

    [Description("Delete the worksheet with the given name from the Excel workbook.")]
    public static string DeleteExcelSheet(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Name of the sheet to delete")] string sheetName)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";

        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            package.Workbook.Worksheets.Delete(sheet);
            package.Save();
            return $"Sheet '{sheetName}' deleted.";
        }
        catch (Exception ex)
        {
            return $"Error deleting sheet: {ex.Message}";
        }
    }

    [Description("Check whether the given Excel file exists. Returns 'true' or 'false'.")]
    public static string CheckExcelFileExists(
        [Description("Full path to the file")] string filePath)
    {
        return File.Exists(filePath) ? "true" : "false";
    }

    // ---------- 新增样式相关方法 ----------

    [Description("Set the background color of a single cell using a color name or hex code (e.g., Red, #FF0000).")]
    public static string SetCellBackgroundColor(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Cell address, e.g., A1")] string cellAddress,
        [Description("Color name or hex code, e.g., Blue, #00FF00")] string colorString)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";

        try
        {
            Color color;
            if (colorString.StartsWith("#"))
                color = ColorTranslator.FromHtml(colorString);
            else
                color = Color.FromName(colorString);
            // Fallback: try to parse as hex if not recognized
            if (!color.IsKnownColor && !color.IsNamedColor && !colorString.StartsWith("#"))
            {
                color = ColorTranslator.FromHtml(colorString);
            }

            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            var cell = sheet.Cells[cellAddress];
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(color);
            package.Save();
            return $"Background color of {cellAddress} in sheet '{sheetName}' set to {color.Name}.";
        }
        catch (Exception ex)
        {
            return $"Error setting background color: {ex.Message}";
        }
    }

    [Description("Set the font color of a single cell using a color name or hex code (e.g., Red, #FF0000).")]
    public static string SetCellFontColor(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Cell address, e.g., A1")] string cellAddress,
        [Description("Color name or hex code, e.g., Blue, #00FF00")] string colorString)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";

        try
        {
            Color color;
            if (colorString.StartsWith("#"))
                color = ColorTranslator.FromHtml(colorString);
            else
                color = Color.FromName(colorString);
            if (!color.IsKnownColor && !color.IsNamedColor && !colorString.StartsWith("#"))
                color = ColorTranslator.FromHtml(colorString);

            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            var cell = sheet.Cells[cellAddress];
            cell.Style.Font.Color.SetColor(color);
            package.Save();
            return $"Font color of {cellAddress} in sheet '{sheetName}' set to {color.Name}.";
        }
        catch (Exception ex)
        {
            return $"Error setting font color: {ex.Message}";
        }
    }

    [Description("Set the font to bold or normal for a single cell.")]
    public static string SetCellFontBold(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Cell address, e.g., A1")] string cellAddress,
        [Description("True to make bold, false for normal")] bool bold)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";
        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            sheet.Cells[cellAddress].Style.Font.Bold = bold;
            package.Save();
            return $"Cell {cellAddress} bold set to {bold}.";
        }
        catch (Exception ex)
        {
            return $"Error setting bold: {ex.Message}";
        }
    }

    [Description("Set the font to italic or normal for a single cell.")]
    public static string SetCellFontItalic(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Cell address, e.g., A1")] string cellAddress,
        [Description("True to make italic, false for normal")] bool italic)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";
        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            sheet.Cells[cellAddress].Style.Font.Italic = italic;
            package.Save();
            return $"Cell {cellAddress} italic set to {italic}.";
        }
        catch (Exception ex)
        {
            return $"Error setting italic: {ex.Message}";
        }
    }

    [Description("Set the font size for a single cell.")]
    public static string SetCellFontSize(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Cell address, e.g., A1")] string cellAddress,
        [Description("Font size, e.g., 12")] float size)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";
        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            sheet.Cells[cellAddress].Style.Font.Size = size;
            package.Save();
            return $"Font size of {cellAddress} set to {size}.";
        }
        catch (Exception ex)
        {
            return $"Error setting font size: {ex.Message}";
        }
    }

    [Description("Apply a thin border around a single cell or range (e.g., A1 or A1:B2).")]
    public static string SetCellBorder(
        [Description("Full path to the Excel file")] string filePath,
        [Description("Worksheet name")] string sheetName,
        [Description("Cell or range address, e.g., A1 or A1:B2")] string cellOrRangeAddress)
    {
        if (!File.Exists(filePath))
            return $"Error: file not found: {filePath}";
        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[sheetName];
            if (sheet == null)
                return $"Error: sheet '{sheetName}' not found.";

            var range = sheet.Cells[cellOrRangeAddress];
            range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            package.Save();
            return $"Border applied to {cellOrRangeAddress}.";
        }
        catch (Exception ex)
        {
            return $"Error applying border: {ex.Message}";
        }
    }

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
