// Yönetici panelindeki çeşitli istatiksel raporların görüntülenmesini sağlar.
// İstatistik kartları, grafikler ve tabloyla farklı amaçlar için için oluşturulan farklı raporlar görüntülenir.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BeyazEsyaE_Ticaret.Controllers
{
    // Rapor işlemleri için controller sınıfı, TemelYoneticiController'dan miras alır.
    public class RaporController : TemelYoneticiController
    {
        ETicaretEntities db = new ETicaretEntities();

        // Rapor sayfasını açmak ve gerekli istatistikleri hesaplamak için kullanılır
        [HttpGet]
        public ActionResult Index(string q, string sortColumn = "", string sortDir = "")
        {
            // Arama ve sıralama işlevleri için gerekli bilgileri ViewBag'e ekler.
            ViewBag.AramaGoster = true;
            ViewBag.AramaURL = Url.Action("Index", "Rapor");

            /* İstatistik kartları */
            // Sistemdeki mevcut müşteri sayısını hesaplar.
            ViewBag.MusteriSayisi = db.tbl_kullanici.Count(kullanici => kullanici.rol_ID == 1);

            // Tüm siaprişlerin toplam tutarını hesaplar.
            ViewBag.ToplamSatisTutari = db.tbl_siparis.Sum(siparis => (decimal?)siparis.toplam_tutar);

            // Toplam tedarik maliyetini ve toplam satış gelirini hesaplar. Bu hesaplamaların farkını alarak kar / zarar durumunu hesaplar.
            var toplamMaliyet = db.tbl_tedarikSiparisDetay.Sum(tedarikSDetay => (decimal?)tedarikSDetay.toplam_maliyet);
            var toplamSatis = db.tbl_siparis.Sum(siparis => (decimal?)siparis.toplam_tutar);
            ViewBag.KarZarar = toplamSatis - toplamMaliyet;

            // Sipariş detay tablosunu ürün ID'sine göre gruplar, her gruptaki adetleri toplayarak tersten sıralar ve en üstteki kaydı alır.
            var enCokSatilan = db.tbl_siparisDetay
                .GroupBy(siparisDetay => siparisDetay.urun_ID)
                .Select(gSiparisDetay => new { UrunID = gSiparisDetay.Key, Adet = gSiparisDetay.Sum(siparisDetay => siparisDetay.adet) })
                .OrderByDescending(x => x.Adet)
                .FirstOrDefault();

            // Eğer satış verisi varsa bulunan en çak satılan ürünün ad ve satış adetlerini Viewbag'e atar
            if (enCokSatilan != null)
            {
                var urunler = db.tbl_urun.FirstOrDefault(urun => urun.urun_ID == enCokSatilan.UrunID);
                ViewBag.EnCokSatilanUrun = urunler != null ? urunler.urun_adi : "Veri yok";
                ViewBag.EnCokSatilanAdet = enCokSatilan.Adet;
            }

            // Satış olmadığı durumda varsyılan verileri ViewBag' atar.
            else
            {
                ViewBag.EnCokSatilanUrun = "Satış yok";
                ViewBag.EnCokSatilanAdet = 0;
            }

            /* Grafikler */
            // Sipariş tablosunu yıl ve ay bazında gruplar; her ay için toplam satış tutarını hesaplar; sonuçları yıl ve aya göre sıralar.
            var aylikSatis = db.tbl_siparis
                .Where(siparis => siparis.siparis_tarihi != null)
                .GroupBy(siparis => new { siparis.siparis_tarihi.Year, siparis.siparis_tarihi.Month })
                .Select(gSiparis => new {
                    Yil = gSiparis.Key.Year,
                    Ay = gSiparis.Key.Month,
                    Tutar = gSiparis.Sum(siparis => siparis.toplam_tutar)
                })
                .OrderBy(x => x.Yil).ThenBy(x => x.Ay)
                .ToList();
            // Aylık satış verilerini JSON formatına dönüştürür.
            ViewBag.AylikSatisJSON = JsonConvert.SerializeObject(aylikSatis);

            // Sipariş detay, ürün ve kategori tablolarını birleştirir; kategori ad, satış adet verilerini alır; kategori adına göre gruplar ve her kategoride satılan ürünleri hesaplar.
            var kategoriSatis = db.tbl_siparisDetay
                .Join(db.tbl_urun, siparisDetay => siparisDetay.urun_ID, urun => urun.urun_ID, (siparisDetay, urun) => new { siparisDetay.adet, urun.kategori_ID })
                .Join(db.tbl_kategori, x => x.kategori_ID, kategori => kategori.kategori_ID, (x, kategori) => new { kategori.kategori_adi, x.adet })
                .GroupBy(x => x.kategori_adi)
                .Select(g => new { Kategori = g.Key, Adet = g.Sum(x => x.adet) })
                .ToList();
            // Kategori bazlı satış verilerini JSON formatına dönüştürür.
            ViewBag.KategoriSatisJSON = JsonConvert.SerializeObject(kategoriSatis);

            /* Tablo */
            // Tedarik sipariş detay, ürün, tedarik siparişleri ve tedarikçi tablolarını birleştirir; her tedarik sipariş detayı için tedarikçi adı, ürün adı, toplam tutar ve tarih bilgilerini alır.
            var tedarikListesi = db.tbl_tedarikSiparisDetay
                .Join(db.tbl_urun, tedarikSDetay => tedarikSDetay.urun_ID, urun => urun.urun_ID, (tedarikSDetay, urun) => new { tedarikSDetay, urun })
                .Join(db.tbl_tedarikSiparisleri, du => du.tedarikSDetay.tedarikSiparis_ID, tedarikSiparis => tedarikSiparis.tedarikSiparis_ID, (du, tedarikSiparis) => new { du, tedarikSiparis })
                .Join(db.tbl_tedarikci, dut => dut.tedarikSiparis.tedarikci_ID, tedarikci => tedarikci.tedarikci_ID, (dut, tedarikci) => new {
                    Tedarikci = tedarikci.tedarikci_adi,
                    UrunAdi = dut.du.urun.urun_adi,
                    ToplamTutar = dut.du.tedarikSDetay.adet * dut.du.tedarikSDetay.birim_maliyet,
                    Tarih = dut.tedarikSiparis.siparis_tarihi
                })
                .AsQueryable();

            // Arama işlemi; tedarikçi ve ürün adına göre arama yapılır.
            if (!string.IsNullOrEmpty(q))
            {
                q = q.Trim().ToUpper();
                tedarikListesi = tedarikListesi.Where(x =>
                    (x.Tedarikci ?? "").ToUpper().Contains(q) ||
                    (x.UrunAdi ?? "").ToUpper().Contains(q));
            }

            // Sıralama işlemi; varsayılan olarak tarihe göre tersten sıralanan tedarik siparişlerini belirtilen sütun ve yöne göre sıralar.
            // Sıralama durumlarını View'a iletmek için ViewBag yapıları kullanılır.
            if (string.IsNullOrEmpty(sortColumn) || string.IsNullOrEmpty(sortDir))
            {
                tedarikListesi = tedarikListesi.OrderByDescending(x => x.Tarih);
            }
            else
            {
                switch (sortColumn)
                {
                    case "tedarikci":
                        tedarikListesi = sortDir == "asc"
                            ? tedarikListesi.OrderBy(x => x.Tedarikci)
                            : tedarikListesi.OrderByDescending(x => x.Tedarikci);
                        break;

                    case "urun":
                        tedarikListesi = sortDir == "asc"
                            ? tedarikListesi.OrderBy(x => x.UrunAdi)
                            : tedarikListesi.OrderByDescending(x => x.UrunAdi);
                        break;

                    case "tutar":
                        tedarikListesi = sortDir == "asc"
                            ? tedarikListesi.OrderBy(x => x.ToplamTutar)
                            : tedarikListesi.OrderByDescending(x => x.ToplamTutar);
                        break;

                    case "tarih":
                        tedarikListesi = sortDir == "asc"
                            ? tedarikListesi.OrderBy(x => x.Tarih)
                            : tedarikListesi.OrderByDescending(x => x.Tarih);
                        break;

                    default:
                        tedarikListesi = tedarikListesi.OrderByDescending(x => x.Tarih);
                        break;
                }
            }

            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDir = sortDir;
            
            // Tedarik lsitesi verilerini HSON formatına dönüştürür.
            ViewBag.TedarikJSON = JsonConvert.SerializeObject(tedarikListesi.ToList());
            return View("~/Views/Yonetici/Rapor/Index.cshtml");
        }
    }
}