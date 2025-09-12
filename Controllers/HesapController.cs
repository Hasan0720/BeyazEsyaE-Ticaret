// Müşteri panelindeki hesap yönetimini sağlar.
// Hesap bilgilerini görüntüleme, kişisel bilgileri güncelleme, şifre değiştirme, sipariş geçmişi / sipariş detaylarını inceleme işlevlerini içerir.

using BeyazEsyaE_Ticaret.Models; // ViewModel'ler için
using BeyazEsyaE_Ticaret.Services; // Fatura dışa aktarma iş mantığı servisi için
using iTextSharp.text; // PDF dokümanı oluşturmak için iTextSharp kütüphanesi
using iTextSharp.text.pdf; // PDF özelliklerini kullanmak için iTextSharp kütüphanesi
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BeyazEsyaE_Ticaret.Controllers
{
    // Hesap yönetimini sağlayan controller sınıfı, TemelMusteriController'dan miras alır.
    public class HesapController : TemelMusteriController
    {
        ETicaretEntities db = new ETicaretEntities();

        // Profil bilgileri sayfasını görüntüler.
        public ActionResult Index()
        {
            // Hesap sayfasında arama kutusunun gösterilmesini engeller.
            ViewBag.AramaGoster = false;

            // Oturumdaki kullanıcının ID'sini alır ve veri tabanında kullanıcıyı bulur.
            int kullaniciID = OturumKullaniciID();
            var kullanicilar = db.tbl_kullanici.FirstOrDefault(kullanici => kullanici.kullanici_ID == kullaniciID);
            if (kullanicilar == null)
            {
                return HttpNotFound();
            }

            // Kullanıcı bilgilerinin yer aldığı ViewModel'in nesnesini oluşturur.
            var viewModel = new HesapViewModel
            {
                Ad = kullanicilar.ad,
                Soyad = kullanicilar.soyad,
                Email = kullanicilar.email,
                Telefon = kullanicilar.telefon_no,
                Adres = kullanicilar.adres
            };
            return View("~/Views/Musteri/Hesap/Index.cshtml", viewModel);
        }

        // Şifre değiştirme formunu görüntüler.
        [HttpGet]
        public ActionResult SifreDegistir()
        {
            return View("~/Views/Musteri/Hesap/SifreDegistir.cshtml");
        }

        // Değiştirilen şifre bilgisini veri tabanında günceller.
        // Başarılı güncelleme sonrası profil bilgileri sayfasına yönlendirir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SifreDegistir(SifreDegistirViewModel sifreModel)
        {
            // Herhangi bir hata varsa sayfayı tekrar yükler.
            if (!ModelState.IsValid)
            {
                return View("~/Views/Musteri/Hesap/SifreDegistir.cshtml", sifreModel);
            }

            // Oturumdaki kullanıcının ID'sini alır ve veri tabanında kullanıcıyı bulur.
            int kullaniciID = OturumKullaniciID();
            var kullanicilar = db.tbl_kullanici.Find(kullaniciID);
            if (kullanicilar == null)
                return RedirectToAction("Index");

            // Validasyonlar
            if (sifreModel.EskiSifre != kullanicilar.sifre)
            {
                TempData["WarningMessage"] = "Eski şifreniz hatalı.";
                return View("~/Views/Musteri/Hesap/SifreDegistir.cshtml", sifreModel);
            }
            if (sifreModel.EskiSifre == sifreModel.YeniSifre)
            {
                TempData["InfoMessage"] = "Yeni şifreniz eski şifrenizile aynı olamaz. Lütfen farklı bir şifre giriniz.";
                return View("~/Views/Musteri/Hesap/SifreDegistir.cshtml", sifreModel);
            }

            // Kullanıcının yeni şifresini veri tabanında günceller ve değişiklikleri veri tabanına kaydeder.
            kullanicilar.sifre = sifreModel.YeniSifre;
            db.SaveChanges();
            TempData["SuccessMessage"] = "Şifreniz başarılı bir şekilde güncellenmiştir.";
            return RedirectToAction("Index");
        }

        // Bilgileri güncelleme formunu görüntüler.
        [HttpGet]
        public ActionResult BilgileriGuncelle()
        {
            // Oturumdaki kullanıcının ID'sini alır ve veri tabanında kullanıcıyı bulur.
            int kullaniciID = OturumKullaniciID();
            var kullanicilar = db.tbl_kullanici.Find(kullaniciID);

            // Kullanıcı bilgilerinin yer aldığı ViewModel'in nesnesini oluşturur.
            var viewModel = new HesapViewModel
            {
                Ad = kullanicilar.ad,
                Soyad = kullanicilar.soyad,
                Email = kullanicilar.email,
                Telefon = kullanicilar.telefon_no,
                Adres = kullanicilar.adres
            };
            return View("~/Views/Musteri/Hesap/BilgileriGuncelle.cshtml", viewModel);
        }

        // Değiştirilen kullanıcı bilgilerini veri tabanında günceller.
        // Başarılı güncelleme sonrası profil bilgileri sayfasına yönlendirir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BilgileriGuncelle(HesapViewModel hesapModel)
        {
            // Herhangi bir hata varsa sayfayı tekrar yükler.
            if (!ModelState.IsValid)
            {
                return View("~/Views/Musteri/Hesap/BilgileriGuncelle.cshtml", hesapModel);
            }

            // Oturumdaki kullanıcının ID'sini alır ve veri tabanında kullanıcıyı bulur.
            int kullaniciID = OturumKullaniciID();
            var kullanicilar = db.tbl_kullanici.Find(kullaniciID);
            if (kullanicilar == null)
                return RedirectToAction("Index");

            // Kullanıcıdan gelen verilerdeki boşlukları kaldırır ve ad / soyadı büyük harfe, emaili küçük harfe dönüştürür.
            hesapModel.Ad = hesapModel.Ad?.Trim().ToUpper();
            hesapModel.Soyad = hesapModel.Soyad?.Trim().ToUpper();
            hesapModel.Email = hesapModel.Email?.Trim().ToLower();
            hesapModel.Telefon = hesapModel.Telefon?.Trim();
            hesapModel.Adres = hesapModel.Adres?.Trim();

            // Veri tabanında tekrar eden kayıt kontrolü
            bool emailVarMi = db.tbl_kullanici.Any(kullanıcı => kullanıcı.email == hesapModel.Email && kullanıcı.kullanici_ID != kullaniciID);
            bool telefonVarMi = db.tbl_kullanici.Any(kullanıcı => kullanıcı.telefon_no == hesapModel.Telefon && kullanıcı.kullanici_ID != kullaniciID);
            if (emailVarMi)
            {
                TempData["InfoMessage"] = "Bu email adresi kullanılamaz.";
                hesapModel.Email = kullanicilar.email;
                ModelState.Clear();
                return View("~/Views/Musteri/Hesap/BilgileriGuncelle.cshtml", hesapModel);
            }
            if (telefonVarMi)
            {
                TempData["InfoMessage"] = "Bu telefon numarası kullanılamaz.";
                hesapModel.Telefon = kullanicilar.telefon_no;
                ModelState.Clear();
                return View("~/Views/Musteri/Hesap/BilgileriGuncelle.cshtml", hesapModel);
            }

            // Kullanıcının bilgilerini veri tabanında günceller ve değişiklikleri veri tabanına akydeder.
            kullanicilar.ad = hesapModel.Ad;
            kullanicilar.soyad = hesapModel.Soyad;
            kullanicilar.email = hesapModel.Email;
            kullanicilar.telefon_no = hesapModel.Telefon;
            kullanicilar.adres = hesapModel.Adres;
            db.SaveChanges();
            TempData["SuccessMessage"] = "Bilgileriniz başarılı bir şekilde güncellenmiştir.";
            return RedirectToAction("Index");
        }

        // Geçmiş siparişler sayfasını görüntüler.
        [HttpGet]
        public ActionResult SiparisGecmisi()
        {
            // Oturumdaki kullanıcının ID'sini alır, sepeti bulur ve geçmiş siparişlerle alakalı bilgilerin yer aldığı ViewModel'in nesnesini oluşturur.
            int kullaniciID = OturumKullaniciID();
            var siparisler = db.tbl_siparis
                .Where(siparis => siparis.kullanici_ID == kullaniciID)
                .OrderByDescending(siparis => siparis.siparis_tarihi)
                .Select(siparis => new SiparisGecmisiViewModel
                {
                    SiparisID = siparis.siparis_ID,
                    SiparisTarihi = siparis.siparis_tarihi,
                    ToplamTutar = siparis.toplam_tutar,
                    SiparisDurumu = siparis.tbl_siparisDurum.durum_adi,
                    AliciAdi = siparis.tbl_kullanici.ad + " " + siparis.tbl_kullanici.soyad,
                    ToplamUrunSayisi = (byte)siparis.tbl_siparisDetay.Sum(siparisDetay => siparisDetay.adet),
                    TeslimatSayisi = 1,
                    Gorsel = siparis.tbl_siparisDetay
                        .Select(siparisDetay => siparisDetay.tbl_urun.gorsel)
                        .Distinct()
                        .ToList()
                })
                .ToList();
            return View("~/Views/Musteri/Hesap/SiparisGecmisi.cshtml", siparisler);
        }

        // Seçilen sipariş ID'sine göre siparişin detaylarını görüntüler.
        [HttpGet]
        public ActionResult SiparisDetay(int siparisID)
        {
            // Oturumdaki kullanıcının ID'sini alır, siparişleri bulur ve siparişteki ürün bilgilerinin yer aldığı ViewModel'in nesnesini oluşturur.
            int kullaniciID = OturumKullaniciID();
            var siparisler = db.tbl_siparis.FirstOrDefault(siparis => siparis.siparis_ID == siparisID);
            if (siparisler == null)
                return HttpNotFound();
            var urunler = db.tbl_siparisDetay
                .Where(siparisDetay => siparisDetay.siparis_ID == siparisID)
                .Select(siparisDetay => new SiparisUrunViewModel
                {
                    UrunID = siparisDetay.tbl_urun.urun_ID,
                    UrunAdi = siparisDetay.tbl_urun.urun_adi,
                    Gorsel = siparisDetay.tbl_urun.gorsel,
                    Adet = siparisDetay.adet,
                    BirimFiyat = siparisDetay.birim_fiyat,
                    ToplamFiyat = siparisDetay.birim_fiyat * siparisDetay.adet,
                    Aktiflik = siparisDetay.tbl_urun.aktif_mi
                })
                .ToList();

            // İlgili siparişle ilişkili faturayı bulur.
            int? faturaID = db.tbl_fatura
                .Where(fatura => fatura.siparis_ID == siparisID)
                .Select(fatura => (int?)fatura.fatura_ID)
                .FirstOrDefault();

            // Fiyat hesaplamalarını yapar.
            decimal araToplam = urunler.Sum(siparisUrunModel => siparisUrunModel.ToplamFiyat);
            decimal kdvTutar = Math.Round(araToplam * 0.20m, 2);
            decimal genelToplam = araToplam + kdvTutar;

            // Sipariş detayının yer aldığı ViewModel'in nesnesini oluşturur.
            var viewModel = new SiparisDetayViewModel
            {
                SiparisID = siparisler.siparis_ID,
                FaturaID = faturaID,
                SiparisTarihi = siparisler.siparis_tarihi,
                SiparisDurumu = siparisler.tbl_siparisDurum.durum_adi,
                TeslimatAdresi = siparisler.teslimat_adresi,
                AraToplam = araToplam,
                KdvTutar = kdvTutar,
                GenelToplam = genelToplam,
                Urunler = urunler
            };
            return View("~/Views/Musteri/Hesap/SiparisDetay.cshtml", viewModel);
        }

        // FaturaService sınıfını kullanarak PDF oluşturma işlemini bu sınıfta gerçekleştirir.
        private readonly FaturaService _faturaService = new FaturaService();

        // Seçilen faturanın ID'si üzerinden FaturaService kullanılarak fatura PDF formatında hazırlanır.
        public ActionResult DisaAktar(int id)
        {
            var pdfBytes = _faturaService.FaturaDisaAktar(id);
            if (pdfBytes == null) return HttpNotFound();

            TempData["SuccessMessage"] = "Fatura dışa aktarıldı.";
            // PDF dosyasını kullanıcya indirmek üzere gönderir.
            return File(pdfBytes, "application/pdf", $"Fatura_{id}.pdf");
        }
    }
}