// Müşteri panelinin temel giriş noktasıdır.

using BeyazEsyaE_Ticaret.Models;  // ViewModel'ler için
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BeyazEsyaE_Ticaret.Controllers
{
    // Müşteri panelindeki işlemler için ana controller sınıfıdır.
    public class MusteriController : Controller
    {
        ETicaretEntities db = new ETicaretEntities();

        // Müşteri ana sayfasını görüntüler ve filtrelenen ürün kategorilerini listeler.
        [HttpGet]
        public ActionResult Index()
        {
            // Ana sayfada arama kutusunun gösterilmesini engeller.
            ViewBag.AramaGoster = false;

            // Farklı filtrele oluşturulan ürün listelerini View'a taşımayı sağlar.
            var viewModel = new AnaSayfaViewModel();

            // Aktif ürünlerden öne çıkan mı değeri aktif olanların ilk 9 tanesini veri tabanından çeker.
            viewModel.OneCikanlar = db.tbl_urun
                .Where(urun => urun.oneCikan_mi == true && urun.aktif_mi == true)
                .Take(9)
                .ToList();
            
            // Sipariş detay tablosunu ürün ID'sine göre gruplar ve her ürün için satılan toplam adedi hesaplar. Hesaplanan ürünleri tersten sıralayıp en üstten başlayarak aktif 9 ürünü veri tabanından çeker.
            viewModel.CokSatanlar = db.tbl_siparisDetay
                .GroupBy(siparisDetay => siparisDetay.urun_ID)
                .OrderByDescending(gSiparisDetay => gSiparisDetay.Sum(siparisDetay => siparisDetay.adet))
                .Take(9)
                .Select(gSiparisDetay => db.tbl_urun.FirstOrDefault(urun => urun.urun_ID == gSiparisDetay.Key))
                .Where(urun => urun != null && urun.aktif_mi == true)
                .ToList();

            // Kritik stok seviyesi belirlenir. Ürün tablosunda mevcut stoğu kritik seviyenin altında ve aktif olan ilk 9 tanesini veri tabanından çeker.
            int kritikSeviye = 15;
            viewModel.StokAzalanlar = db.tbl_urun
                .Where(urun => urun.mevcut_stok < kritikSeviye &&  urun.aktif_mi == true)
                .Take(9)
                .ToList();

            // Tedarik sipariş ve tedarik sipariş detay tablolarını birleştirerek her ürün için son tedarik tarihini bulur. Bu tarihe göre tesrten sıralanan ve aktif olan ilk 6 ürünü veri tabanından çeker.
            viewModel.YeniGelenler = db.tbl_tedarikSiparisDetay
                .Join(db.tbl_tedarikSiparisleri,
                    tedarikSiparisDetay => tedarikSiparisDetay.tedarikSiparis_ID,
                    tedarikSiparis => tedarikSiparis.tedarikSiparis_ID,
                    (tedarikSiparisDetay, tedarikSiparis)
                    => new { tedarikSiparisDetay.urun_ID, tedarikSiparis.siparis_tarihi })
                .GroupBy(x => x.urun_ID)
                .Select(g => new { urun_ID = g.Key, SonTedarikTarihi = g.Max(x => x.siparis_tarihi) })
                .OrderByDescending(x => x.SonTedarikTarihi)
                .Take(6)
                .Select(x => db.tbl_urun.FirstOrDefault(urun => urun.urun_ID == x.urun_ID && urun.aktif_mi == true))
                .Where(urun => urun != null)
                .ToList();

            return View("~/Views/Musteri/Index.cshtml", viewModel);
        }
    }
}