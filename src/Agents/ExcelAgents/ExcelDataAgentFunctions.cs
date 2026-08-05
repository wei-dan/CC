using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using OfficeOpenXml;

namespace CC.Agents.ExcelAgents;

public static class ExcelDataAgentFunctions
{
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
}
