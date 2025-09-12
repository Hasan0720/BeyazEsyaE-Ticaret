// Yönetici panelindeki ürünlerin yönetimini sağlar.
// Ürün listeleme, ekleme, düzenleme, aktiflik değiştirme, marka / model yönetimi, arama ve sıralama işlevlerini içerir.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BeyazEsyaE_Ticaret.Controllers
{
    // Ürün yönetimini sağlayan controller sınıfı, TemelYoneticiController'dan miras alır.
    public class UrunController : TemelYoneticiController
    {
        ETicaretEntities db = new ETicaretEntities();

        // Ürün ekleme ve düzenleme sayfalarındaki dropdown listeleri dolduran yardımcı metot.
        // Kategori, marka ve model listelerini oluşturarak ViewBag yapılarına atar.
        private void DropDownlariDoldur(int? kategoriID = null, int? markaID = null, int? modelID = null)
        {
            ViewBag.Kategoriler = new SelectList(db.tbl_kategori.ToList(), "kategori_ID", "kategori_adi", kategoriID);
            ViewBag.Markalar = new SelectList(db.tbl_marka.ToList(), "marka_ID", "marka_adi", markaID);

            var modeller = markaID.HasValue && markaID.Value > 0
                ? db.tbl_model.Where(model => model.marka_ID == markaID.Value).ToList()
                : new List<tbl_model>();

            ViewBag.Modeller = new SelectList(modeller, "model_ID", "model_adi", modelID);
        }

        // Ürün listeleme sayfasını görüntüler ve listeleme, sıralama, arama işlevleri tanımlanır.
        public ActionResult Index(string q, string sortColumn = "", string sortDir = "")
        {
            // Arama ve sıralama işlevleri için gerekli bilgileri ViewBag'e ekler.
            ViewBag.AramaGoster = true;
            ViewBag.AramaURL = Url.Action("Index", "Urun");

            // Veri tabanındaki ürünleri ilgili tablolarla birleştirerek sorgu için hazırlar.
            var urunler = db.tbl_urun
                .Include("tbl_kategori")
                .Include("tbl_marka")
                .Include("tbl_model")
                .AsQueryable();

            // Arama işlemi; ürün, kategori ve marka adına göre arama yapılır.
            if (!string.IsNullOrEmpty(q))
            {
                q = q.Trim().ToUpper();
                urunler = urunler.Where(urun =>
                    urun.urun_adi.ToUpper().Contains(q) ||
                    urun.tbl_kategori.kategori_adi.ToUpper().Contains(q) ||
                    urun.tbl_marka.marka_adi.ToUpper().Contains(q));
            }

            // Sıralama işlemi; varsayılan olarak ürün ID'sine göre sıralanan ürünleri belirtilen sütun ve yöne göre sıralar.
            // Sıralama durumlarını View'a iletmek için ViewBag yapıları kullanılır.
            if (string.IsNullOrEmpty(sortColumn) || string.IsNullOrEmpty(sortDir))
            {
                urunler = urunler.OrderBy(urun => urun.urun_ID);
            }
            else
            {
                switch (sortColumn)
                {
                    case "urun_adi":
                        urunler = sortDir == "asc" ? urunler.OrderBy(urun => urun.urun_adi) : urunler.OrderByDescending(urun => urun.urun_adi);
                        break;

                    case "kategori":
                        urunler = sortDir == "asc" ? urunler.OrderBy(urun => urun.tbl_kategori.kategori_adi) : urunler.OrderByDescending(urun => urun.tbl_kategori.kategori_adi);
                        break;

                    case "marka":
                        urunler = sortDir == "asc" ? urunler.OrderBy(urun => urun.tbl_marka.marka_adi) : urunler.OrderByDescending(urun => urun.tbl_marka.marka_adi);
                        break;

                    case "model":
                        urunler = sortDir == "asc" ? urunler.OrderBy(urun => urun.tbl_model.model_adi) : urunler.OrderByDescending(urun => urun.tbl_model.model_adi);
                        break;

                    case "fiyat":
                        urunler = sortDir == "asc" ? urunler.OrderBy(urun => urun.fiyat) : urunler.OrderByDescending(urun => urun.fiyat);
                        break;

                    case "stok":
                        urunler = sortDir == "asc" ? urunler.OrderBy(urun => urun.mevcut_stok) : urunler.OrderByDescending(urun => urun.mevcut_stok);
                        break;

                    case "oneCikan":
                        urunler = sortDir == "asc" ? urunler.OrderBy(urun => urun.oneCikan_mi) : urunler.OrderByDescending(urun => urun.oneCikan_mi);
                        break;

                    case "aktif":
                        urunler = sortDir == "asc" ? urunler.OrderBy(urun => urun.aktif_mi) : urunler.OrderByDescending(urun => urun.aktif_mi);
                        break;
                }
            }

            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDir = sortDir;
            return View("~/Views/Yonetici/Urun/Index.cshtml", urunler.ToList());
        }

        // Yardımcı metot aracılığıyla dropdown listeleri doldurarak ürün ekleme formunu kullanıcıya gösterir.
        [HttpGet]
        public ActionResult UrunEkle()
        {
            DropDownlariDoldur();
            return View("~/Views/Yonetici/Urun/UrunEkle.cshtml");
        }

        // Formdan gönderilen ürün verilerini işleyerek veri tabanına kaydeder.
        // Hata varsa sayfayı tekrar yükler.
        // Başarılı ekleme sonrası listeleme sayfasına yönlendirir.
        [HttpPost]
        public ActionResult UrunEkle(tbl_urun urunEkleme, HttpPostedFileBase gorsel)
        {
            ModelState.Remove("mevcut_stok");
            ModelState.Remove("kategori_ID");
            ModelState.Remove("marka_ID");
            ModelState.Remove("model_ID");
            ModelState.Remove("fiyat");

            // Formda girilen verilerdeki boşlukları kaldırır ve büyük harfe dönüştürür.
            urunEkleme.urun_adi = urunEkleme.urun_adi?.Trim().ToUpper();
            urunEkleme.aciklama = urunEkleme.aciklama?.Trim().ToUpper();

            // Validasyonlar
            if (string.IsNullOrWhiteSpace(urunEkleme.urun_adi))
                ModelState.AddModelError("urun_adi", "Ürün adı zorunludur.");
            else if (urunEkleme.urun_adi.Length < 5)
                ModelState.AddModelError("urun_adi", "Ürün adı en az 5 karakter olmalıdır.");
            else if (urunEkleme.urun_adi.Length > 150)
                ModelState.AddModelError("urun_adi", "Ürün adı en fazla 150 karakter olabilir.");
            if (urunEkleme.kategori_ID <= 0)
                ModelState.AddModelError("kategori_ID", "Kategori seçimi zorunludur.");
            if (urunEkleme.marka_ID <= 0)
                ModelState.AddModelError("marka_ID", "Marka seçimi zorunludur.");
            if (urunEkleme.model_ID <= 0)
                ModelState.AddModelError("model_ID", "Model seçimi zorunludur.");
            if (string.IsNullOrWhiteSpace(urunEkleme.aciklama))
                ModelState.AddModelError("aciklama", "Açıklama zorunludur.");
            if (urunEkleme.fiyat <= 0)
                ModelState.AddModelError("fiyat", "Fiyat zorunludur ve 0'dan büyük olmalıdır.");
            bool gorselVarMi = (gorsel != null && gorsel.ContentLength > 0) || !string.IsNullOrWhiteSpace(urunEkleme.gorsel);
            if (!gorselVarMi)
                ModelState.AddModelError("gorsel", "Ürün görseli zorunludur.");
            if (!ModelState.IsValid)
            {
                DropDownlariDoldur(urunEkleme.kategori_ID, urunEkleme.marka_ID, urunEkleme.model_ID);
                return View("~/Views/Yonetici/Urun/UrunEkle.cshtml", urunEkleme);
            }

            // Aynı kombinasyonla mevut ürün kontrolü
            bool ayniKombinasyonVar = db.tbl_urun.Any(urun =>
                urun.marka_ID == urunEkleme.marka_ID &&
                urun.model_ID == urunEkleme.model_ID
            );
            if (ayniKombinasyonVar)
            {
                TempData["InfoMessage"] = "Bu marka / model kombinasyonunda zaten bir ürün var.";
                DropDownlariDoldur(urunEkleme.kategori_ID, urunEkleme.marka_ID, urunEkleme.model_ID);
                return View("~/Views/Yonetici/Urun/UrunEkle.cshtml", urunEkleme);
            }

            // Yüklenen görselin Uploads klasörüne, yolunun veri tabanına kaydedilir.
            if (gorsel != null && gorsel.ContentLength > 0)
            {
                string dosyaAdi = Path.GetFileName(gorsel.FileName);
                string dosyaYolu = Path.Combine(Server.MapPath("~/Uploads/"), dosyaAdi);
                gorsel.SaveAs(dosyaYolu);
                urunEkleme.gorsel = "/Uploads/" + dosyaAdi;
            }

            // Yeni eklenen ürünün stoğunu 0 şeklinde varsayılan olarak ayarlanarak veri tabanına kaydedilir.
            urunEkleme.mevcut_stok = 0;
            db.tbl_urun.Add(urunEkleme);
            db.SaveChanges();
            TempData["SuccessMessage"] = $"{urunEkleme.urun_adi} adlı yeni ürün başarıyla eklendi.";
            return RedirectToAction("Index");
        }

        // Yardımcı metot aracılığıyla dropdown listeleri doldurarak ürün düzenleme formunu kullanıcıya gösterir.
        [HttpGet]
        public ActionResult UrunDuzenle(int id)
        {
            var urunler = db.tbl_urun.Find(id);
            if (urunler == null)
            {
                return HttpNotFound();
            }
            DropDownlariDoldur(urunler.kategori_ID, urunler.marka_ID, urunler.model_ID);
            return View("~/Views/Yonetici/Urun/UrunDuzenle.cshtml", urunler);
        }

        // Formdan gönderilen ürün verilerini işleyerek veri tabanında günceller.
        // Hata varsa sayfayı tekrar yükler.
        // Başarılı ekleme sonrası listeleme sayfasına yönlendirir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UrunDuzenle(tbl_urun guncelUrun, HttpPostedFileBase gorsel)
        {
            var urunler = db.tbl_urun.Find(guncelUrun.urun_ID);
            if (urunler == null)
            {
                return HttpNotFound();
            }

            ModelState.Remove("mevcut_stok");
            ModelState.Remove("kategori_ID");
            ModelState.Remove("marka_ID");
            ModelState.Remove("model_ID");
            ModelState.Remove("fiyat");

            // Validasyonlar
            if (string.IsNullOrWhiteSpace(guncelUrun.urun_adi))
                ModelState.AddModelError("urun_adi", "Ürün adı zorunludur.");
            else if (guncelUrun.urun_adi.Length < 5)
                ModelState.AddModelError("urun_adi", "Ürün adı en az 5 karakter olmalıdır.");
            else if (guncelUrun.urun_adi.Length > 150)
                ModelState.AddModelError("urun_adi", "Ürün adı en fazla 150 karakter olabilir.");
            if (guncelUrun.kategori_ID <= 0)
                ModelState.AddModelError("kategori_ID", "Kategori seçimi zorunludur.");
            if (guncelUrun.marka_ID <= 0)
                ModelState.AddModelError("marka_ID", "Marka seçimi zorunludur.");
            if (guncelUrun.model_ID <= 0)
                ModelState.AddModelError("model_ID", "Model seçimi zorunludur.");
            if (string.IsNullOrWhiteSpace(guncelUrun.aciklama))
                ModelState.AddModelError("aciklama", "Açıklama zorunludur.");
            if (guncelUrun.fiyat <= 0)
                ModelState.AddModelError("fiyat", "Fiyat zorunludur ve 0'dan büyük olmalıdır.");
            bool gorselVarMi = (gorsel != null && gorsel.ContentLength > 0) || !string.IsNullOrWhiteSpace(guncelUrun.gorsel);
            if (!gorselVarMi)
                ModelState.AddModelError("gorsel", "Ürün görseli zorunludur.");
            if (!ModelState.IsValid)
            {
                DropDownlariDoldur(guncelUrun.kategori_ID, guncelUrun.marka_ID, guncelUrun.model_ID);
                return View("~/Views/Yonetici/Urun/UrunDuzenle.cshtml", guncelUrun);
            }

            /* Durum kontrolleri */
            // Ürünün sipariş veya fatura kayıtlarında yer alması kontrolü
            bool siparisteVar = db.tbl_siparisDetay.Any(siparisDetay => siparisDetay.urun_ID == urunler.urun_ID);
            bool faturadaVar = db.tbl_fatura.Any(fatura => db.tbl_siparisDetay.Any(siparisDetay => siparisDetay.siparis_ID == fatura.siparis_ID && siparisDetay.urun_ID == urunler.urun_ID));
            bool kilitli = siparisteVar || faturadaVar;

            // Değişiklik yapılan alanların belirlenmesi
            bool gorselYeni = gorsel != null && gorsel.ContentLength > 0;
            bool gorselYoluDegisti = !string.IsNullOrWhiteSpace(guncelUrun.gorsel) && guncelUrun.gorsel != urunler.gorsel;
            bool gorselDegisti = gorselYeni || gorselYoluDegisti;
            bool adDegisti = guncelUrun.urun_adi != urunler.urun_adi;
            bool kategoriDegisti = guncelUrun.kategori_ID != urunler.kategori_ID;
            bool markaDegisti = guncelUrun.marka_ID != urunler.marka_ID;
            bool modelDegisti = guncelUrun.model_ID != urunler.model_ID;
            bool aciklamaDegisti = guncelUrun.aciklama != urunler.aciklama;
            bool fiyatDegisti = guncelUrun.fiyat != urunler.fiyat;
            bool oneCikanDegisti = guncelUrun.oneCikan_mi != urunler.oneCikan_mi;
            bool aktifDegisti = guncelUrun.aktif_mi != urunler.aktif_mi;
            bool izinliDegisimVar = aciklamaDegisti || fiyatDegisti || oneCikanDegisti || aktifDegisti;

            // Değişiklik kontrolü
            bool hicDegisimYok = !(adDegisti || kategoriDegisti || markaDegisti || modelDegisti ||
                                   aciklamaDegisti || fiyatDegisti || oneCikanDegisti || aktifDegisti || gorselDegisti);
            if (hicDegisimYok)
            {
                TempData["InfoMessage"] = "Herhangi bir değişiklik yapmadınız.";
                DropDownlariDoldur(urunler.kategori_ID, urunler.marka_ID, urunler.model_ID);
                return View("~/Views/Yonetici/Urun/UrunDuzenle.cshtml", urunler);
            }

            // Aynı kombinasyonla mevut ürün kontrolü
            bool kombinasyonDegisti = kategoriDegisti || markaDegisti || modelDegisti;
            if (kombinasyonDegisti)
            {
                bool ayniKombinasyonVar = db.tbl_urun.Any(urun =>
                    urun.urun_ID != guncelUrun.urun_ID &&
                    urun.marka_ID == guncelUrun.marka_ID &&
                    urun.model_ID == guncelUrun.model_ID
                );
                if (ayniKombinasyonVar)
                {
                    TempData["InfoMessage"] = "Bu marka / model kombinasyonunda başka bir ürün zaten var.";
                    DropDownlariDoldur(guncelUrun.kategori_ID, guncelUrun.marka_ID, guncelUrun.model_ID);
                    return View("~/Views/Yonetici/Urun/UrunDuzenle.cshtml", guncelUrun);
                }
            }

            // Ürün herhangi bir sipariş / fatura kaydında yer alıyorsa temel bilgilerin değiştirilmesini engeller.
            if (kilitli)
            {
                if (adDegisti || kategoriDegisti || markaDegisti || modelDegisti || gorselDegisti)
                {
                    TempData["WarningMessage"] = "Bu ürün sipariş / fatura kayıtlarında bulunduğu için ad, kategori, marka, model ve görseli değiştirilemez.";
                    DropDownlariDoldur(urunler.kategori_ID, urunler.marka_ID, urunler.model_ID);
                    ModelState.Clear();
                    return View("~/Views/Yonetici/Urun/UrunDuzenle.cshtml", urunler);
                }

                // Sadece izin verilen temel bilgilerin dışındaki alanlar güncellenir.
                if (izinliDegisimVar)
                {
                    urunler.aciklama = guncelUrun.aciklama?.Trim().ToUpper();
                    urunler.fiyat = guncelUrun.fiyat;
                    urunler.oneCikan_mi = guncelUrun.oneCikan_mi;
                    urunler.aktif_mi = guncelUrun.aktif_mi;

                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Ürün bilgileri (sadece izin verilen alanlar) güncellendi.";
                    return RedirectToAction("Index");
                }
            }

            // Ürün nesnesini günceller.
            urunler.urun_adi = guncelUrun.urun_adi?.Trim().ToUpper();
            urunler.kategori_ID = guncelUrun.kategori_ID;
            urunler.marka_ID = guncelUrun.marka_ID;
            urunler.model_ID = guncelUrun.model_ID;
            urunler.aciklama = guncelUrun.aciklama?.Trim().ToUpper();
            urunler.fiyat = guncelUrun.fiyat;
            urunler.oneCikan_mi = guncelUrun.oneCikan_mi;
            urunler.aktif_mi = guncelUrun.aktif_mi;

            // Yeni bir görsel eklendiğinde eski resmi silerek yenisini kaydeder.
            if (gorsel != null && gorsel.ContentLength > 0)
            {
                if (!string.IsNullOrEmpty(urunler.gorsel))
                {
                    string eskiYol = Server.MapPath(urunler.gorsel);
                    if (System.IO.File.Exists(eskiYol))
                    {
                        System.IO.File.Delete(eskiYol);
                    }
                }
                string dosyaAdi = Path.GetFileName(gorsel.FileName);
                string yeniYol = Path.Combine(Server.MapPath("~/Uploads/"), dosyaAdi);
                gorsel.SaveAs(yeniYol);
                urunler.gorsel = "/Uploads/" + dosyaAdi;
            }

            // Herhangi bir hata yoksa veri tabanında güncelleme gerçekleşir.
            db.SaveChanges();
            TempData["SuccessMessage"] = "Ürün bilgileri başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        // Seçilen ürünün ID'si üzerinden ürünün aktiflik durumunu değiştirir.
        [HttpGet]
        public ActionResult AktiflikDegistir(int id)
        {
            var urunler = db.tbl_urun.Find(id);
            if (urunler == null)
            {
                return HttpNotFound();
            }

            // Aktiflik durumunu tersine çevir
            urunler.aktif_mi = !urunler.aktif_mi;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Markayı veri tabanına kaydetmek için kullanılan AJAX metodu
        // Başarılı ekleme sonrası marka bilgileri JSON formatında döner
        [HttpPost]
        public ActionResult MarkaEkleAjax(string markaAdi)
        {
            // Girilen marka adındaki boşlukları kaldırır ve büyük harfe dönüştürür.
            markaAdi = markaAdi?.Trim().ToUpper();

            // Validasyon
            if (string.IsNullOrWhiteSpace(markaAdi))
                return Json(new { success = false, type = "danger", message = "Marka adı zorunludur." });

            // Aynı marka kontrolü
            bool varMi = db.tbl_marka.Any(m => m.marka_adi.ToUpper() == markaAdi);
            if (varMi)
                return Json(new { success = false, type = "info", message = "Bu marka zaten mevcut." });

            // Yeni markayı veri tabanına ekler ve JSON formatında döner
            var yeni = new tbl_marka { marka_adi = markaAdi };
            db.tbl_marka.Add(yeni);
            db.SaveChanges();

            return Json(new
            {
                success = true,
                type = "success",
                message = "Marka eklendi.",
                id = yeni.marka_ID,
                ad = yeni.marka_adi
            });
        }

        // Modeli veri tabanına kaydetmek için kullanılan AJAX metodu
        // Başarılı ekleme sonrası model bilgileri JSON formatında döner
        [HttpPost]
        public ActionResult ModelEkleAjax(string modelAdi, int markaID)
        {
            // Girilen marka adındaki boşlukları kaldırır ve büyük harfe dönüştürür.
            modelAdi = modelAdi?.Trim().ToUpper();

            // Validasyonlar
            if (markaID <= 0 || string.IsNullOrWhiteSpace(modelAdi))
                return Json(new { success = false, type = "danger", message = "Marka ve model adı zorunludur." });

            // Aynı model kontrolü
            bool varMi = db.tbl_model.Any(x => x.model_adi.ToUpper() == modelAdi);
            if (varMi)
                return Json(new { success = false, type = "info", message = "Bu model zaten mevcut." });

            // Yeni modeli veri tabanına ekler ve JSON formatında döner
            var yeni = new tbl_model { model_adi = modelAdi, marka_ID = markaID };
            db.tbl_model.Add(yeni);
            db.SaveChanges();

            return Json(new
            {
                success = true,
                type = "success",
                message = "Model eklendi.",
                id = yeni.model_ID,
                ad = yeni.model_adi,
                markaID = yeni.marka_ID
            });
        }

        // Seçilen markaya ait modelleri JSON formatında döndürür
        [HttpGet]
        public JsonResult ModelGetir(int markaID)
        {
            // Marka ID'sine göre modelleri filtreler
            var modeller = db.tbl_model
                             .Where(model => model.marka_ID == markaID)
                             .Select(model => new
                             {
                                 model.model_ID,
                                 model.model_adi
                             }).ToList();

            return Json(modeller, JsonRequestBehavior.AllowGet);
        }

        // AJAX yardımıyla yüklenen ürün görsellerini Uploads klasörüne kaydeder ve görsel yolunu veri tabanına kaydeder
        [HttpPost]
        public ActionResult GorselYukle(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                string dosyaAdi = Path.GetFileName(file.FileName);
                string uzanti = Path.GetExtension(dosyaAdi);
                string yeniAd = Guid.NewGuid().ToString() + uzanti;
                string yol = "/Uploads/" + yeniAd;
                string fizikselYol = Server.MapPath("~" + yol);

                string klasor = Path.GetDirectoryName(fizikselYol);
                if (!Directory.Exists(klasor))
                    Directory.CreateDirectory(klasor);

                file.SaveAs(fizikselYol);
                return Content(yol);
            }

            return new HttpStatusCodeResult(400, "Dosya alınamadı.");
        }
    }
}