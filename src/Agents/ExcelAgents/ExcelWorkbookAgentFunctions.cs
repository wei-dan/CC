using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace CC.Agents.ExcelAgents;

public static class ExcelWorkbookAgentFunctions
{
    static ExcelWorkbookAgentFunctions()
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

    [Description("Check whether the given Excel file exists. Returns 'true' or 'false'.")]
    public static string CheckExcelFileExists(
        [Description("Full path to the file")] string filePath)
    {
        return File.Exists(filePath) ? "true" : "false";
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
}
