using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace CT_Dashboard.Models
{    
    public class ExcelValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
    }

    public class ExcelRowData
    {
        public int RowIndex { get; set; }
        public string Gia_ban { get; set; }
        public string HD { get; set; }
        public decimal? Thuc_hien_HT { get; set; }
        public decimal? Thuc_hien_Luy_ke { get; set; }
        public decimal? Ke_hoach { get; set; }
        public decimal? Cung_ky_Luy_ke { get; set; }
        public decimal? Don_vi_tinh { get; set; }
        public string Nam { get; set; }
        public string Thang { get; set; }
        public DateTime? Ngay_cap_nhat { get; set; }
        public string Dien_giai { get; set; }
    }

    public class ExcelValidator
    {
        private readonly string[] _expectedHeaders = {
            "Giá bán", "HD", "Thực hiện HT", "Thực hiện (Lũy kế)",
            "Kế hoạch", "Cùng kỳ (Lũy kế)", "Đơn vị tính", "Năm",
            "Tháng", "Ngày cập nhật", "Diễn giải"
        };

        public ExcelValidationResult ValidateExcelFile(HttpPostedFileBase file)
        {
            var result = new ExcelValidationResult();

            try
            {
                if (file == null || file.ContentLength == 0)
                {
                    result.Errors.Add("File không được để trống");
                    return result;
                }

                // Kiểm tra định dạng file
                if (!IsValidExcelFile(file))
                {
                    result.Errors.Add("File phải có định dạng .xlsx hoặc .xls");
                    return result;
                }

                using (var stream = file.InputStream)
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        result.Errors.Add("File Excel không có worksheet nào");
                        return result;
                    }

                    // Kiểm tra cấu trúc header
                    ValidateHeaders(worksheet, result);
                    if (result.Errors.Any())
                        return result;

                    // Đọc và validate dữ liệu
                    var data = ReadExcelData(worksheet, result);
                    result.TotalRows = data.Count;

                    // Kiểm tra dữ liệu
                    ValidateData(data, result);

                    result.IsValid = !result.Errors.Any();
                    result.ValidRows = data.Count - result.Errors.Count(e => e.Contains("Dòng"));
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Lỗi khi xử lý file: {ex.Message}");
            }

            return result;
        }

        private bool IsValidExcelFile(HttpPostedFileBase file)
        {
            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(file.FileName)?.ToLower();
            return allowedExtensions.Contains(fileExtension);
        }

        private void ValidateHeaders(ExcelWorksheet worksheet, ExcelValidationResult result)
        {
            var headerRow = 1;
            var actualHeaders = new List<string>();

            // Đọc headers từ file
            for (int col = 1; col <= _expectedHeaders.Length; col++)
            {
                var cellValue = worksheet.Cells[headerRow, col].Value?.ToString()?.Trim();
                actualHeaders.Add(cellValue ?? "");
            }

            // So sánh với headers mẫu
            for (int i = 0; i < _expectedHeaders.Length; i++)
            {
                if (i >= actualHeaders.Count || actualHeaders[i] != _expectedHeaders[i])
                {
                    result.Errors.Add($"Cột {i + 1}: Tiêu đề '{actualHeaders.ElementAtOrDefault(i)}' không khớp với mẫu '{_expectedHeaders[i]}'");
                }
            }

            // Kiểm tra số lượng cột
            if (actualHeaders.Count != _expectedHeaders.Length)
            {
                result.Errors.Add($"Số lượng cột không khớp. Mẫu có {_expectedHeaders.Length} cột, file có {actualHeaders.Count} cột");
            }
        }

        private List<ExcelRowData> ReadExcelData(ExcelWorksheet worksheet, ExcelValidationResult result)
        {
            var data = new List<ExcelRowData>();
            var startRow = 2; // Bắt đầu từ dòng 2 (sau header)
            var endRow = worksheet.Dimension?.End.Row ?? 0;

            for (int row = startRow; row <= endRow; row++)
            {
                // Kiểm tra dòng trống
                if (IsEmptyRow(worksheet, row))
                {
                    result.Warnings.Add($"Dòng {row}: Dòng trống được bỏ qua");
                    continue;
                }

                var rowData = new ExcelRowData
                {
                    RowIndex = row,
                    Gia_ban = GetCellValue(worksheet, row, 1),
                    HD = GetCellValue(worksheet, row, 2),
                    Thuc_hien_HT = GetDecimalValue(worksheet, row, 3),
                    Thuc_hien_Luy_ke = GetDecimalValue(worksheet, row, 4),
                    Ke_hoach = GetDecimalValue(worksheet, row, 5),
                    Cung_ky_Luy_ke = GetDecimalValue(worksheet, row, 6),
                    Don_vi_tinh = GetDecimalValue(worksheet, row, 7),
                    Nam = GetCellValue(worksheet, row, 8),
                    Thang = GetCellValue(worksheet, row, 9),
                    Ngay_cap_nhat = GetDateValue(worksheet, row, 10),
                    Dien_giai = GetCellValue(worksheet, row, 11)
                };

                data.Add(rowData);
            }

            return data;
        }

        private void ValidateData(List<ExcelRowData> data, ExcelValidationResult result)
        {
            var hdValues = new HashSet<string>();

            foreach (var row in data)
            {
                var errors = new List<string>();

                // Kiểm tra các trường bắt buộc không được để trống
                if (string.IsNullOrWhiteSpace(row.Gia_ban))
                    errors.Add("Giá bán");

                if (string.IsNullOrWhiteSpace(row.HD))
                    errors.Add("HD");
                else
                {
                    // Kiểm tra trùng lặp HD
                    if (hdValues.Contains(row.HD))
                    {
                        result.Errors.Add($"Dòng {row.RowIndex}: HD '{row.HD}' bị trùng lặp");
                    }
                    else
                    {
                        hdValues.Add(row.HD);
                    }
                }

                if (string.IsNullOrWhiteSpace(row.Nam))
                    errors.Add("Năm");

                if (string.IsNullOrWhiteSpace(row.Thang))
                    errors.Add("Tháng");

                if (row.Ngay_cap_nhat == null)
                    errors.Add("Ngày cập nhật");

                if (string.IsNullOrWhiteSpace(row.Dien_giai))
                    errors.Add("Diễn giải");

                // Báo lỗi cho các trường bị trống
                if (errors.Any())
                {
                    result.Errors.Add($"Dòng {row.RowIndex}: Các trường sau bị trống: {string.Join(", ", errors)}");
                }

                // Kiểm tra định dạng số
                ValidateNumericFields(row, result);

                // Kiểm tra định dạng năm
                if (!string.IsNullOrWhiteSpace(row.Nam) && !int.TryParse(row.Nam, out int year))
                {
                    result.Errors.Add($"Dòng {row.RowIndex}: Năm '{row.Nam}' không hợp lệ");
                }

                // Kiểm tra định dạng tháng
                if (!string.IsNullOrWhiteSpace(row.Thang) && (!int.TryParse(row.Thang, out int month) || month < 1 || month > 12))
                {
                    result.Errors.Add($"Dòng {row.RowIndex}: Tháng '{row.Thang}' không hợp lệ");
                }
            }
        }

        private void ValidateNumericFields(ExcelRowData row, ExcelValidationResult result)
        {
            // Có thể thêm validation cho các trường số nếu cần
            // Ví dụ: kiểm tra giá trị âm, giới hạn...
        }

        private bool IsEmptyRow(ExcelWorksheet worksheet, int row)
        {
            for (int col = 1; col <= _expectedHeaders.Length; col++)
            {
                if (worksheet.Cells[row, col].Value != null &&
                    !string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Value.ToString()))
                {
                    return false;
                }
            }
            return true;
        }

        private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            return worksheet.Cells[row, col].Value?.ToString()?.Trim();
        }

        private decimal? GetDecimalValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cellValue = worksheet.Cells[row, col].Value;
            if (cellValue == null) return null;

            if (decimal.TryParse(cellValue.ToString(), out decimal result))
                return result;

            return null;
        }

        private DateTime? GetDateValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cellValue = worksheet.Cells[row, col].Value;
            if (cellValue == null) return null;

            if (DateTime.TryParse(cellValue.ToString(), out DateTime result))
                return result;

            if (double.TryParse(cellValue.ToString(), out double oaDate))
                return DateTime.FromOADate(oaDate);

            return null;
        }
    }

}