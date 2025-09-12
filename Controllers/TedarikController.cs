// Yönetici panelindeki tedarik yönetimini sağlar.
// Tedarikçi ve tedarikçi ürünlerini listeleme, stok güncelleme, arama ve sıralama işlevlerini içerir.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BeyazEsyaE_Ticaret.Controllers
{
    // Tedarik yönetimini sağlayan controller sınıfı, TemelYoneticiController'dan miras alır.
    public class TedarikController : TemelYoneticiController
    {
        ETicaretEntities db = new ETicaretEntities();

        // Tedarikçi listeleme sayfasını görüntüler ve listeleme, sıralama, arama işlevleri tanımlanır.
        public ActionResult Index(string q, string sortColumn = "", string sortDir = "")
        {
            // Arama ve sıralama işlevleri için gerekli bilgileri ViewBag'e ekler.
            ViewBag.AramaGoster = true;
            ViewBag.AramaURL = Url.Action("Index", "Tedarik");

            // Veri tabanındaki tedarikçi verilerini sorgu için hazırlar.
            var tedarikciler = db.tbl_tedarikci.AsQueryable();

            // Arama işlemi; tedarikçi adı, telefon, email ve adrese göre arama yapılır
            if (!string.IsNullOrEmpty(q))
            {
                q = q.Trim().ToUpper();
                tedarikciler = tedarikciler.Where(tedarikci =>
                    tedarikci.tedarikci_adi.ToUpper().Contains(q) ||
                    tedarikci.telefon.ToUpper().Contains(q) ||
                    tedarikci.email.ToUpper().Contains(q) ||
                    tedarikci.adres.ToUpper().Contains(q));
            }

            // Sıralama işlemi; varsayılan olarak tedarikçi ID'sine göre sıralanan tedarikçileri belirtilen sütun ve yöne göre sıralar.
            // Sıralama durumlarını View'a iletmek için ViewBag yapıları kullanılır.
            if (string.IsNullOrEmpty(sortColumn) || string.IsNullOrEmpty(sortDir))
            {
                tedarikciler = tedarikciler.OrderBy(tedarikci => tedarikci.tedarikci_ID);
            }
            else if (sortColumn == "tedarikci_adi" && sortDir == "asc")
            {
                tedarikciler = tedarikciler.OrderBy(tedarikci => tedarikci.tedarikci_adi);
            }
            else if (sortColumn == "tedarikci_adi" && sortDir == "desc")
            {
                tedarikciler = tedarikciler.OrderByDescending(tedarikci => tedarikci.tedarikci_adi);
            }
            else if (sortColumn == "telefon" && sortDir == "asc")
            {
                tedarikciler = tedarikciler.OrderBy(tedarikci => tedarikci.telefon);
            }
            else if (sortColumn == "telefon" && sortDir == "desc")
            {
                tedarikciler = tedarikciler.OrderByDescending(tedarikci => tedarikci.telefon);
            }
            else if (sortColumn == "email" && sortDir == "asc")
            {
                tedarikciler = tedarikciler.OrderBy(tedarikci => tedarikci.email);
            }
            else if (sortColumn == "email" && sortDir == "desc")
            {
                tedarikciler = tedarikciler.OrderByDescending(tedarikci => tedarikci.email);
            }
            else if (sortColumn == "adres" && sortDir == "asc")
            {
                tedarikciler = tedarikciler.OrderBy(tedarikci => tedarikci.adres);
            }
            else if (sortColumn == "adres" && sortDir == "desc")
            {
                tedarikciler = tedarikciler.OrderByDescending(tedarikci => tedarikci.adres);
            }

            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDir = sortDir;
            return View("~/Views/Yonetici/Tedarik/Index.cshtml", tedarikciler.ToList());
        }

        // Seçilen tedarikçinin ID'sine göre tedarikçinin sunduğu ürünleri tedarikçi ürünleri sayfasında gösterir.
        [HttpGet]
        public ActionResult TedarikciUrunleri(int tedarikci_id, int[] marka, int? kategori)
        {
            // Veri tabanındaki ürünleri ilgili tablolarla birleştirerek sorgu için hazırlar.
            var urunlerQuery = db.tbl_urun
                .Include("tbl_kategori")
                .Include("tbl_marka")
                .Include("tbl_model")
                .AsQueryable();

            /* Filtrelemeler */
            // Seçilen marka filtrelerine göre ürünleri filtreler.
            if (marka != null && marka.Any())
            {
                urunlerQuery = urunlerQuery.Where(urun => marka.Contains(urun.marka_ID));
            }

            // Seçilen kategori filtresine göre ürünleri filtreler.
            if (kategori.HasValue)
            {
                urunlerQuery = urunlerQuery.Where(urun => urun.kategori_ID == kategori.Value);
            }

            // Ürün listesini alır.
            var urunler = urunlerQuery.ToList();

            var tedarikciler = db.tbl_tedarikci.FirstOrDefault(tedarikci => tedarikci.tedarikci_ID == tedarikci_id);
            if (tedarikciler == null)
                return HttpNotFound();

            ViewBag.TedarikciID = tedarikciler.tedarikci_ID;
            ViewBag.TedarikciAdi = tedarikciler.tedarikci_adi;

            // Her bir ürün tedarik siparişlerini gruplar ve en son birim maliyetini bulur.
            var birimMaliyetler = db.tbl_tedarikSiparisDetay
                .GroupBy(tedarikSDetay => tedarikSDetay.urun_ID)
                .Select(gTedarikSDetay => new
                {
                    urunID = gTedarikSDetay.Key,
                    sonMaliyet = gTedarikSDetay.OrderByDescending(tedarikSDetay => tedarikSDetay.tedarikSiparisDetay_ID).Select(tedarikSDetay => tedarikSDetay.birim_maliyet).FirstOrDefault()
                }).ToDictionary(x => x.urunID, x => x.sonMaliyet);

            ViewBag.BirimMaliyetler = birimMaliyetler;
            ViewBag.Markalar = db.tbl_marka.OrderBy(m => m.marka_adi).ToList();
            ViewBag.Kategoriler = db.tbl_kategori.OrderBy(k => k.kategori_adi).ToList();
            ViewBag.SeciliMarkalar = marka != null ? marka.ToList() : new List<int>();
            ViewBag.SeciliKategori = kategori;


            return View("~/Views/Yonetici/Tedarik/TedarikciUrunleri.cshtml", urunler);
        }

        // Seçilen tedarikçi ve ürün ID'lerine göre stok güncellme formunu kullanıcıya gösterir.
        [HttpGet]
        public ActionResult StokGuncelle(int urun_id, int tedarikci_id)
        {
            // Formda görüntülenecek ilgili ürün ve tüm alt tablolarını birleştirerek veri tabanından çeker.
            var urunler = db.tbl_urun
                .Include("tbl_kategori")
                .Include("tbl_marka")
                .Include("tbl_model")
                .FirstOrDefault(urun => urun.urun_ID == urun_id);

            if (urunler == null)
                return HttpNotFound();

            var tedarikciler = db.tbl_tedarikci.FirstOrDefault(tedarikci => tedarikci.tedarikci_ID == tedarikci_id);
            if (tedarikciler == null)
                return HttpNotFound();

            ViewBag.TedarikciID = tedarikciler.tedarikci_ID;
            ViewBag.TedarikciAdi = tedarikciler.tedarikci_adi;

            // İlgili ürün için son tedarik maliyetini bulur ve ViewBag aracılığıyla View'a taşır.
            var birimMaliyetler = db.tbl_tedarikSiparisDetay
                .Where(tedarikSDetay => tedarikSDetay.urun_ID == urun_id)
                .OrderByDescending(tedarikSDetay => tedarikSDetay.tedarikSiparisDetay_ID)
                .Select(tedarikSDetay => (decimal)tedarikSDetay.birim_maliyet)
                .FirstOrDefault();

            ViewBag.BirimMaliyet = birimMaliyetler;
            return View("~/Views/Yonetici/Tedarik/StokGuncelle.cshtml", urunler);
        }

        // Güncellenen stok bilgisini işleyerek veri tabanında günceller.
        // Başarılı güncelleme sonrası tedarikçi ürünleri listeleme sayfasına yönlendirir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StokGuncelle(int urun_ID, int tedarikci_ID, int? adet)
        {
            var urunler = db.tbl_urun.Find(urun_ID);
            if (urunler == null)
                return HttpNotFound();

            // Birim maliyet değerini ondalık sayıya dönüştürür.
            decimal birim_maliyet;
            if (!decimal.TryParse(Request.Form["birimMaliyet"], out birim_maliyet))
                birim_maliyet = 0;

            // Validasyonlar
            if (adet == null || adet <= 0)
            {
                ModelState.AddModelError("adet", "Adet alanı zorunludur ve 0'dan büyük olmalıdır.");
            }
            if (birim_maliyet <= 0)
            {
                ModelState.AddModelError("birimMaliyet", "Birim maliyet boş bırakılamaz ve 0'dan büyük olmalıdır.");
            }

            // Herhangi bir hata varsa formu tekrar yükler.
            if (!ModelState.IsValid)
            {
                var tedarikciler = db.tbl_tedarikci.FirstOrDefault(tedarikci => tedarikci.tedarikci_ID == tedarikci_ID);
                if (tedarikciler == null)
                    return HttpNotFound();

                ViewBag.TedarikciID = tedarikciler.tedarikci_ID;
                ViewBag.TedarikciAdi = tedarikciler.tedarikci_adi;
                ViewBag.BirimMaliyet = birim_maliyet;

                return View("~/Views/Yonetici/Tedarik/StokGuncelle.cshtml", urunler);
            }

            // Ürünün mevcut stoğunu arttırır ve toplam maliyeti hesaplar.
            urunler.mevcut_stok += (byte)adet;
            decimal toplam_maliyet = adet.Value * birim_maliyet;

            // Yeni bir tedarikçi siparişi oluşturarak veri tabanına kaydeder.
            var yeniSiparisler = new tbl_tedarikSiparisleri
            {
                tedarikci_ID = tedarikci_ID,
                siparis_tarihi = DateTime.Now,
                toplam_tutar = toplam_maliyet
            };
            db.tbl_tedarikSiparisleri.Add(yeniSiparisler);
            db.SaveChanges();

            // Yeni bir tedarikçi sipariş detay kaydı oluşturarak veri tabanına kaydeder.
            var tedarikSDetaylar = new tbl_tedarikSiparisDetay
            {
                tedarikSiparis_ID = yeniSiparisler.tedarikSiparis_ID,
                urun_ID = urun_ID,
                adet = (byte)adet,
                birim_maliyet = birim_maliyet,
                toplam_maliyet = toplam_maliyet
            };
            db.tbl_tedarikSiparisDetay.Add(tedarikSDetaylar);

            db.SaveChanges();
            TempData["SuccessMessage"] = $"'{urunler.urun_adi}' ürününün stok bilgisi {adet} adet eklenerek {urunler.mevcut_stok} adet olarak güncellendi.";
            return RedirectToAction("TedarikciUrunleri", new { tedarikci_id = tedarikci_ID });
        }
    }
}