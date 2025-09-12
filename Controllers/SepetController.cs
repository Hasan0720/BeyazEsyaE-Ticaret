// Müşteri panelindeki sepet yönetimini sağlar.
// Sepeti görüntüleme, sepet ürün ekleme, adet değiştirme, sepetten silme ve fiyat hesaplama işlevlerini içerir.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BeyazEsyaE_Ticaret.Models; // ViewModel'ler için

namespace BeyazEsyaE_Ticaret.Controllers
{
    // Sepet yönetimini sağlayan controller sınıfı, TemelMusteriController'dan miras alır.
    public class SepetController : TemelMusteriController
    {
        ETicaretEntities db = new ETicaretEntities();

        // Sepet sayfasını görüntüler ve oturumdaki kullanıcının sepet içeriğini gösterir.
        [HttpGet]
        public ActionResult Index()
        {
            // Sepet sayfasında arama kutusunun gösterilmesini engeller.
            ViewBag.AramaGoster = false;

            // Oturumdaki kullanıcının ID'sini alır, kullanıcıya ait sepet varsa sepete eklenen ürünleri veri tabanından çeker.
            int kullaniciID = OturumKullaniciID();
            var sepet = db.tbl_sepet.FirstOrDefault(s => s.kullanici_ID == kullaniciID);
            if (sepet == null)
                return View("~/Views/Musteri/Sepet/Index.cshtml", new SepetViewModel());
            var detaylar = db.tbl_sepetDetay
                .Where(sepetDetay => sepetDetay.sepet_ID == sepet.sepet_ID)
                .ToList();

            //Sepetteki ürün bilgilerinin yer aldığı ViewModel'in nesnesini oluşturur.
            var urunler = detaylar.Select(sepetDetay => new SepetUrunViewModel
            {
                UrunID = sepetDetay.urun_ID,
                UrunAdi = sepetDetay.tbl_urun.urun_adi,
                Gorsel = sepetDetay.tbl_urun.gorsel,
                BirimFiyat = sepetDetay.birim_fiyat,
                Adet = sepetDetay.adet,
                StokAdedi = sepetDetay.tbl_urun.mevcut_stok
            }).ToList();
            return View("~/Views/Musteri/Sepet/Index.cshtml", new SepetViewModel { Urunler = urunler });
        }

        // Seçilen sepet ID'sine ait fiyat alanlarını güncelleyen yardımcı metot.
        // Sepeti veri tabanında bulur ve sepetteki tüm ürün detaylarının toplam fiyatını hesaplar.
        private void SepetToplamlariniGuncelle(int sepetID)
        {
            var sepet = db.tbl_sepet.Find(sepetID);
            if (sepet != null)
            {
                decimal? yeniToplamFiyat = db.tbl_sepetDetay
                                            .Where(sepetDetay => sepetDetay.sepet_ID == sepetID)
                                            .Sum(sepetDetay => (decimal?)(sepetDetay.birim_fiyat * sepetDetay.adet));

                // KDV oranını belirleyerek KDV'li toplam tutarı hesaplar.
                decimal kdvOrani = 0.20m;
                decimal kdvliTutar = (yeniToplamFiyat ?? 0) * (1 + kdvOrani);

                // Sepetin toplam fiyatını veri tabanında günceller.
                sepet.toplam_fiyat = kdvliTutar;
                db.SaveChanges();
            }
        }

        // Sepete dinamik olarak ürün ekler ve güncel ürün listesi JSON formtında döner.
        [HttpPost]
        public JsonResult SepeteEkle(int urunId, byte adet)
        {
            int kullaniciID = OturumKullaniciID();
            if (adet < 1) adet = 1;

            // Ürünü veri tabanında bulur, maksimum adet sınırını (stok veya 2) belirler.
            var urun = db.tbl_urun.Find(urunId);
            if (urun == null)
                return Json(new { 
                    basarili = false, mesaj = "Ürün bulunamadı.", mesajTipi = "danger" 
                });
            int maxAdet = urun.mevcut_stok < 2 ? urun.mevcut_stok : 2;
            if (maxAdet <= 0)
                return Json(new { 
                    basarili = false, mesaj = $"'{urun.urun_adi}' adlı ürün stokta yok.", mesajTipi = "warning"
                });
            string mesaj = $"'{urun.urun_adi}' adlı ürün sepete eklendi. Bu üründen sepetinize maksimum {maxAdet} adet eklenebilmektedir.";
            string mesajTipi = "success";

            // Eklenmek istenen adet maksimum adetten fazlaysa adet miktarını sınırlandırır.
            if (adet > maxAdet)
            {
                adet = (byte)maxAdet;
                mesaj = $"'{urun.urun_adi}' adlı ürün için sepetinize maksimum {maxAdet} adet eklenebilmektedir.";
                mesajTipi = "info";
            }

            // Mevcut kullanıcının sepetini bulur eğer sepet yoksa yeni bir sepet oluştururarak veri tabanına kaydeder.
            var sepet = db.tbl_sepet.FirstOrDefault(s => s.kullanici_ID == kullaniciID);
            if (sepet == null)
            {
                sepet = new tbl_sepet { kullanici_ID = kullaniciID, toplam_fiyat = 0 };
                db.tbl_sepet.Add(sepet);
                db.SaveChanges();
            }
            // Sepete eklenmek istenen ürünün mevcut sepette yer alıp almadığını kontrol eder.
            var sepetDetaylar = db.tbl_sepetDetay.FirstOrDefault(sepetDetay => sepetDetay.sepet_ID == sepet.sepet_ID && sepetDetay.urun_ID == urunId);
            if (sepetDetaylar != null)
            {
                // Ürün sepette varsa adedini güncelleyerek sepet detayını günceller ve toplam fiyatı yeniden hesaplar.
                int yeniAdet = sepetDetaylar.adet + adet;
                if (yeniAdet > maxAdet)
                {
                    yeniAdet = maxAdet;
                    mesaj = $"'{urun.urun_adi}' adlı ürün için sepetinize maksimum {maxAdet} adet eklenebilmektedir.";
                    mesajTipi = "info";
                }
                else
                {
                    mesaj = $"'{urun.urun_adi}' adlı ürünün sepet adedi güncellendi. Bu üründen sepetinize maksimum {maxAdet} adet eklenebilmektedir.";
                    mesajTipi = "success";
                }
                sepetDetaylar.adet = (byte)yeniAdet;
                sepetDetaylar.toplam_fiyat = sepetDetaylar.birim_fiyat * sepetDetaylar.adet;
            }
            else
            {
                // Ürün sepette yoksa yeni sepet detayı oluşturur.
                db.tbl_sepetDetay.Add(new tbl_sepetDetay
                {
                    sepet_ID = sepet.sepet_ID,
                    urun_ID = urunId,
                    adet = adet,
                    birim_fiyat = urun.fiyat,
                    toplam_fiyat = urun.fiyat * adet
                });
            }
            
            // Değişiklikleri veri tabanına kaydeder ve sepetin toplam fiyatını güncelleyen yardımcı metodu çağırır.
            db.SaveChanges();
            SepetToplamlariniGuncelle(sepet.sepet_ID);

            // Güncel olarak sepette yer alan ürünlerin listesini JSON formatında hazırlar.
            var urunler = db.tbl_sepetDetay
                .Where(sepetDetay => sepetDetay.sepet_ID == sepet.sepet_ID)
                .ToList()
                .Select(sepetDetay => new
                {
                    sepetDetay.urun_ID,
                    UrunAdi = sepetDetay.tbl_urun.urun_adi,
                    Gorsel = sepetDetay.tbl_urun.gorsel,
                    BirimFiyat = sepetDetay.birim_fiyat,
                    Adet = sepetDetay.adet,
                    KdvTutar = (sepetDetay.birim_fiyat * sepetDetay.adet) * 0.20m,
                    ToplamFiyat = (sepetDetay.birim_fiyat * sepetDetay.adet) * 1.20m
                }).ToList();
            return Json(new { basarili = true, mesaj, mesajTipi, urunler });
        }

        // Sepetteki ürün adetlerini dinamik olarak günceller ve güncel ürün listesi JSON formatında döner.
        [HttpPost]
        public JsonResult GuncelleAdet(int urunId, byte adet)
        {
            if (adet < 1) adet = 1;
            int kullaniciID = OturumKullaniciID();

            // Kullanıcının sepet ve sepet detayını bulur.
            var sepetler = db.tbl_sepet.FirstOrDefault(sepet => sepet.kullanici_ID == kullaniciID);
            if (sepetler == null)
                return Json(new { 
                    basarili = false, mesaj = "Sepet bulunamadı.", mesajTipi = "warning" 
                });
            var sepetDetaylar = db.tbl_sepetDetay.FirstOrDefault(sepetDetay => sepetDetay.sepet_ID == sepetler.sepet_ID && sepetDetay.urun_ID == urunId);
            if (sepetDetaylar == null)
                return Json(new { 
                    basarili = false, mesaj = "Ürün sepetinizde bulunamadı.", mesajTipi = "warning" 
                });

            // Ürün adı ve mevcut stoğu veri tabanında bulur ve maksimum adet sınırını (stok veya 2) belirler.
            var urunAdi = sepetDetaylar.tbl_urun?.urun_adi ?? "Ürün";
            int maxAdet = sepetDetaylar.tbl_urun.mevcut_stok < 2 ? sepetDetaylar.tbl_urun.mevcut_stok : 2;
            string mesaj = $"'{urunAdi}' adlı ürünün adedi {adet} olarak güncellendi. Bu üründen sepetinize maksimum {maxAdet} adet eklenebilmektedir.";
            string mesajTipi = "success";

            // Güncellenmek istenen adet maksimum adetten fazlaysa uyarı mesajı gösterir.
            if (adet > maxAdet)
            {
                adet = (byte)maxAdet;
                return Json(new { 
                    basarili = false, mesaj = $"'{urunAdi}' adlı ürün için maksimum {maxAdet} adet eklenebilir.", mesajTipi = "info" 
                });
            }

            // Sepet detayındaki adet ve toplam fiyat güncellenerek değişiklikleri veri tabanına kaydeder ve sepetin toplam fiyatını güncelleyen yardımcı metodu çağırır.
            sepetDetaylar.adet = adet;
            sepetDetaylar.toplam_fiyat = sepetDetaylar.birim_fiyat * sepetDetaylar.adet;
            db.SaveChanges();
            SepetToplamlariniGuncelle(sepetler.sepet_ID);
            return Json(new { 
                basarili = true, mesaj, mesajTipi 
            });
        }

        // Sepetten dinamik olarak ürün siler ve güncel ürün listesi JSON formatında döner.
        [HttpPost]
        public JsonResult Sil(int urunId)
        {
            int kullaniciID = OturumKullaniciID();

            // Kullanıcının sepetini bulur.
            var sepetler = db.tbl_sepet.FirstOrDefault(sepet => sepet.kullanici_ID == kullaniciID);
            if (sepetler == null)
                return Json(new { 
                    basarili = false, mesaj = "Sepet bulunamadı.", mesajTipi = "warning" 
                });
            string mesaj = "Ürün sepetten kaldırıldı.";
            string mesajTipi = "warning";

            // Sepet detaını bulur ve ilgili ürünü sepetten silerek değişiklikleri veri tabanına kaydeder. Sepetin toplam fiyatını güncelleyen yardımcı metodu çağırır.
            var sepetDetaylar = db.tbl_sepetDetay.FirstOrDefault(sepetDetay => sepetDetay.sepet_ID == sepetler.sepet_ID && sepetDetay.urun_ID == urunId);
            if (sepetDetaylar != null)
            {
                var urunAdi = sepetDetaylar.tbl_urun?.urun_adi ?? "Ürün";
                db.tbl_sepetDetay.Remove(sepetDetaylar);
                db.SaveChanges();
                SepetToplamlariniGuncelle(sepetler.sepet_ID);
                mesaj = $"'{urunAdi}' adlı ürün sepetten kaldırıldı.";
                mesajTipi = "warning";
            }
            else
            {
                mesaj = "Ürün sepetinizde bulunamadı.";
                mesajTipi = "warning";
            }

            // Güncel olarak sepette yer alan ürünlerin listesini JSON formatında hazırlar.
            var yeniDetaylar = db.tbl_sepetDetay
                .Where(sepetDetay => sepetDetay.sepet_ID == sepetler.sepet_ID)
                .Select(sepetDetay => new
                {
                    sepetDetay.urun_ID,
                    UrunAdi = sepetDetay.tbl_urun.urun_adi,
                    Gorsel = sepetDetay.tbl_urun.gorsel,
                    BirimFiyat = sepetDetay.birim_fiyat,
                    Adet = sepetDetay.adet,
                    KdvTutar = (sepetDetay.birim_fiyat * sepetDetay.adet) * 0.20m,
                    ToplamFiyat = (sepetDetay.birim_fiyat * sepetDetay.adet) * 1.20m
                }).ToList();
            return Json(new { basarili = true, mesaj, mesajTipi, sepet = yeniDetaylar });
        }

        // Navbar'da gösterilen sepet rozetindeki adet bilgisini döndğrğr ve adet bilgisi JSON formatında döner
        [HttpGet]
        public JsonResult RozetAdet()
        {
            int kullaniciID = OturumKullaniciID();

            // Kullanıcının sepetini bulur.
            var sepetler = db.tbl_sepet.FirstOrDefault(sepet => sepet.kullanici_ID == kullaniciID);
            if (sepetler == null)
                return Json(new { adet = 0 }, JsonRequestBehavior.AllowGet);

            // Sepetteki tüm ürünlerin toplam adedini hesaplar ve toplam adedi JSON formatında döner.
            int toplamAdet = db.tbl_sepetDetay
                .Where(sepetDetaylar => sepetDetaylar.sepet_ID == sepetler.sepet_ID)
                .Sum(sepetDetay => (int?)sepetDetay.adet) ?? 0;
            return Json(new { adet = toplamAdet }, JsonRequestBehavior.AllowGet);
        }

        // Arayüzde sepeti güncellemek için kullanılır ve sepetteki ürünler JSON formatında döner.
        [HttpGet]
        public JsonResult GetSepetUrunler()
        {
            int kullaniciID = OturumKullaniciID();

            // Kullanıcının sepetini bulur.
            var sepetler = db.tbl_sepet.FirstOrDefault(sepet => sepet.kullanici_ID == kullaniciID);
            if (sepetler == null)
                return Json(new { urunler = new List<object>() }, JsonRequestBehavior.AllowGet);

            // Sepet detalarını JSON formatında hazırlar ve ürün listesi de JSON formatında döner.
            var urunler = db.tbl_sepetDetay
                .Where(sepetDetay => sepetDetay.sepet_ID == sepetler.sepet_ID)
                .Select(sepetDetay => new
                {
                    sepetDetay.urun_ID,
                    UrunAdi = sepetDetay.tbl_urun.urun_adi,
                    Gorsel = sepetDetay.tbl_urun.gorsel,
                    BirimFiyat = sepetDetay.birim_fiyat * sepetDetay.adet,
                    Adet = sepetDetay.adet,
                    KdvTutar = (sepetDetay.birim_fiyat * sepetDetay.adet) * 0.20m,
                    ToplamFiyat = (sepetDetay.birim_fiyat * sepetDetay.adet) * 1.20m,
                    StokAdedi = sepetDetay.tbl_urun.mevcut_stok
                }).ToList();
            return Json(new { urunler }, JsonRequestBehavior.AllowGet);
        }
    }
}