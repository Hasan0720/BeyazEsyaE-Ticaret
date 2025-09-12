// Yönetici panelindeki fatura yönetimini sağlar.
// Fatura listeleme, fatura oluşturma, arama ve sıralama işlevlerini içerir.

using BeyazEsyaE_Ticaret.Models; // ViewModel'ler için
using BeyazEsyaE_Ticaret.Services; // Fatura dışa aktarma iş mantığı servisi için
using iTextSharp.text; // PDF dokümanı oluşturmak için iTextSharp kütüphanesi
using iTextSharp.text.pdf; // PDF özelliklerini kullanmak için iTextSharp kütüphanesi
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BeyazEsyaE_Ticaret.Controllers
{
    // Fatura yönetimini sağlayan controller sınıfı, TemelYoneticiController'dan miras alır.
    public class FaturaController : TemelYoneticiController
    {
        ETicaretEntities db = new ETicaretEntities();

        // Fatura listeleme sayfasını görüntüler ve listeleme, sıralama, arama işlevleri tanımlanır.
        public ActionResult Index(string q, string sortColumnF = "", string sortDirF = "", string sortColumnO = "", string sortDirO = "")
        {
            // Arama ve sıralama işlevleri için gerekli bilgileri ViewBag'e ekler.
            ViewBag.AramaGoster = true;
            ViewBag.AramaURL = Url.Action("Index", "Fatura");

            // Veri tabanındaki fatura verilerini sorgu için hazırlar.
            var faturalar = db.tbl_fatura.AsQueryable();

            // Oluşturulmuş faturaları sıralama işlemi; varsayılan olarak fatura ID'sine göre tersten sıralanan faturaları belirtilen sütun ve yöne göre sıralar.
            // Sıralama durumlarını View'a iletmek için ViewBag yapıları kullanılır.
            if (string.IsNullOrEmpty(sortColumnO) || string.IsNullOrEmpty(sortDirO))
            {
                faturalar = faturalar.OrderByDescending(fatura => fatura.fatura_ID);
            }
            else
            {
                switch (sortColumnO)
                {
                    case "fatura_tarihi":
                        faturalar = sortDirO == "asc"
                            ? faturalar.OrderBy(fatura => fatura.fatura_tarihi)
                            : faturalar.OrderByDescending(fatura => fatura.fatura_tarihi);
                        break;

                    case "toplam_tutar":
                        faturalar = sortDirO == "asc"
                            ? faturalar.OrderBy(fatura => fatura.toplam_tutar)
                            : faturalar.OrderByDescending(fatura => fatura.toplam_tutar);
                        break;

                    default:
                        faturalar = faturalar.OrderByDescending(fatura => fatura.fatura_ID);
                        break;
                }
            }

            // Tüm siparişlerden sipariş durumu "Teslim Edildi" olanları sorgu için hazırlar.
            var faturalanmayanlar = db.tbl_siparis
                .Where(siparis => !db.tbl_fatura.Select(fatura => fatura.siparis_ID).Contains(siparis.siparis_ID)
                         && siparis.tbl_siparisDurum.durum_adi == "Teslim Edildi")
                .AsQueryable();

            // Arama işlemi; faturalanmamış siparişler tablosundaki kullanıcı ad, soyad ve teslimat adresine göre arama yapılır.
            if (!string.IsNullOrEmpty(q))
            {
                q = q.Trim().ToUpper();
                faturalanmayanlar = faturalanmayanlar.Where(siparis =>
                    siparis.tbl_kullanici.ad.ToUpper().Contains(q) ||
                    siparis.tbl_kullanici.soyad.ToUpper().Contains(q) ||
                    siparis.teslimat_adresi.ToUpper().Contains(q));
            }

            // Faturası oluşturulmamış siparişleri sıralama işlemi; varsayılan olarak sipariş ID'sine göre tersten sıralanan siparişleri belirtilen sütun ve yöne göre sıralar.
            // Sıralama durumlarını View'a iletmek için ViewModel sınıfı kullanılır.
            if (string.IsNullOrEmpty(sortColumnF) || string.IsNullOrEmpty(sortDirF))
            {
                faturalanmayanlar = faturalanmayanlar.OrderByDescending(siparis => siparis.siparis_ID);
            }
            else
            {
                switch (sortColumnF)
                {
                    case "musteri":
                        faturalanmayanlar = sortDirF == "asc"
                            ? faturalanmayanlar.OrderBy(siparis => siparis.tbl_kullanici.ad).ThenBy(siparis => siparis.tbl_kullanici.soyad)
                            : faturalanmayanlar.OrderByDescending(siparis => siparis.tbl_kullanici.ad).ThenByDescending(siparis => siparis.tbl_kullanici.soyad);
                        break;

                    case "toplam_tutar":
                        faturalanmayanlar = sortDirF == "asc"
                            ? faturalanmayanlar.OrderBy(siparis => siparis.toplam_tutar)
                            : faturalanmayanlar.OrderByDescending(siparis => siparis.toplam_tutar);
                        break;

                    case "teslimat_adresi":
                        faturalanmayanlar = sortDirF == "asc"
                            ? faturalanmayanlar.OrderBy(siparis => siparis.teslimat_adresi)
                            : faturalanmayanlar.OrderByDescending(siparis => siparis.teslimat_adresi);
                        break;

                    case "tarih":
                        faturalanmayanlar = sortDirF == "asc"
                            ? faturalanmayanlar.OrderBy(siparis => siparis.siparis_tarihi)
                            : faturalanmayanlar.OrderByDescending(siparis => siparis.siparis_tarihi);
                        break;

                    default:
                        faturalanmayanlar = faturalanmayanlar.OrderByDescending(siparis => siparis.siparis_ID);
                        break;
                }
            }

            ViewBag.SortColumnO = sortColumnO;
            ViewBag.SortDirO = sortDirO;
            ViewBag.SortColumnF = sortColumnF;
            ViewBag.SortDirF = sortDirF;

            // Faturası oluşturulmamış siparişleri ve oluşturulmuş faturaları listelerini ViewModel sınıfında birleştirilir.
            var model = new FaturaIndexViewModel
            {
                Faturalar = faturalar.ToList(),
                Faturalanmayanlar = faturalanmayanlar.ToList()
            };

            return View("~/Views/Yonetici/Fatura/Index.cshtml", model);
        }

        // Seçilen siparişin ID'sine göre fatura oluşturma sayfasını kullanıcıya gösterir.
        [HttpGet]
        public ActionResult FaturaOlustur(int id)
        {
            var siparisler = db.tbl_siparis.FirstOrDefault(siparis => siparis.siparis_ID == id);
            if (siparisler == null)
                return HttpNotFound();

            var musteri = siparisler.tbl_kullanici;

            // Siparişe ait ürünleri ViewModel sınıfı aracılığıyla çeker.
            var urunler = db.tbl_siparisDetay
                .Where(siparisDetay => siparisDetay.siparis_ID == id)
                .Select(siparisDetay => new FaturaUrunViewModel
                {
                    UrunAdi = siparisDetay.tbl_urun.urun_adi,
                    Adet = siparisDetay.adet,
                    BirimFiyat = siparisDetay.birim_fiyat,
                    ToplamFiyat = siparisDetay.toplam_fiyat
                })
                .ToList();

            // Fatura oluşturma sayfasında kullanılacak ürün ViewModel sınıfını, sipariş ve kullanıcı bilgilerini oluşturur.
            var viewModel = new FaturaOlusturViewModel
            {
                SiparisID = siparisler.siparis_ID,
                SiparisTarihi = siparisler.siparis_tarihi,
                TeslimatAdresi = siparisler.teslimat_adresi,
                ToplamTutar = siparisler.toplam_tutar,
                MusteriAdSoyad = musteri.ad + " " + musteri.soyad,
                Urunler = urunler
            };

            return View("~/Views/Yonetici/Fatura/FaturaOlustur.cshtml", viewModel);
        }

        // Oluşturulan fatura verisini işleyerek veri tabanına kaydeder ve siparişin durumu "Faturalandı" olarak günceller.
        // Fatura oluşturma sonrası listeleme sayfasına yönlendirir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FaturaOlustur(FaturaOlusturViewModel faturaModel)
        {
            var siparisler = db.tbl_siparis.FirstOrDefault(siparis => siparis.siparis_ID == faturaModel.SiparisID);
            if (siparisler == null)
            {
                return HttpNotFound();
            }
            
            // Yeni bir fatura nesnesi oluşturur ve faturayı veri tabanına kaydeder.
            tbl_fatura yeniFatura = new tbl_fatura
            {
                siparis_ID = faturaModel.SiparisID,
                toplam_tutar = siparisler.toplam_tutar,
                fatura_tarihi = DateTime.Now
            };
            db.tbl_fatura.Add(yeniFatura);

            // // Siparişin durumunu "Faturalandı" olarak günceller
            if (siparisler != null)
            {
                siparisler.siparisDurum_ID = 4;
            }
            db.SaveChanges();
            TempData["SuccessMessage"] = $"#{siparisler.siparis_ID} ID değerli sipariş için fatura başarıyla oluşturuldu.";
            return RedirectToAction("Index");
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