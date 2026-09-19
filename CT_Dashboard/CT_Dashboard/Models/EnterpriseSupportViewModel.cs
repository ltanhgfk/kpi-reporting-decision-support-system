// Models/EnterpriseSupportViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CT_Dashboard.Models
{
    /// <summary>
    /// ViewModel cho báo cáo hỗ trợ doanh nghiệp
    /// </summary>
    public class EnterpriseSupportViewModel
    {
        // Thông tin chung
        public DateTime NgayBaoCao { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public string DonViBaoCao { get; set; }

        // Thống kê tổng quan
        public ThongKeTongQuanModel ThongKeTongQuan { get; set; }

        // Chi tiết các mục
        public HoTroNopThueModel HoTroNopThue { get; set; }
        public DangKyThueModel DangKyThue { get; set; }
        public QuanLyKeKhaiModel QuanLyKeKhai { get; set; }
        public KiemTraTaiCoQuanModel KiemTraTaiCoQuan { get; set; }

        public EnterpriseSupportViewModel()
        {
            ThongKeTongQuan = new ThongKeTongQuanModel();
            HoTroNopThue = new HoTroNopThueModel();
            DangKyThue = new DangKyThueModel();
            QuanLyKeKhai = new QuanLyKeKhaiModel();
            KiemTraTaiCoQuan = new KiemTraTaiCoQuanModel();
        }
    }

    /// <summary>
    /// Model thống kê tổng quan
    /// </summary>
    public class ThongKeTongQuanModel
    {
        public int SoDoanhNghiepDangHoatDong { get; set; }
        public int SoHoSoKeKhaiDaNop { get; set; }
        public int TongSoHoSoKeKhaiPhaiNop { get; set; }
        public int SoQuyetDinhXuPhat { get; set; }
        public int SoHoSoKiemTraHoanThanh { get; set; }
        public int TongSoHoSoKiemTra { get; set; }

        // Calculated properties
        public double TyLeHoSoKeKhaiDaNop
        {
            get { return TongSoHoSoKeKhaiPhaiNop > 0 ? Math.Round((double)SoHoSoKeKhaiDaNop / TongSoHoSoKeKhaiPhaiNop * 100, 1) : 0; }
        }

        public double TyLeKiemTraHoanThanh
        {
            get { return TongSoHoSoKiemTra > 0 ? Math.Round((double)SoHoSoKiemTraHoanThanh / TongSoHoSoKiemTra * 100, 1) : 0; }
        }
    }

    /// <summary>
    /// Model hỗ trợ người nộp thuế
    /// </summary>
    public class HoTroNopThueModel
    {
        public int TongVanBanTiepNhan { get; set; }
        public int VanBanDaGiaiDap { get; set; }
        public int VanBanChuaDenHan { get; set; }
        public int TuVanQuaHeTHongEtax { get; set; }

        public double TyLeGiaiDapDungHan
        {
            get { return TongVanBanTiepNhan > 0 ? Math.Round((double)VanBanDaGiaiDap / TongVanBanTiepNhan * 100, 1) : 0; }
        }

        public string TrangThaiHoTro
        {
            get
            {
                if (TyLeGiaiDapDungHan >= 80) return "Tốt";
                if (TyLeGiaiDapDungHan >= 60) return "Khá";
                return "Cần cải thiện";
            }
        }
    }

    /// <summary>
    /// Model đăng ký thuế
    /// </summary>
    public class DangKyThueModel
    {
        public int SoDoanhNghiepDangHoatDong { get; set; }
        public int SoDoanhNghiep { get; set; }
        public int SoToChucKinhTeVaKhac { get; set; }
        public int SoDoanhNghiepTangTrongKy { get; set; }
        public int SoDoanhNghiepDangKyMoi { get; set; }
        public int SoDoanhNghiepGiaiThe { get; set; }
        public int SoNNTTrangThai03 { get; set; }
        public int SoNNTRaSoatGiaiQuyetDutDiem { get; set; }

        public int DoanhNghiepTangRong
        {
            get { return SoDoanhNghiepDangKyMoi - SoDoanhNghiepGiaiThe; }
        }

        public double TyLeNNTHoanThanhThutuc
        {
            get { return SoNNTTrangThai03 > 0 ? Math.Round((double)SoNNTRaSoatGiaiQuyetDutDiem / SoNNTTrangThai03 * 100, 1) : 0; }
        }
    }

    /// <summary>
    /// Model quản lý kê khai thuế
    /// </summary>
    public class QuanLyKeKhaiModel
    {
        public int SoToKhaiDaNop { get; set; }
        public int TongSoToKhaiPhaiNop { get; set; }
        public int SoToKhaiNopDungHan { get; set; }
        public int SoQuyetDinhXuPhat { get; set; }
        public double TongSoTienPhat { get; set; }
        public double SoTienPhatDaThu { get; set; }

        public double TyLeNopDungHan
        {
            get { return SoToKhaiDaNop > 0 ? Math.Round((double)SoToKhaiNopDungHan / SoToKhaiDaNop * 100, 2) : 0; }
        }

        public double TyLeToKhaiDaNop
        {
            get { return TongSoToKhaiPhaiNop > 0 ? Math.Round((double)SoToKhaiDaNop / TongSoToKhaiPhaiNop * 100, 1) : 0; }
        }

        public double TyLeTienPhatDaThu
        {
            get { return TongSoTienPhat > 0 ? Math.Round(SoTienPhatDaThu / TongSoTienPhat * 100, 2) : 0; }
        }
    }

    /// <summary>
    /// Model kiểm tra tại cơ quan thuế
    /// </summary>
    public class KiemTraTaiCoQuanModel
    {
        public int TongSoDoanhNghiepPheDuyetKiemTra { get; set; }
        public int SoHoSoDaThucHien { get; set; }
        public int SoNNTBiXuPhatVPHC { get; set; }
        public double SoTienXuPhatVPHC { get; set; }

        public double TyLeHoSoHoanThanh
        {
            get { return TongSoDoanhNghiepPheDuyetKiemTra > 0 ? Math.Round((double)SoHoSoDaThucHien / TongSoDoanhNghiepPheDuyetKiemTra * 100, 1) : 0; }
        }

        public double TyLeNNTViPham
        {
            get { return SoHoSoDaThucHien > 0 ? Math.Round((double)SoNNTBiXuPhatVPHC / SoHoSoDaThucHien * 100, 1) : 0; }
        }
    }

    /// <summary>
    /// Model chi tiết chỉ tiêu
    /// </summary>
    public class ChiTieuChiTietModel
    {
        public string TenChiTieu { get; set; }
        public object GiaTri { get; set; }
        public double TyLe { get; set; }
        public string TrangThai { get; set; }
        public string MauSac { get; set; }
        public string Icon { get; set; }
        public string DonViTinh { get; set; }
        public string GhiChu { get; set; }
    }
}