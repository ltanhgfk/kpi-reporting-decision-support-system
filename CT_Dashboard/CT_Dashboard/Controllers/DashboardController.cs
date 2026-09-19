using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using CT_Dashboard.Filters;
using CT_Dashboard.Models;
using Newtonsoft.Json;

namespace CT_Dashboard.Controllers
{
    //[AuthorizeRole("Admin", "User", "Leader")]
    public class DashboardController : Controller
    {
        private readonly CT_DASHBOARDEntities _db = new CT_DASHBOARDEntities();

        // ─────────────────────────────────────────────────────────────
        //  MAIN ACTION  –  nhận thang / nam từ combobox
        //  URL mặc định: /Dashboard/Dashboard
        //  URL có filter: /Dashboard/Dashboard?thang=3&nam=2025
        // ─────────────────────────────────────────────────────────────
        public ActionResult Dashboard(int? thang = null, int? nam = null)
        {
            // 1. Xác định kỳ hiển thị
            //    Ưu tiên: tham số URL → nếu không có thì lấy kỳ mới nhất trong DB
            int selectedThang, selectedNam;

            if (thang.HasValue && nam.HasValue)
            {
                selectedThang = thang.Value;
                selectedNam   = nam.Value;
            }
            else
            {
                // Lấy kỳ mới nhất đã duyệt từ bảng báo cáo chính (NVDTPC)
                var latest = _db.BAO_CAO
                    .Where(b => b.DON_VI == "NVDTPC" && b.TRANG_THAI_DUYET == "DaDuyet")
                    .OrderByDescending(b => b.NAM)
                    .ThenByDescending(b => b.THANG)
                    .FirstOrDefault();

                // Nếu không có dữ liệu NVDTPC, lấy kỳ mới nhất bất kỳ đơn vị
                if (latest == null)
                {
                    latest = _db.BAO_CAO
                        .Where(b => b.TRANG_THAI_DUYET == "DaDuyet")
                        .OrderByDescending(b => b.NAM)
                        .ThenByDescending(b => b.THANG)
                        .FirstOrDefault();
                }

                selectedNam   = latest?.NAM   ?? DateTime.Now.Year;
                selectedThang = latest?.THANG ?? DateTime.Now.Month;
            }

            ViewBag.CurrentThang = selectedThang;
            ViewBag.CurrentNam   = selectedNam;

            // 2. Menu nhóm chỉ tiêu (không phụ thuộc kỳ)
            var NhomChiTieu = _db.CHI_TIEU
                .Where(b => b.PARENTID == "0" && b.TRANG_THAI == "Active")
                .OrderBy(m => m.VI_TRI)
                .Select(c => new ChiTieuViewModel
                {
                    ID          = c.ID,
                    MaChiTieu   = c.MA_CHI_TIEU,
                    TenChiTieu  = c.TEN_CHI_TIEU,
                    TenDayDu    = c.TEN_DAY_DU,
                    NhomChiTieu = c.PARENTID
                }).ToList();
            ViewBag.NhomChiTieu = NhomChiTieu;

            // 3. Biểu đồ tròn TTND / TTRU
            ViewBag.TTND = BieuDoTronTheoKy("NVDTPC", "All", "DT_TTND", selectedThang, selectedNam);
            ViewBag.TTRU = BieuDoTronTheoKy("NVDTPC", "All", "DT_TTRU", selectedThang, selectedNam);

            // 4. Biểu đồ cột nguồn thu & địa bàn
            ViewBag.ChiTieuAll = BieuDoSoSanhNguonThuTheoKy("NVDTPC", "All", "DT",      selectedThang, selectedNam);
            ViewBag.DonViAll   = BieuDoSoSanhDiaBanTheoKy(  "NVDTPC", "All", "DT_TTND", selectedThang, selectedNam);

            // 5. Danh sách chỉ tiêu từng nhóm
            ViewBag.DN = LietKeDanhSachChiTieuTheoKy("QLDN2", "TTP", "DN", selectedThang, selectedNam);
            ViewBag.HT = LietKeDanhSachChiTieuTheoKy("QLDN2", "TTP", "HT", selectedThang, selectedNam);
            ViewBag.NO = LietKeDanhSachChiTieuTheoKy("QLDN2", "TTP", "NO", selectedThang, selectedNam);
            ViewBag.KT = LietKeDanhSachChiTieuTheoKy("KT1",   "TTP", "KT", selectedThang, selectedNam);
            ViewBag.HD = LietKeDanhSachChiTieuTheoKy("QLDN1", "TTP", "HD", selectedThang, selectedNam);
            ViewBag.CN = LietKeDanhSachChiTieuTheoKy("CNTK",  "TTP", "CN", selectedThang, selectedNam);
            ViewBag.HC = LietKeDanhSachChiTieuTheoKy("VP",    "TTP", "HC", selectedThang, selectedNam);

            // 6. Ngày báo cáo từng đơn vị (theo kỳ được chọn)
            ViewBag.NgayBCQLDN = FormatNgayBaoCaoChoDashboard(GetNgayBaoCaoTheoKy("QLDN2", selectedThang, selectedNam));
            ViewBag.NgayBCKT   = FormatNgayBaoCaoChoDashboard(GetNgayBaoCaoTheoKy("KT1",   selectedThang, selectedNam));
            ViewBag.NgayBCCNTK = FormatNgayBaoCaoChoDashboard(GetNgayBaoCaoTheoKy("CNTK",  selectedThang, selectedNam));
            ViewBag.NgayBCVP   = FormatNgayBaoCaoChoDashboard(GetNgayBaoCaoTheoKy("VP",    selectedThang, selectedNam));

            return View();
        }

        // ─────────────────────────────────────────────────────────────
        //  HELPER: lấy ID báo cáo đã duyệt theo đơn vị + kỳ
        //  CHỈ lấy đúng tháng/năm được chọn – KHÔNG fallback
        //  Nếu không có dữ liệu thì trả về null → hiển thị "Không có dữ liệu"
        // ─────────────────────────────────────────────────────────────
        private BAO_CAO GetBaoCaoTheoKy(string donVi, int thang, int nam)
        {
            var bc = _db.BAO_CAO
                .Where(b => b.DON_VI == donVi
                         && b.NAM   == nam
                         && b.THANG == thang
                         && b.TRANG_THAI_DUYET == "DaDuyet")
                .OrderByDescending(b => b.NGAY_TAO)
                .FirstOrDefault();

            return bc;
        }

        private DateTime? GetNgayBaoCaoTheoKy(string donVi, int thang, int nam)
        {
            return GetBaoCaoTheoKy(donVi, thang, nam)?.NGAY;
        }

        // ─────────────────────────────────────────────────────────────
        //  Biểu đồ tròn (TTND / TTRU) theo kỳ
        // ─────────────────────────────────────────────────────────────
        private BaoCaoViewModel BieuDoTronTheoKy(string donvi, string diaban, string chitieu, int thang, int nam)
        {
            try
            {
                var bc = GetBaoCaoTheoKy(donvi, thang, nam);
                if (bc == null) return null;

                var q = _db.BAO_CAO_CHI_TIET
                    .Include(x => x.BAO_CAO1)
                    .Include(x => x.CHI_TIEU1)
                    .Where(x => x.BAO_CAO == bc.ID
                             && x.CHI_TIEU == chitieu
                             && x.CHI_TIEU1.TRANG_THAI == "Active"
                             && x.CHI_TIEU1.QUAN_TRONG == true);

                if (!string.IsNullOrEmpty(diaban) && diaban != "All")
                    q = q.Where(x => x.DIA_BAN == diaban);

                var list  = q.ToList();
                if (!list.Any()) return null;

                var first = list.First();
                double thucHien = list.Sum(x => (double)(x.THUC_HIEN ?? 0));
                double keHoach  = list.Sum(x => (double)(x.KE_HOACH  ?? 0));

                return new BaoCaoViewModel
                {
                    MaChiTieu       = first.CHI_TIEU,
                    TenChiTieu      = first.CHI_TIEU1?.TEN_CHI_TIEU ?? "",
                    TenDayDu        = first.CHI_TIEU1?.TEN_DAY_DU   ?? "",
                    ThucHien        = thucHien,
                    KeHoach         = keHoach,
                    Conlai          = keHoach - thucHien,
                    CungKy          = list.Sum(x => (double)(x.CUNG_KY ?? 0)),
                    ThucHienTrongKy = list.Sum(x => (double)(x.THUC_HIEN_TRONG_KY ?? 0)),
                    DonViTinh       = first.CHI_TIEU1?.DVT ?? "",
                    NgaySoLieu      = first.BAO_CAO1?.NGAY ?? DateTime.MinValue,
                    Thang           = (int)(first.BAO_CAO1?.THANG ?? 0)
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BieuDoTronTheoKy error: {ex.Message}");
                return null;
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Biểu đồ cột/đường – nguồn thu theo kỳ
        // ─────────────────────────────────────────────────────────────
        private List<BaoCaoViewModel> BieuDoSoSanhNguonThuTheoKy(string donvi, string diaban, string chitieu, int thang, int nam)
        {
            try
            {
                var bc = GetBaoCaoTheoKy(donvi, thang, nam);
                if (bc == null) return new List<BaoCaoViewModel>();

                var q = _db.BAO_CAO_CHI_TIET
                    .Include(b => b.BAO_CAO1)
                    .Include(b => b.CHI_TIEU1)
                    .Where(b => b.BAO_CAO == bc.ID
                             && b.CHI_TIEU1.PARENTID  == chitieu
                             && b.CHI_TIEU1.TRANG_THAI == "Active"
                             && b.CHI_TIEU1.QUAN_TRONG == true
                             && b.CHI_TIEU != "DT_TTND"
                             && b.CHI_TIEU != "DT_TTRU");

                if (!string.IsNullOrEmpty(diaban) && diaban != "All")
                    q = q.Where(b => b.DIA_BAN == diaban);

                return q.OrderBy(x => x.CHI_TIEU1.VI_TRI)
                    .GroupBy(b => b.CHI_TIEU)
                    .Select(g => new BaoCaoViewModel
                    {
                        MaChiTieu  = g.Key,
                        TenChiTieu = g.FirstOrDefault().CHI_TIEU1.TEN_CHI_TIEU ?? "",
                        TenDayDu   = g.FirstOrDefault().CHI_TIEU1.TEN_DAY_DU   ?? "",
                        ThucHien   = (double)g.Sum(x => x.THUC_HIEN ?? 0),
                        KeHoach    = (double)g.Sum(x => x.KE_HOACH  ?? 0),
                        CungKy     = (double)g.Sum(x => x.CUNG_KY   ?? 0),
                        Conlai     = (double)g.Sum(x => (x.KE_HOACH ?? 0) - (x.THUC_HIEN ?? 0)),
                        DonViTinh  = g.FirstOrDefault().CHI_TIEU1.DVT ?? "",
                        NgaySoLieu = g.FirstOrDefault().BAO_CAO1.NGAY ?? DateTime.MinValue,
                        Thang      = (int)(g.FirstOrDefault().BAO_CAO1.THANG ?? 0)
                    }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BieuDoSoSanhNguonThuTheoKy error: {ex.Message}");
                return new List<BaoCaoViewModel>();
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Biểu đồ cột/đường – địa bàn theo kỳ
        //  Sửa lỗi: dùng b.CHI_TIEU == chitieu thay vì b.CHI_TIEU1.MA_CHI_TIEU == chitieu
        // ─────────────────────────────────────────────────────────────
        private List<BaoCaoViewModel> BieuDoSoSanhDiaBanTheoKy(string donvi, string diaban, string chitieu, int thang, int nam)
        {
            try
            {
                var bc = GetBaoCaoTheoKy(donvi, thang, nam);
                if (bc == null) return new List<BaoCaoViewModel>();

                var q = _db.BAO_CAO_CHI_TIET
                    .Include(b => b.BAO_CAO1)
                    .Include(b => b.CHI_TIEU1)
                    .Include(b => b.DIA_BAN1)
                    .Where(b => b.BAO_CAO == bc.ID
                             && b.CHI_TIEU == chitieu          // ← sửa lỗi: so sánh trực tiếp thay vì qua navigation
                             && b.CHI_TIEU1.TRANG_THAI  == "Active"
                             && b.CHI_TIEU1.QUAN_TRONG  == true
                             && b.DIA_BAN1.TRANG_THAI   == "Active"
                             && b.DIA_BAN1.QUAN_TRONG   == true);

                if (!string.IsNullOrEmpty(diaban) && diaban != "All")
                    q = q.Where(b => b.DIA_BAN == diaban);

                return q.OrderBy(x => x.DIA_BAN1.VI_TRI)
                    .GroupBy(b => b.DIA_BAN)
                    .Select(g => new BaoCaoViewModel
                    {
                        MaDiaBan   = g.Key,
                        TenDiaBan  = g.FirstOrDefault().DIA_BAN1.TEN_DIA_BAN ?? "",
                        ThucHien   = (double)g.Sum(x => x.THUC_HIEN ?? 0),
                        KeHoach    = (double)g.Sum(x => x.KE_HOACH  ?? 0),
                        CungKy     = (double)g.Sum(x => x.CUNG_KY   ?? 0),
                        Conlai     = (double)g.Sum(x => (x.KE_HOACH ?? 0) - (x.THUC_HIEN ?? 0)),
                        DonViTinh  = g.FirstOrDefault().CHI_TIEU1.DVT ?? "",
                        NgaySoLieu = g.FirstOrDefault().BAO_CAO1.NGAY ?? DateTime.MinValue,
                        Thang      = (int)(g.FirstOrDefault().BAO_CAO1.THANG ?? 0)
                    }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BieuDoSoSanhDiaBanTheoKy error: {ex.Message}");
                return new List<BaoCaoViewModel>();
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Danh sách chỉ tiêu từng nhóm theo kỳ
        // ─────────────────────────────────────────────────────────────
        private List<BaoCaoViewModel> LietKeDanhSachChiTieuTheoKy(string donvi, string diaban, string nhomChiTieu, int thang, int nam)
        {
            try
            {
                var bc = GetBaoCaoTheoKy(donvi, thang, nam);
                if (bc == null) return new List<BaoCaoViewModel>();

                var q = _db.BAO_CAO_CHI_TIET
                    .Include(b => b.BAO_CAO1)
                    .Include(b => b.CHI_TIEU1)
                    .Where(b => b.BAO_CAO == bc.ID
                             && b.CHI_TIEU1.PARENTID  == nhomChiTieu
                             && b.CHI_TIEU1.TRANG_THAI == "Active"
                             && b.CHI_TIEU1.QUAN_TRONG == true);

                if (!string.IsNullOrEmpty(diaban) && diaban != "All")
                    q = q.Where(b => b.DIA_BAN == diaban);

                var result = q.OrderBy(x => x.CHI_TIEU1.VI_TRI)
                    .GroupBy(b => b.CHI_TIEU)
                    .Select(g => new BaoCaoViewModel
                    {
                        MaChiTieu       = g.Key,
                        TenChiTieu      = g.FirstOrDefault().CHI_TIEU1.TEN_CHI_TIEU  ?? "",
                        TenDayDu        = g.FirstOrDefault().CHI_TIEU1.TEN_DAY_DU    ?? "",
                        DienGiai        = g.FirstOrDefault().DIEN_GIAI               ?? "",
                        ThucHien        = (double)g.Sum(x => x.THUC_HIEN ?? 0),
                        KeHoach         = (double)g.Sum(x => x.KE_HOACH  ?? 0),
                        CungKy          = (double)g.Sum(x => x.CUNG_KY   ?? 0),
                        Conlai          = (double)g.Sum(x => (x.KE_HOACH ?? 0) - (x.THUC_HIEN ?? 0)),
                        ThucHienTrongKy = (double)g.Sum(x => x.THUC_HIEN_TRONG_KY ?? 0),
                        DonViTinh       = g.FirstOrDefault().CHI_TIEU1.DVT ?? "",
                        NgaySoLieu      = g.FirstOrDefault().BAO_CAO1.NGAY ?? DateTime.MinValue,
                        Thang           = (int)(g.FirstOrDefault().BAO_CAO1.THANG ?? 0)
                    }).ToList();

                foreach (var item in result)
                    item.TyLeHoanThanh = item.KeHoach > 0
                        ? Math.Round(item.ThucHien / item.KeHoach * 100, 1) : 0;

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LietKeDanhSachChiTieuTheoKy error: {ex.Message}");
                return new List<BaoCaoViewModel>();
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  UTILS
        // ─────────────────────────────────────────────────────────────
        public string FormatNgayBaoCaoChoDashboard(DateTime? ngay)
        {
            if (ngay.HasValue && ngay.Value != DateTime.MinValue)
                return ngay.Value.ToString("dd/MM/yyyy");
            return "Chưa có dữ liệu";
        }

        // ─────────────────────────────────────────────────────────────
        //  CÁC ACTION GIỮ NGUYÊN (ChiTieuDetail, NhomChiTieuDetail,
        //  DetailDiaBan, DetailChiTieu, GetThongKeTongHop, ...)
        // ─────────────────────────────────────────────────────────────
        public ActionResult ChiTieuDetail(string maChiTieu)
        {
            try
            {
                if (string.IsNullOrEmpty(maChiTieu))
                { ViewBag.ErrorMessage = "Mã chỉ tiêu không hợp lệ"; return View(new List<ChiTieuDetailViewModel>()); }

                var chiTieuInfo = _db.CHI_TIEU.FirstOrDefault(ct => ct.MA_CHI_TIEU == maChiTieu && ct.TRANG_THAI == "Active");
                if (chiTieuInfo == null)
                { ViewBag.ErrorMessage = "Không tìm thấy chỉ tiêu"; return View(new List<ChiTieuDetailViewModel>()); }

                ViewBag.TenChiTieu = chiTieuInfo.TEN_DAY_DU;
                ViewBag.MaChiTieu  = maChiTieu;
                return View(GetChiTietBaoCaoTheoChiTieu(maChiTieu));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Lỗi: {ex.Message}";
                return View(new List<ChiTieuDetailViewModel>());
            }
        }

        public ActionResult ChiTieuDetailDT(string maChiTieu)
        {
            try
            {
                if (string.IsNullOrEmpty(maChiTieu))
                { ViewBag.ErrorMessage = "Mã chỉ tiêu không hợp lệ"; return View(new List<ChiTieuDetailViewModel>()); }

                var chiTieuInfo = _db.CHI_TIEU.FirstOrDefault(ct => ct.MA_CHI_TIEU == maChiTieu && ct.TRANG_THAI == "Active");
                if (chiTieuInfo == null)
                { ViewBag.ErrorMessage = "Không tìm thấy chỉ tiêu"; return View(new List<ChiTieuDetailViewModel>()); }

                ViewBag.TenChiTieu = chiTieuInfo.TEN_DAY_DU;
                ViewBag.MaChiTieu  = maChiTieu;
                return View(GetChiTietBaoCaoTheoChiTieu(maChiTieu));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Lỗi: {ex.Message}";
                return View(new List<ChiTieuDetailViewModel>());
            }
        }

        public ActionResult NhomChiTieuDetail(string nhomChiTieu)
        {
            try
            {
                if (string.IsNullOrEmpty(nhomChiTieu))
                { ViewBag.ErrorMessage = "Mã nhóm chỉ tiêu không hợp lệ"; return View(new List<ChiTieuDetailViewModel>()); }

                var info = _db.CHI_TIEU.FirstOrDefault(ct => ct.MA_CHI_TIEU == nhomChiTieu && ct.TRANG_THAI == "Active");
                if (info == null)
                { ViewBag.ErrorMessage = "Không tìm thấy nhóm chỉ tiêu"; return View(new List<ChiTieuDetailViewModel>()); }

                ViewBag.TenNhomChiTieu = info.TEN_DAY_DU;
                ViewBag.MaNhomChiTieu  = nhomChiTieu;
                return View(GetChiTietBaoCaoTheoNhomChiTieu(nhomChiTieu));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Lỗi: {ex.Message}";
                return View(new List<ChiTieuDetailViewModel>());
            }
        }

        // Giữ nguyên các private methods cũ cho các action detail
        private List<ChiTieuDetailViewModel> GetChiTietBaoCaoTheoChiTieu(string maChiTieu)
        {
            try
            {
                var query = from bcct in _db.BAO_CAO_CHI_TIET
                            join bc in _db.BAO_CAO on bcct.BAO_CAO equals bc.ID
                            join ct in _db.CHI_TIEU on bcct.CHI_TIEU equals ct.MA_CHI_TIEU
                            join db in _db.DIA_BAN on bcct.DIA_BAN equals db.MA_DIA_BAN into dbJoin
                            from db in dbJoin.DefaultIfEmpty()
                            where bcct.CHI_TIEU == maChiTieu
                                  && bc.IS_LATEST == true
                                  && bc.TRANG_THAI_DUYET == "DaDuyet"
                                  && ct.TRANG_THAI == "Active"
                            orderby bc.NAM, bc.THANG ascending, (db != null ? db.VI_TRI : 0)
                            select new ChiTieuDetailViewModel
                            {
                                ID               = bcct.ID,
                                MaChiTieu        = bcct.CHI_TIEU,
                                TenChiTieu       = ct.TEN_CHI_TIEU,
                                TenDayDu         = ct.TEN_DAY_DU,
                                MaDiaBan         = bcct.DIA_BAN ?? "",
                                TenDiaBan        = db != null ? db.TEN_DIA_BAN : bcct.DIA_BAN ?? "Không xác định",
                                DonVi            = bc.DON_VI ?? "",
                                Thang            = bc.THANG ?? 0,
                                Nam              = bc.NAM   ?? 0,
                                NgayBaoCao       = bc.NGAY  ?? DateTime.MinValue,
                                ThucHienTrongKy  = bcct.THUC_HIEN_TRONG_KY ?? 0,
                                ThucHien         = bcct.THUC_HIEN ?? 0,
                                KeHoach          = bcct.KE_HOACH  ?? 0,
                                CungKy           = bcct.CUNG_KY   ?? 0,
                                DonViTinh        = ct.DVT ?? "",
                                GhiChu           = bcct.DIEN_GIAI ?? "",
                                TrangThaiDuyet   = bc.TRANG_THAI_DUYET ?? "",
                                NgayTao          = bc.NGAY_TAO ?? DateTime.MinValue,
                                NguoiTao         = bc.NGUOI_TAO.ToString() ?? ""
                            };
                return query.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetChiTietBaoCaoTheoChiTieu error: {ex.Message}");
                return new List<ChiTieuDetailViewModel>();
            }
        }

        private List<ChiTieuDetailViewModel> GetChiTietBaoCaoTheoNhomChiTieu(string nhomChiTieu)
        {
            try
            {
                var query = from bcct in _db.BAO_CAO_CHI_TIET
                            join bc in _db.BAO_CAO on bcct.BAO_CAO equals bc.ID
                            join ct in _db.CHI_TIEU on bcct.CHI_TIEU equals ct.MA_CHI_TIEU
                            join db in _db.DIA_BAN on bcct.DIA_BAN equals db.MA_DIA_BAN into dbJoin
                            from db in dbJoin.DefaultIfEmpty()
                            where ct.PARENTID == nhomChiTieu
                                  && bc.IS_LATEST == true
                                  && bc.TRANG_THAI_DUYET == "DaDuyet"
                                  && ct.TRANG_THAI == "Active"
                            orderby bc.NAM, bc.THANG descending, ct.VI_TRI, (db != null ? db.VI_TRI : 0)
                            select new ChiTieuDetailViewModel
                            {
                                ID               = bcct.ID,
                                MaChiTieu        = bcct.CHI_TIEU,
                                TenChiTieu       = ct.TEN_CHI_TIEU,
                                TenDayDu         = ct.TEN_DAY_DU,
                                MaDiaBan         = bcct.DIA_BAN ?? "",
                                TenDiaBan        = db != null ? db.TEN_DIA_BAN : bcct.DIA_BAN ?? "Không xác định",
                                DonVi            = bc.DON_VI ?? "",
                                Thang            = bc.THANG ?? 0,
                                Nam              = bc.NAM   ?? 0,
                                NgayBaoCao       = bc.NGAY  ?? DateTime.MinValue,
                                ThucHienTrongKy  = bcct.THUC_HIEN_TRONG_KY ?? 0,
                                ThucHien         = bcct.THUC_HIEN ?? 0,
                                KeHoach          = bcct.KE_HOACH  ?? 0,
                                CungKy           = bcct.CUNG_KY   ?? 0,
                                DonViTinh        = ct.DVT ?? "",
                                GhiChu           = bcct.DIEN_GIAI ?? "",
                                TrangThaiDuyet   = bc.TRANG_THAI_DUYET ?? "",
                                NgayTao          = bc.NGAY_TAO ?? DateTime.MinValue,
                                NguoiTao         = bc.NGUOI_TAO.ToString() ?? ""
                            };
                return query.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetChiTietBaoCaoTheoNhomChiTieu error: {ex.Message}");
                return new List<ChiTieuDetailViewModel>();
            }
        }

        public ActionResult GetThongKeTongHop()
        {
            try
            {
                var thongKe = new
                {
                    TongBaoCao     = _db.BAO_CAO.Count(x => x.IS_LATEST == true),
                    BaoCaoDaDuyet  = _db.BAO_CAO.Count(x => x.TRANG_THAI_DUYET == "DaDuyet" && x.IS_LATEST == true),
                    BaoCaoChoDuyet = _db.BAO_CAO.Count(x => x.TRANG_THAI_DUYET == "TrinhLanhDao" && x.IS_LATEST == true),
                    ChiTieuHienThi = _db.CHI_TIEU.Count(x => x.TRANG_THAI == "Active"),
                    CapNhatCuoi    = _db.BAO_CAO.Where(x => x.IS_LATEST == true).Max(x => (DateTime?)x.NGAY_TAO)
                };
                return Json(thongKe, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
