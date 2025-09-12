// Müşteri panelindeki sipariş tamamlama sürecinin yönetimini sağlar.
// Siparişi tamamlama, adres seçimi işlevlerini içerir.

using BeyazEsyaE_Ticaret.Models; // ViewModel'ler için
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BeyazEsyaE_Ticaret.Controllers
{
    // Sipariş tamamlama süreç yönetimini sağlayan controller sınıfı, TemelMusteriController'dan miras alır.
    public class SiparisTamamlamaController : TemelMusteriController
    {
        ETicaretEntities db = new ETicaretEntities();

        // Sipariş tamamlama sayfasını görüntüler ve oturumdaki kullanıcı bilgilerini, sepet içeriğini sipariş tamamlama sayfasına aktarır..
        [HttpGet]
        public ActionResult Index()
        {
            // Sipariş tamamlama sayfasında arama kutusunun gösterilmesini engeller.
            ViewBag.AramaGoster = false;

            // Oturumdaki kullanıcının ID'sini alır, kullanıcı ve kullanıcıya ait sepet verilerini bulur.
            int kullaniciID = OturumKullaniciID();
            var musteri = db.tbl_kullanici.FirstOrDefault(kullanici => kullanici.kullanici_ID == kullaniciID);
            if (musteri == null)
                return RedirectToAction("Index", "Sepet");
            var sepetler = db.tbl_sepet.FirstOrDefault(sepet => sepet.kullanici_ID == kullaniciID);
            if (sepetler == null)
                return RedirectToAction("Index", "Sepet");

            // Sepetteki ürünlerin detaylarını çeker ve siaprişteki ürün bilgilerinin yer aldığı ViewModel'in nesnesini oluşturur.
            var sepetUrunler = db.tbl_sepetDetay
                .Where(sepetDetay => sepetDetay.sepet_ID == sepetler.sepet_ID)
                .Select(sepetDetay => new SiparisUrunViewModel
                {
                    UrunAdi = sepetDetay.tbl_urun.urun_adi,
                    Adet = sepetDetay.adet,
                    BirimFiyat = sepetDetay.tbl_urun.fiyat,
                    ToplamFiyat = sepetDetay.adet * sepetDetay.birim_fiyat
                })
                .ToList();

            // Siparişe dönüşecek ürün listesi ve sipariş bilgilerinin yer aldığı ViewModel'in nesnesini oluşturur.
            var model = new SiparisTamamlamaViewModel
            {
                MusteriAdSoyad = musteri.ad + " " + musteri.soyad,
                Telefon = musteri.telefon_no,
                Adres = musteri.adres,
                Urunler = sepetUrunler,
                ToplamTutar = sepetUrunler.Sum(sUViewModel => sUViewModel.ToplamFiyat),
                KayitliAdresSeciliMi = true // varsayılan
            };
            return View("~/Views/Musteri/SiparisTamamlama/Index.cshtml", model);
        }

        // Tamamlanan sipariş bilgisini işleyerek veri tabanına kaydeder.
        // Başarılı kayıt sonrasında tamamlandı sayfasına yönelndirir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SiparisTamamlamaViewModel viewModel)
        {
            // Oturumdaki kullanıcının ID'sini alır ve sepeti bulur.
            int kullaniciID = OturumKullaniciID();
            var sepetler = db.tbl_sepet.FirstOrDefault(sepet => sepet.kullanici_ID == kullaniciID);
            if (sepetler == null)
                return RedirectToAction("Index", "Sepet");

            // Sepetteki ürünlerin detaylarını çeker, toplam tutarı yeniden hesaplar ve siparişteki ürün bilgilerinin yer aldığı ViewModel'in nesnesini oluşturur.
            viewModel.Urunler = db.tbl_sepetDetay
                .Where(sepetDetay => sepetDetay.sepet_ID == sepetler.sepet_ID)
                .Select(sepetDetay => new SiparisUrunViewModel
                {
                    UrunAdi = sepetDetay.tbl_urun.urun_adi,
                    Adet = sepetDetay.adet,
                    BirimFiyat = sepetDetay.tbl_urun.fiyat,
                    ToplamFiyat = sepetDetay.adet * sepetDetay.birim_fiyat
                })
                .ToList();
            viewModel.ToplamTutar = viewModel.Urunler.Sum(sUViewModel => sUViewModel.ToplamFiyat);

            // Eğer kayıtlı adres seçili değilse yeni adres alanını doğrular.
            if (!viewModel.KayitliAdresSeciliMi)
            {
                viewModel.YeniAdres = viewModel.YeniAdres?.Trim();

                // Validasyonlar
                if (string.IsNullOrWhiteSpace(viewModel.YeniAdres))
                {
                    ModelState.AddModelError("YeniAdres", "Yeni adres zorunludur.");
                }
                else if (viewModel.YeniAdres.Trim().Length < 10)
                {
                    ModelState.AddModelError("YeniAdres", "Yeni adres en az 10 karakter olmalıdır.");
                }
            }

            // Herhangi bir hata varsa sayfayı tekrar yükler.
            if (!ModelState.IsValid)
            {
                return View("~/Views/Musteri/SiparisTamamlama/Index.cshtml", viewModel);
            }

            // Teslimat adresini belirler.
            string teslimatAdresi;
            if (viewModel.KayitliAdresSeciliMi)
            {
                var musteri = db.tbl_kullanici.FirstOrDefault(kullanici => kullanici.kullanici_ID == kullaniciID);
                if (musteri == null || string.IsNullOrWhiteSpace(musteri.adres))
                {
                    ModelState.AddModelError("", "Kayıtlı adresiniz bulunamadı.");
                    return View("~/Views/Musteri/SiparisTamamlama/Index.cshtml", viewModel);
                }
                teslimatAdresi = musteri.adres.Trim();
            }
            else
            {
                teslimatAdresi = viewModel.YeniAdres.Trim();
            }

            // KDV oranını belirler ve sepet detaylarını veri tabanından çeker.
            const decimal kdvOrani = 0.2m;
            var sepetUrunler = db.tbl_sepetDetay
                .Where(sepetDetay => sepetDetay.sepet_ID == sepetler.sepet_ID)
                .ToList();

            // Sipariş için yeni bir sipariş nesnesi oluşturur ve oluşturulan siparişi veri tabanına ekleyerek değişiklikleri kaydeder.
            var siparis = new tbl_siparis
            {
                kullanici_ID = kullaniciID,
                siparisDurum_ID = 1,
                toplam_tutar = sepetUrunler.Sum(sepetDetay => sepetDetay.birim_fiyat * sepetDetay.adet * (1 + kdvOrani)),
                teslimat_adresi = teslimatAdresi,
                siparis_tarihi = DateTime.Now
            };
            db.tbl_siparis.Add(siparis);
            db.SaveChanges();

            // Sepetteki her bir ürün için  sipariş detay nesnesi oluşturur ve oluşturulan sipariş detayını veri tabanına ekleyerek değişiklikleri kaydeder.
            foreach (var urun in sepetUrunler)
            {
                var sipariDetaylar = new tbl_siparisDetay
                {
                    siparis_ID = siparis.siparis_ID,
                    urun_ID = urun.urun_ID,
                    adet = urun.adet,
                    birim_fiyat = urun.birim_fiyat,
                    toplam_fiyat = urun.birim_fiyat * urun.adet
                };
                db.tbl_siparisDetay.Add(sipariDetaylar);
            }
            db.SaveChanges();
            return View("~/Views/Musteri/SiparisTamamlama/Tamamlandi.cshtml");
        }
    }
}