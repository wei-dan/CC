using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;

public static partial class ExcelAgent
{
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
}
