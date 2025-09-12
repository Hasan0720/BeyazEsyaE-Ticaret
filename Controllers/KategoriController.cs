// Yönetici panelindeki ürün kategorilerinin yönetimini sağlar.
// Kategori listeleme, ekleme, düzenleme, arama ve sıralama işlevlerini içerir.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BeyazEsyaE_Ticaret.Controllers
{
    // Kategori yönetimini sağlayan controller sınıfı, TemelYoneticiController'dan miras alır.
    public class KategoriController : TemelYoneticiController
    {
        ETicaretEntities db = new ETicaretEntities();

        // Kategori listeleme sayfasını görüntüler ve listeleme, sıralama, arama işlevleri tanımlanır.
        [HttpGet]
        public ActionResult Index(string q, string sortColumn = "", string sortDir = "")
        {
            // Arama ve sıralama işlevleri için gerekli bilgileri ViewBag'e ekler.
            ViewBag.AramaGoster = true;
            ViewBag.AramaURL = Url.Action("Index", "Kategori");

            // Veri tabanındaki kategori verilerini sorgu için hazırlar.
            var kategoriler = db.tbl_kategori.AsQueryable();

            // Arama işlemi; kategori adına göre arama yapılır
            if (!string.IsNullOrEmpty(q))
            {
                q = q.Trim().ToUpper();
                kategoriler = kategoriler.Where(kategori => kategori.kategori_adi.ToUpper().Contains(q));
            }

            // Sıralama işlemi; varsayılan olarak kategori ID'sine göre sıralanan kategorileri belirtilen sütun ve yöne göre sıralar.
            // Sıralama durumlarını View'a iletmek için ViewBag yapıları kullanılır.
            if (string.IsNullOrEmpty(sortColumn) || string.IsNullOrEmpty(sortDir))
            {
                kategoriler = kategoriler.OrderBy(kategori => kategori.kategori_ID);
            }
            else if (sortColumn == "kategori_adi" && sortDir == "asc")
            {
                kategoriler = kategoriler.OrderBy(kategori => kategori.kategori_adi);
            }
            else if (sortColumn == "kategori_adi" && sortDir == "desc")
            {
                kategoriler = kategoriler.OrderByDescending(kategori => kategori.kategori_adi);
            }

            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDir = sortDir;
            return View("~/Views/Yonetici/Kategori/Index.cshtml", kategoriler.ToList());
        }

        // Kategori ekleme formunu kullanıcıya gösterir.
        [HttpGet]
        public ActionResult KategoriEkle()
        {
            return View("~/Views/Yonetici/Kategori/KategoriEkle.cshtml");
        }

        // Formdan gönderilen kategori verisini işleyerek veri tabanına kaydeder.
        // Hata varsa sayfayı tekrar yükler.
        // Başarılı ekleme sonrası listeleme sayfasına yönlendirir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult KategoriEkle(tbl_kategori kategoriEkleme)
        {
            // Formda girilen verideki boşlukları kaldırır ve büyük harfe dönüştürür.
            kategoriEkleme.kategori_adi = kategoriEkleme.kategori_adi?.Trim().ToUpper();

            // Validasyonlar
            if (string.IsNullOrWhiteSpace(kategoriEkleme.kategori_adi))
                ModelState.AddModelError("kategori_adi", "Kategori adı zorunludur.");
            else if (kategoriEkleme.kategori_adi.Length < 2)
                ModelState.AddModelError("kategori_adi", "Kategori adı en az 2 karakter olmalıdır.");
            else if (kategoriEkleme.kategori_adi.Length > 100)
                ModelState.AddModelError("kategori_adi", "Kategori adı en fazla 100 karakter olabilir.");

            // Veri tabanında tekrar eden kayıt kontrolü
            string kontrolAd = kategoriEkleme.kategori_adi.ToUpper();
            bool adTekrar = db.tbl_kategori.Any(kategori => kategori.kategori_adi.ToUpper() == kontrolAd);
            if (adTekrar)
            {
                TempData["InfoMessage"] = $"{kategoriEkleme.kategori_adi} adında bir kategori zaten mevcut.";
                return View("~/Views/Yonetici/Kategori/KategoriEkle.cshtml", kategoriEkleme);
            }

            // Herhangi bir hata yoksa veri tabanına kayıt gerçekleşir.
            if (ModelState.IsValid)
            {
                db.tbl_kategori.Add(kategoriEkleme);
                db.SaveChanges();
                TempData["SuccessMessage"] = $"{kategoriEkleme.kategori_adi} adlı yeni kategori başarıyla eklendi.";
                return RedirectToAction("Index");
            }
            return View("~/Views/Yonetici/Kategori/KategoriEkle.cshtml", kategoriEkleme);
        }

        // Seçilen kategorinin ID'sine göre kategori düzenleme formunu kullanıcıya gösterir.
        [HttpGet]
        public ActionResult KategoriDuzenle(int id)
        {
            var kategoriler = db.tbl_kategori.Find(id);
            if (kategoriler == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/Yonetici/Kategori/KategoriDuzenle.cshtml", kategoriler);
        }

        // Düzenlenen kategori verisini işleyerek veri tabanında günceller.
        // Hata varsa sayfayı tekrar yükler.
        // Başarılı düzenleme sonrası listeleme sayfasına yönlendirir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult KategoriDuzenle(tbl_kategori guncelKategori)
        {
            // Formda düzenlenen verideki boşlukları kaldırır ve büyük harfe dönüştürür.
            guncelKategori.kategori_adi = guncelKategori.kategori_adi.Trim().ToUpper();

            // Validasyonlar
            if (string.IsNullOrWhiteSpace(guncelKategori.kategori_adi))
                ModelState.AddModelError("kategori_adi", "Kategori adı zorunludur.");
            else if (guncelKategori.kategori_adi.Length < 2)
                ModelState.AddModelError("kategori_adi", "Kategori adı en az 2 karakter olmalıdır.");
            else if (guncelKategori.kategori_adi.Length > 100)
                ModelState.AddModelError("kategori_adi", "Kategori adı en fazla 100 karakter olabilir.");

            // Değişiklik kontrolü
            var mevcutKategori = db.tbl_kategori.Find(guncelKategori.kategori_ID);
            if (mevcutKategori == null)
            {
                return HttpNotFound();
            }
            if (mevcutKategori.kategori_adi == guncelKategori.kategori_adi)
            {
                TempData["InfoMessage"] = "Mevcut kategori adında herhangi bir değişiklik yapmadınız.";
                return View("~/Views/Yonetici/Kategori/KategoriDuzenle.cshtml", guncelKategori);
            }

            // Veri tabanında tekrar eden kayıt kontrolü
            string kontrolAd = guncelKategori.kategori_adi.ToUpper();
            bool adTekrar = db.tbl_kategori.Any(kategori =>
                kategori.kategori_adi.ToUpper() == kontrolAd &&
                kategori.kategori_ID != guncelKategori.kategori_ID);
            if (adTekrar)
            {
                TempData["InfoMessage"] = $"{guncelKategori.kategori_adi} adında bir kategori zaten mevcut.";
                return View("~/Views/Yonetici/Kategori/KategoriDuzenle.cshtml", guncelKategori);
            }

            // Kategoriye bağlı ürün kontrolü
            bool kategoriAltindaUrunVar = db.tbl_urun.Any(urun => urun.kategori_ID == guncelKategori.kategori_ID);
            if (kategoriAltindaUrunVar)
            {
                TempData["WarningMessage"] = "Bu kategoriye bağlı ürünler bulunduğu için kategori güncellenemez.";
                ModelState.Clear();
                return View("~/Views/Yonetici/Kategori/KategoriDuzenle.cshtml", mevcutKategori);
            }

            // Herhangi bir hata yoksa veri tabanında güncelleme gerçekleşir.
            if (ModelState.IsValid)
            {
                var kategoriDuzenleme = db.tbl_kategori.Find(guncelKategori.kategori_ID);
                if (kategoriDuzenleme == null)
                {
                    return HttpNotFound();
                }
                kategoriDuzenleme.kategori_adi = guncelKategori.kategori_adi;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Kategori başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            return View("~/Views/Yonetici/Kategori/KategoriDuzenle.cshtml", guncelKategori);
        }
    }
}