// Yönetici panelindeki siparişlerin yönetimini sağlar.
// Siparpiş listeleme, sipariş durumlarını düzenleme, arama ve sıralama işlevlerini içerir.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace BeyazEsyaE_Ticaret.Controllers
{
    // Sipariş yönetimini sağlayan controller sınıfı, TemelYoneticiController'dan miras alır.
    public class SiparisController : TemelYoneticiController
    {
        ETicaretEntities db = new ETicaretEntities();

        // Sipariş listeleme sayfasını görüntüler ve listeleme, sıralama, arama işlevleri tanımlanır.
        public ActionResult Index(string q, string sortColumnT = "", string sortDirT = "", string sortColumnC = "", string sortDirC = "")
        {
            // Arama ve sıralama işlevleri için gerekli bilgileri ViewBag'e ekler.
            ViewBag.AramaGoster = true;
            ViewBag.AramaURL = Url.Action("Index", "Siparis");

            // Veri tabanındaki tüm siparişleri ilgili tablolarla birleştirerek sorgu için hazırlar.
            var tumSiparisler = db.tbl_siparis
                .Include(siparis => siparis.tbl_kullanici)
                .Include(siparis => siparis.tbl_siparisDurum)
                .Include(siparis => siparis.tbl_siparisDetay.Select(siparisDetay => siparisDetay.tbl_urun))
                .AsQueryable();

            // Arama işlemi; her iki tablodaki kullanıcı ad, soyad, ürün adı ve teslimat adresine göre arama yapılır.
            if (!string.IsNullOrEmpty(q))
            {
                q = q.ToUpper();
                tumSiparisler = tumSiparisler.Where(siparis =>
                    siparis.tbl_kullanici.ad.ToUpper().Contains(q) ||
                    siparis.tbl_kullanici.soyad.ToUpper().Contains(q) ||
                    siparis.tbl_siparisDetay.Any(siparisDetay => siparisDetay.tbl_urun.urun_adi.ToUpper().Contains(q)) ||
                    siparis.teslimat_adresi.ToUpper().Contains(q)
                );
            }

            // Tüm siparişler liste olarak alınır ve durumlarına göre iki ayrı tabloda listelenirler.
            var tumSiparislerList = tumSiparisler.ToList();

            var tamamlanmayanSiparisler = tumSiparislerList
                .Where(siparis => siparis.tbl_siparisDurum.durum_adi != "Teslim Edildi" && siparis.tbl_siparisDurum.durum_adi != "Faturalandı")
                .AsQueryable();

            var tamamlananSiparisler = tumSiparislerList
                .Where(siparis => siparis.tbl_siparisDurum.durum_adi == "Teslim Edildi" || siparis.tbl_siparisDurum.durum_adi == "Faturalandı")
                .AsQueryable();

            // Tamamlanmayan siparişleri sıralama işlemi; varsayılan olarak sipariş ID'sine göre sıralanan siparişleri belirtilen sütun ve yöne göre sıralar.
            // Sıralama durumlarını View'a iletmek için ViewBag yapıları kullanılır.
            if (!string.IsNullOrEmpty(sortColumnT) && !string.IsNullOrEmpty(sortDirT))
            {
                if (sortColumnT == "musteri")
                    tamamlanmayanSiparisler = sortDirT == "asc"
                        ? tamamlanmayanSiparisler.OrderBy(siparis => siparis.tbl_kullanici.ad)
                        : sortDirT == "desc" ? tamamlanmayanSiparisler.OrderByDescending(siparis => siparis.tbl_kullanici.ad)
                        : tamamlanmayanSiparisler.OrderBy(siparis => siparis.siparis_ID);

                else if (sortColumnT == "toplamTutar")
                    tamamlanmayanSiparisler = sortDirT == "asc"
                        ? tamamlanmayanSiparisler.OrderBy(siparis => siparis.toplam_tutar)
                        : sortDirT == "desc" ? tamamlanmayanSiparisler.OrderByDescending(siparis => siparis.toplam_tutar)
                        : tamamlanmayanSiparisler.OrderBy(siparis => siparis.siparis_ID);

                else if (sortColumnT == "teslimatAdresi")
                    tamamlanmayanSiparisler = sortDirT == "asc"
                        ? tamamlanmayanSiparisler.OrderBy(siparis => siparis.teslimat_adresi)
                        : sortDirT == "desc" ? tamamlanmayanSiparisler.OrderByDescending(siparis => siparis.teslimat_adresi)
                        : tamamlanmayanSiparisler.OrderBy(siparis => siparis.siparis_ID);

                else if (sortColumnT == "tarih")
                    tamamlanmayanSiparisler = sortDirT == "asc"
                        ? tamamlanmayanSiparisler.OrderBy(siparis => siparis.siparis_tarihi)
                        : sortDirT == "desc" ? tamamlanmayanSiparisler.OrderByDescending(siparis => siparis.siparis_tarihi)
                        : tamamlanmayanSiparisler.OrderBy(siparis => siparis.siparis_ID);

                else if (sortColumnT == "durum")
                    tamamlanmayanSiparisler = sortDirT == "asc"
                        ? tamamlanmayanSiparisler.OrderBy(siparis => siparis.tbl_siparisDurum.durum_adi)
                        : sortDirT == "desc" ? tamamlanmayanSiparisler.OrderByDescending(siparis => siparis.tbl_siparisDurum.durum_adi)
                        : tamamlanmayanSiparisler.OrderBy(siparis => siparis.siparis_ID);
            }

            // Tamamlanan siparişleri sıralama işlemi; varsayılan olarak sipariş ID'sine göre sıralanan siparişleri belirtilen sütun ve yöne göre sıralar.
            // Sıralama durumlarını View'a iletmek için ViewBag yapıları kullanılır.
            if (!string.IsNullOrEmpty(sortColumnC) && !string.IsNullOrEmpty(sortDirC))
            {
                if (sortColumnC == "musteri")
                    tamamlananSiparisler = sortDirC == "asc"
                        ? tamamlananSiparisler.OrderBy(siparis => siparis.tbl_kullanici.ad)
                        : sortDirC == "desc" ? tamamlananSiparisler.OrderByDescending(siparis => siparis.tbl_kullanici.ad)
                        : tamamlananSiparisler.OrderBy(siparis => siparis.siparis_ID);

                else if (sortColumnC == "toplamTutar")
                    tamamlananSiparisler = sortDirC == "asc"
                        ? tamamlananSiparisler.OrderBy(siparis => siparis.toplam_tutar)
                        : sortDirC == "desc" ? tamamlananSiparisler.OrderByDescending(siparis => siparis.toplam_tutar)
                        : tamamlananSiparisler.OrderBy(siparis => siparis.siparis_ID);

                else if (sortColumnC == "teslimatAdresi")
                    tamamlananSiparisler = sortDirC == "asc"
                        ? tamamlananSiparisler.OrderBy(siparis => siparis.teslimat_adresi)
                        : sortDirC == "desc" ? tamamlananSiparisler.OrderByDescending(siparis => siparis.teslimat_adresi)
                        : tamamlananSiparisler.OrderBy(siparis => siparis.siparis_ID);

                else if (sortColumnC == "tarih")
                    tamamlananSiparisler = sortDirC == "asc"
                        ? tamamlananSiparisler.OrderBy(siparis => siparis.siparis_tarihi)
                        : sortDirC == "desc" ? tamamlananSiparisler.OrderByDescending(siparis => siparis.siparis_tarihi)
                        : tamamlananSiparisler.OrderBy(siparis => siparis.siparis_ID);

                else if (sortColumnC == "durum")
                    tamamlananSiparisler = sortDirC == "asc"
                        ? tamamlananSiparisler.OrderBy(siparis => siparis.tbl_siparisDurum.durum_adi)
                        : sortDirC == "desc" ? tamamlananSiparisler.OrderByDescending(siparis => siparis.tbl_siparisDurum.durum_adi)
                        : tamamlananSiparisler.OrderBy(siparis => siparis.siparis_ID);
            }

            ViewBag.Tamamlanmayanlar = tamamlanmayanSiparisler.ToList();
            ViewBag.Tamamlananlar = tamamlananSiparisler.ToList();
            ViewBag.SortColumnT = sortColumnT;
            ViewBag.SortDirT = sortDirT;
            ViewBag.SortColumnC = sortColumnC;
            ViewBag.SortDirC = sortDirC;
            return View("~/Views/Yonetici/Siparis/Index.cshtml", tumSiparislerList);
        }

        // Seçilen siparişin ID'sine göre sipariş durum düzenleme sayfasını kullanıcıya gösterir.
        [HttpGet]
        public ActionResult SiparisDurumDuzenle (int id)
        {
            // Sayfada görüntülenecek ilgili siparişi ve tüm alt tablolarını birleştirerek veri tabanından çeker.
            var siparisler = db.tbl_siparis
                .Include("tbl_kullanici")
                .Include("tbl_siparisDurum")
                .Include("tbl_siparisDetay")
                .Include("tbl_siparisDetay.tbl_urun")
                .FirstOrDefault(siparis => siparis.siparis_ID == id);

            if (siparisler == null)
            {
                return HttpNotFound();
            }

            // Dropdown listede listelenecek tüm sipariş durumlarını veri tabanından çekerek ViewBag'e atar.
            var tumDurumlar = db.tbl_siparisDurum.OrderBy(siparisDurum => siparisDurum.siparisDurum_ID).ToList();
            ViewBag.TumDurumlar = tumDurumlar;
            return View("~/Views/Yonetici/Siparis/SiparisDurumDuzenle.cshtml", siparisler);
        }

        // Durumu güncellenen sipariş verisini işleyerek veri tabanında günceller.
        // Hata varsa sayfayı tekrar yükler.
        // Başarılı düzenleme sonrası listeleme sayfasına yönlendirir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SiparisDurumDuzenle (int siparis_ID, int yeniDurum_ID)
        {
            var siparisler = db.tbl_siparis.FirstOrDefault(siparis => siparis.siparis_ID == siparis_ID);
            if (siparisler == null)
            {
                return HttpNotFound();
            }

            // Mevcut ve yeni seçilen siapriş durumlarını veri tabanından çeker ve karşılaştırır.
            var mevcutDurum = siparisler.tbl_siparisDurum;
            var yeniDurum = db.tbl_siparisDurum.Find(yeniDurum_ID);

            if (mevcutDurum.siparisDurum_ID == yeniDurum_ID)
            {
                TempData["InfoMessage"] = "Herhangi bir değişiklik yapmadınız.";
                return RedirectToAction("SiparisDurumDuzenle", new { id = siparis_ID });
            }

            siparisler.siparisDurum_ID = yeniDurum_ID;
            db.SaveChanges();
            TempData["SuccessMessage"] = $"Sipariş durumu '{mevcutDurum.durum_adi}' olan {siparis_ID} ID değerli siparişin durumu '{yeniDurum.durum_adi}' olarak güncellendi.";
            return RedirectToAction("Index");
        }

    }
}