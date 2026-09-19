using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CT_Dashboard.Models
{
    /// <summary>
    /// ViewModel pour afficher le détail d'un chỉ tiêu
    /// </summary>
    public class ChiTieuDetailViewModel
    {  
        public int ID { get; set; }

        [Display(Name = "Mã chỉ tiêu")]
        public string MaChiTieu { get; set; }

        [Display(Name = "Tên chỉ tiêu")]
        public string TenChiTieu { get; set; }

        [Display(Name = "Tên đầy đủ")]
        public string TenDayDu { get; set; }

        [Display(Name = "Mã địa bàn")]
        public string MaDiaBan { get; set; }

        [Display(Name = "Tên địa bàn")]
        public string TenDiaBan { get; set; }

        [Display(Name = "Đơn vị")]
        public string DonVi { get; set; }

        [Display(Name = "Tháng")]
        public int Thang { get; set; }

        [Display(Name = "Năm")]
        public int Nam { get; set; }

        [Display(Name = "Ngày báo cáo")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime NgayBaoCao { get; set; }

        [Display(Name = "Thực hiện trong kỳ")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
        public double ThucHienTrongKy { get; set; }

        [Display(Name = "Thực hiện lũy kế")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
        public double ThucHien { get; set; }

        [Display(Name = "Kế hoạch")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
        public double KeHoach { get; set; }

        [Display(Name = "Cùng kỳ năm trước")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
        public double CungKy { get; set; }

        [Display(Name = "Đơn vị tính")]
        public string DonViTinh { get; set; }

        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }

        [Display(Name = "Trạng thái duyệt")]
        public string TrangThaiDuyet { get; set; }

        [Display(Name = "Ngày tạo")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime NgayTao { get; set; }

        [Display(Name = "Người tạo")]
        public string NguoiTao { get; set; }

        // Các property tính toán
        [Display(Name = "Còn lại")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
        public double ConLai => KeHoach - ThucHien;

        [Display(Name = "Tỷ lệ hoàn thành (%)")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = false)]
        public double TyLeHoanThanh => KeHoach != 0 ? Math.Round((double)ThucHien * 100 / KeHoach, 2) : 0;

        [Display(Name = "So với cùng kỳ (%)")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = false)]
        public double SoVoiCungKy => CungKy != 0 ? Math.Round((double)ThucHien * 100 / CungKy - 100, 2) : 0;

        [Display(Name = "Trạng thái")]
        public string TrangThaiHoanThanh => TyLeHoanThanh >= 100 ? "Đã hoàn thành" :
                                           TyLeHoanThanh >= 80 ? "Sắp hoàn thành" :
                                           TyLeHoanThanh >= 50 ? "Đang thực hiện" : "Chậm tiến độ";

        public string CssClassTrangThai => TyLeHoanThanh >= 100 ? "text-success" :
                                          TyLeHoanThanh >= 80 ? "text-info" :
                                          TyLeHoanThanh >= 50 ? "text-warning" : "text-danger";

        //public long ID { get; set; }
        //public string MaChiTieu { get; set; }
        //public string TenChiTieu { get; set; }
        //public string MaDiaBan { get; set; }
        //public string TenDiaBan { get; set; }
        //public string DonVi { get; set; }
        //public int Thang { get; set; }
        //public int Nam { get; set; }
        //public DateTime NgayBaoCao { get; set; }
        //public double ThucHienTrongKy { get; set; }
        //public double ThucHien { get; set; }
        //public double KeHoach { get; set; }
        //public double CungKy { get; set; }
        //public string DonViTinh { get; set; }
        //public string GhiChu { get; set; }
        //public string TrangThaiDuyet { get; set; }
        //public DateTime NgayTao { get; set; }
        //public string NguoiTao { get; set; }

        //// Calculated properties
        //public double TyLeHoanThanh
        //{
        //    get { return KeHoach > 0 ? Math.Round((double)ThucHien / KeHoach * 100, 1) : 0; }
        //}

        //public double TangTruongSoVoiCungKy
        //{
        //    get { return CungKy > 0 ? Math.Round((double)(ThucHien - CungKy) / CungKy * 100, 1) : 0; }
        //}

        //public string TrangThaiHoanThanh
        //{
        //    get
        //    {
        //        if (TyLeHoanThanh >= 100) return "Đạt";
        //        if (TyLeHoanThanh >= 80) return "Gần đạt";
        //        return "Chưa đạt";
        //    }
        //}
    }

    /// <summary>
    /// ViewModel pour le graphique détaillé
    /// </summary>
    public class ChartDetailViewModel
    {
        public string MaNhomChiTieu { get; set; }
        public string TenNhomChiTieu { get; set; }
        public string LoaiChart { get; set; }
        public ChartDataModel ChartData { get; set; }
        public DateTime NgayCapNhat { get; set; }

        public ChartDetailViewModel()
        {
            ChartData = new ChartDataModel();
        }
    }

    /// <summary>
    /// Model dữ liệu cho biểu đồ
    /// </summary>
    public class ChartDataModel
    {
        public List<ChartItemModel> ChartItems { get; set; }
        public DateTime NgayCapNhat { get; set; }
        public int Thang { get; set; }

        public ChartDataModel()
        {
            ChartItems = new List<ChartItemModel>();
        }
    }

    /// <summary>
    /// Model item cho biểu đồ
    /// </summary>
    public class ChartItemModel
    {
        public string Label { get; set; }
        public string MaChiTieu { get; set; }
        public double ThucHien { get; set; }
        public double KeHoach { get; set; }
        public double CungKy { get; set; }
        public string DonViTinh { get; set; }

        public double TyLeHoanThanh
        {
            get { return KeHoach > 0 ? Math.Round(ThucHien / KeHoach * 100, 1) : 0; }
        }
    }

    /// <summary>
    /// ViewModel cho so sánh địa bàn
    /// </summary>
    public class SoSanhDiaBanViewModel
    {
        public string MaChiTieu { get; set; }
        public string TenChiTieu { get; set; }       
        public string TenDayDu { get; set; }
        public int Nam { get; set; }
        public List<SoSanhDiaBanItemModel> DataSoSanh { get; set; }

        public SoSanhDiaBanViewModel()
        {
            DataSoSanh = new List<SoSanhDiaBanItemModel>();
        }
    }

    /// <summary>
    /// Model item cho so sánh địa bàn
    /// </summary>
    public class SoSanhDiaBanItemModel
    {
        public string MaDiaBan { get; set; }
        public string TenDiaBan { get; set; }
        public double ThucHien { get; set; }
        public double KeHoach { get; set; }
        public double CungKy { get; set; }
        public double ThucHienTrongKy { get; set; }
        public string DonViTinh { get; set; }
        public int ViTri { get; set; }

        public double TyLeHoanThanh
        {
            get { return KeHoach > 0 ? Math.Round(ThucHien / KeHoach * 100, 1) : 0; }
        }

        public double TangTruongSoVoiCungKy
        {
            get { return CungKy > 0 ? Math.Round((ThucHien - CungKy) / CungKy * 100, 1) : 0; }
        }
    }

    /// <summary>
    /// Extension cho BaoCaoViewModel để thêm các thuộc tính tính toán
    /// </summary>
    public partial class BaoCaoViewModel
    {
        // Propriétés de base - ajoutez ces propriétés si elles n'existent pas déjà
        public double ThucHien { get; set; }
        public double KeHoach { get; set; }
        public double CungKy { get; set; }

        // Autres propriétés communes
        public string MaChiTieu { get; set; }
        public string TenChiTieu { get; set; }       
        public string TenDayDu { get; set; }
        public string MaDiaBan { get; set; }
        public string TenDiaBan { get; set; }
        public string DonViTinh { get; set; }
        public DateTime? NgaySoLieu { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public double ThucHienTrongKy { get; set; }
        public double Conlai { get; set; }
        public double TyLeHoanThanh { get; set; }
        public string DienGiai { get; set; }

        //// Propriétés calculées
        //public double TyLeHoanThanh
        //{
        //    get { return KeHoach > 0 ? Math.Round(ThucHien / KeHoach * 100, 1) : 0; }
        //}

        public double TangTruongSoVoiCungKy
        {
            get { return CungKy > 0 ? Math.Round((ThucHien - CungKy) / CungKy * 100, 1) : 0; }
        }

        public string TrangThaiHoanThanh
        {
            get
            {
                if (TyLeHoanThanh >= 100) return "Đạt";
                if (TyLeHoanThanh >= 80) return "Gần đạt";
                return "Chưa đạt";
            }
        }

        public string MauSacTrangThai
        {
            get
            {
                if (TyLeHoanThanh >= 100) return "success";
                if (TyLeHoanThanh >= 80) return "warning";
                return "danger";
            }
        }

        // Propriétés supplémentaires utiles
        public string IconTrangThai
        {
            get
            {
                if (TyLeHoanThanh >= 100) return "mdi-check-circle";
                if (TyLeHoanThanh >= 80) return "mdi-alert-circle";
                return "mdi-close-circle";
            }
        }

        public string TrendIcon
        {
            get
            {
                if (TangTruongSoVoiCungKy > 0) return "mdi-trending-up";
                if (TangTruongSoVoiCungKy < 0) return "mdi-trending-down";
                return "mdi-trending-neutral";
            }
        }

        public string TrendColor
        {
            get
            {
                if (TangTruongSoVoiCungKy > 0) return "text-success";
                if (TangTruongSoVoiCungKy < 0) return "text-danger";
                return "text-muted";
            }
        }
    }

    // Alternative: Si vous voulez étendre une classe existante BaoCaoViewModel
    public static class BaoCaoViewModelExtensions
    {
        public static double GetTyLeHoanThanh(this BaoCaoViewModel model)
        {
            return model.KeHoach > 0 ? Math.Round(model.ThucHien / model.KeHoach * 100, 1) : 0;
        }

        public static double GetTangTruongSoVoiCungKy(this BaoCaoViewModel model)
        {
            return model.CungKy > 0 ? Math.Round((model.ThucHien - model.CungKy) / model.CungKy * 100, 1) : 0;
        }

        public static string GetTrangThaiHoanThanh(this BaoCaoViewModel model)
        {
            var tyLe = model.GetTyLeHoanThanh();
            if (tyLe >= 100) return "Đạt";
            if (tyLe >= 80) return "Gần đạt";
            return "Chưa đạt";
        }

        public static string GetMauSacTrangThai(this BaoCaoViewModel model)
        {
            var tyLe = model.GetTyLeHoanThanh();
            if (tyLe >= 100) return "success";
            if (tyLe >= 80) return "warning";
            return "danger";
        }
    }

    /// <summary>
    /// ViewModel cho báo cáo tổng hợp
    /// </summary>
    public class BaoCaoTongHopViewModel
    {
        public string TieuDe { get; set; }
        public DateTime NgayBaoCao { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public List<BaoCaoViewModel> DanhSachBaoCao { get; set; }
        public ThongKeTongHopModel ThongKe { get; set; }

        public BaoCaoTongHopViewModel()
        {
            DanhSachBaoCao = new List<BaoCaoViewModel>();
            ThongKe = new ThongKeTongHopModel();
        }
    }

    /// <summary>
    /// Model thống kê tổng hợp
    /// </summary>
    public class ThongKeTongHopModel
    {
        public double TongThucHien { get; set; }
        public double TongKeHoach { get; set; }
        public double TongCungKy { get; set; }
        public double TyLeTrungBinh { get; set; }
        public double TangTruongTrungBinh { get; set; }
        public int SoLuongChiTieu { get; set; }
        public int SoChiTieuDat { get; set; }
        public int SoChiTieuChuaDat { get; set; }

        public double TyLeChiTieuDat
        {
            get { return SoLuongChiTieu > 0 ? Math.Round((double)SoChiTieuDat / SoLuongChiTieu * 100, 1) : 0; }
        }
    }

    /// <summary>
    /// ViewModel cho xuất báo cáo
    /// </summary>
    public class ExportReportViewModel
    {
        public string TenBaoCao { get; set; }
        public string LoaiBaoCao { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public string DonVi { get; set; }
        public List<string> DanhSachChiTieu { get; set; }
        public string DinhDang { get; set; } // Excel, PDF, Word

        public ExportReportViewModel()
        {
            DanhSachChiTieu = new List<string>();
            DinhDang = "Excel";
        }
    }

    /// <summary>
    /// ViewModel cho tìm kiếm và lọc dữ liệu
    /// </summary>
    public class SearchFilterViewModel
    {
        public string TuKhoa { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public string DonVi { get; set; }
        public string DiaBan { get; set; }
        public string ChiTieu { get; set; }
        public string TrangThai { get; set; }
        public double? TyLeHoanThanhMin { get; set; }
        public double? TyLeHoanThanhMax { get; set; }
        public string SapXepTheo { get; set; }
        public string HuongSapXep { get; set; } // ASC, DESC

        public SearchFilterViewModel()
        {
            SapXepTheo = "NgayBaoCao";
            HuongSapXep = "DESC";
        }
    }

    /// <summary>
    /// Response model cho API calls
    /// </summary>
    public class ApiResponseModel<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }

        public ApiResponseModel()
        {
            Success = true;
            Message = "Thành công";
        }

        public ApiResponseModel(bool success, string message, T data = default(T))
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }
}