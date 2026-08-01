using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using OfficeOpenXml;

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
}
