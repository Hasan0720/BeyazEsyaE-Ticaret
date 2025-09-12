// Müşteri panelinde ürünleri listeleme, arama, filtreleme (kategori sekmeleri ve popover yapı) ve ürün detaylarını görüntüleme işlemlerini yönetir.

using BeyazEsyaE_Ticaret.Models; // ViewModel'ler için
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BeyazEsyaE_Ticaret.Controllers
{
    // Müşteri tarafındaki ürün işlemlerini kontrol eden controller sınıfıdır.
    public class MusteriUrunController : Controller
    {
        ETicaretEntities db = new ETicaretEntities();

        // Müşteri panelindeki ürün listeleme sayfasını görüntüler ve listeleme, filtreleme işlevleri tanımlanır.
        public ActionResult Index(int? kategoriID, int[] marka, decimal? minFiyat, decimal? maxFiyat)
        {
            // Arama ve sıralama işlevleri için gerekli bilgileri ViewBag'e ekler.
            ViewBag.AramaGoster = true;
            ViewBag.AramaURL = Url.Action("Ara", "MusteriUrun");
            ViewBag.AramaHedef = "#urunKartlar";

            // Veri tabanındaki aktif olan ürünleri listeler.
            var urunler = db.tbl_urun
                .Where(urun => urun.aktif_mi == true)
                .ToList();

            // Veri tabanındaki tüm kategorileri, kategori adına göre sıralayarak listeler.
            var kategoriler = db.tbl_kategori
                .OrderBy(kategori => kategori.kategori_adi)
                .ToList();

            // Veri tabanındaki tüm markaları, marka adına göre sıralayarak listeler.
            var markalar = db.tbl_marka
                .OrderBy(m => m.marka_adi)
                .ToList();

            // Kategori filtresi uygulanmışsa ürünleri kategorilere göre filtreler.
            if (kategoriID != null)
            {
                urunler = urunler
                    .Where(urun => urun.kategori_ID == kategoriID)
                    .ToList();
            }

            // Marka filtresi uygulanmışsa ürünleri markalara göre filtreler.
            if (marka != null && marka.Any())
            {
                urunler = urunler
                    .Where(u => marka.Contains(u.marka_ID))
                    .ToList();
            }

            // Fiyat aralığı filtreleri uygulanmışsa ürünleri minimum ve maksimum fiyat aralığında filtreler.
            if (minFiyat.HasValue)
            {
                urunler = urunler
                    .Where(urun => urun.fiyat >= minFiyat.Value)
                    .ToList();
            }
            if (maxFiyat.HasValue)
            {
                urunler = urunler
                    .Where(urun => urun.fiyat <= maxFiyat.Value)
                    .ToList();
            }

            // Seçilen kategoriyi ve filtrelenecek marka listesini ViewBag'e ekler.
            ViewBag.SecilenKategoriID = kategoriID;
            ViewBag.Markalar = markalar;

            // Ürün ve kategori listelerinin yer aldığı ViewModel'in nesnesini oluşturur.
            var urunModel = new UrunlerViewModel
            {
                Urunler = urunler,
                Kategoriler = kategoriler
            };
            return View("~/Views/Musteri/MusteriUrun/Index.cshtml", urunModel);
        }

        // Seçilen ürün ID'sine göre ürün detay sayfasını görüntüler.
        [HttpGet]
        public ActionResult UrunDetay(int id)
        {
            // Veri tabanında ilgili ürün ID'si ile eşleşen ve aktif olan ürünü bulur.
            var urunler = db.tbl_urun.FirstOrDefault(urun => urun.urun_ID == id && urun.aktif_mi == true);
            if (urunler == null)
            {
                return HttpNotFound();
            }

            // Ürün tablosunun nesnesinin ve ürüne ait alt bilgilerin yer aladığı ViewModel'in nesnesini oluşturur.
            var UrunDetay = new UrunDetayViewModel
            {
                Urun = urunler,
                MarkaAdi = urunler.tbl_marka.marka_adi,
                ModelAdi = urunler.tbl_model.model_adi,
                KategoriAdi = urunler.tbl_kategori.kategori_adi,
                StokAdedi = urunler.mevcut_stok,
            };

            // Detayı gösterilen ürünle aynı kategorideki diğer aktif ürünlerden en son eklenen son 4 tanesini listeler.
            ViewBag.BenzerUrunler = db.tbl_urun
                .Where(urun => urun.kategori_ID == urunler.kategori_ID && urun.urun_ID != urunler.urun_ID && urun.aktif_mi == true)
                .OrderByDescending(urun => urun.urun_ID)
                .Take(4)
                .ToList();
            return View("~/Views/Musteri/MusteriUrun/UrunDetay.cshtml", UrunDetay);
        }

        // Ürün arama işlevi tanımlanır.
        public ActionResult Ara(string q)
        {
            ViewBag.AramaGoster = true;
            ViewBag.AramaURL = Url.Action("Ara", "MusteriUrun");
            ViewBag.AramaHedef = "#urunKartlar";

            // Veri tabanındaki aktif ürün verileri için sorgu hazırlar.
            var urunler = db.tbl_urun.Where(urun => urun.aktif_mi).AsQueryable();

            // Arama işlemi; ürün adına göre arama yapılır.
            if (!string.IsNullOrEmpty(q))
            {
                q = q.Trim().ToUpper();
                urunler = urunler.Where(urun => urun.urun_adi.ToUpper().Contains(q));
            }

            // Filtrelenmiş ürün ve kategori listelerinin yer aldığı ViewModel'in nesnesini oluşturu.
            var urunModel = new UrunlerViewModel
            {
                Urunler = urunler.ToList(),
                Kategoriler = db.tbl_kategori.OrderBy(kategori => kategori.kategori_adi).ToList()
            };

            ViewBag.Markalar = db.tbl_marka.OrderBy(marka => marka.marka_adi).ToList();
            return View("~/Views/Musteri/MusteriUrun/Index.cshtml", urunModel);
        }
    }
}