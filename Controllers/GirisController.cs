// Sisteme giriş işlemlerinin yönetildiği temel giriş noktasıdır.
// Kullanıcıların giriş, çıkı, kayıt ve şifre sıfırlama işlevlerini içerir.

using BeyazEsyaE_Ticaret.Models;  // ViewModel'ler için
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Net;
using System.Net.Mail; // Email gönderme işlemleri için
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Net.Configuration; // SMTP konfigürasyonu için
using System.Configuration; // Web.config'den ayarları okumak için

namespace BeyazEsyaE_Ticaret.Controllers
{
    // Giriş işlemlerini kontrol eden controller sınıfıdır.
    public class GirisController : Controller
    {
        ETicaretEntities db = new ETicaretEntities();

        private const string beniHatirla_ID = "beniHatirlaKullanici_ID"; // beni hatırla çerezinin adı
        private const int beniHatirlaGun = 3; // beni hatırla çerezinin geçerlilik süresi
        private const int admin_ID = 2; // yönetici rolü ID'si
        private const int tokenGecerlilikSure = 15; // şifre sıfırlama token'ının geçerlilik süresi (dk)

        // Müşteri şifresini unuttuğunda mail üzerinden gönderilecek şifre sıfırlama bağlantısının hazırlandığı yardımcı metot
        private void SifreSifirlamaEmail(string toEmail, string resetURL, string AdSoyad)
        {
            // Email konu başlığı ve içeriği oluşturur.
            var konu = "Şifre Sıfırlama Bağlantısı";
            var icerik = $@"
                <p>Merhaba {(string.IsNullOrWhiteSpace(AdSoyad) ? "kullanıcımız" : AdSoyad)},</p>
                <p>Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayınız:</p>
                <p><a href=""{HttpUtility.HtmlEncode(resetURL)}"" target=""BeyazVitrinLogin"">Şifreyi Sıfırla</a></p>
                <p>Bu bağlantı sınırlı süreyle geçerlidir.</p>
                <p>İyi günler dileriz.</p>";

            // Projenin Web.config dosyasındaki SMTP ayarlarını okur.
            var smtpCfg = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");

            // MailMessage nesnesi üzerinden email mesajı oluşturur.
            using (var mail = new MailMessage(smtpCfg.From, toEmail, konu, icerik))
            {
                mail.IsBodyHtml = true;
                mail.SubjectEncoding = Encoding.UTF8;
                mail.BodyEncoding = Encoding.UTF8;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // SmtpClient nesnesi aracılığıyla email gönderilir.
                using (var smtp = new SmtpClient(smtpCfg.Network.Host, smtpCfg.Network.Port))
                {
                    smtp.EnableSsl = smtpCfg.Network.EnableSsl;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(
                        smtpCfg.Network.UserName,
                        smtpCfg.Network.Password
                    );
                    smtp.Send(mail);
                }
            }
        }

        // Giriş formunu görüntüler eğer önceki girişlerde "Beni Hatırla" çerezi seçilmişse otomatik giriş sağlar.
        [HttpGet]
        public ActionResult Giris(string token = null, int? openReset = null)
        {
            // Giriş sayfasında arama kutusunun gösterilmesini engeller.
            ViewBag.AramaGoster = false;

            // Eğer oturumda "kullaniciID" yoksa çerezdeki ID ile veri tabanındaki kulllanıcıyı bularak ilgili panele yönlendirir.
            if (Session["kullaniciID"] == null)
            {
                var httpCookie = Request.Cookies[beniHatirla_ID];
                if (httpCookie != null && int.TryParse(httpCookie.Value, out int kullaniciId))
                {
                    var kullanicilar = db.tbl_kullanici.Find(kullaniciId);
                    if (kullanicilar != null)
                    {
                        Session["kullaniciID"] = kullanicilar.kullanici_ID;

                        if (kullanicilar.rol_ID == admin_ID)
                        {
                            return RedirectToAction("Index", "Rapor");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Musteri");
                        }
                    }
                }
            }

            // URL'de şifre sıfırlama token'ı varsa, bunu view'e iletir.
            ViewBag.ResetToken = token;

            /// Şifre sıfırlama modalını açmak için bir bayrak belirler.
            ViewBag.OpenReset = true;
            return View("~/Views/Musteri/Giris/Giris.cshtml");
        }

        // Kullanıcının email ve şifre bilgilerini doğrular ve role göre yönlendirme yapar.
        // "Beni Hatırla" seçeneği işaretliyse çerez oluşturur.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Giris(GirisViewModel girisModel)
        {
            // ViewModel'deki validasyonlara uyulmazsa sayfayı tekrar yükler.
            if (!ModelState.IsValid)
            {
                return View("~/Views/Musteri/Giris/Giris.cshtml", girisModel);
            }

            // Email adresini standartlaştırmak için boşlukları kaldırır ve küçük harfe dönüştürür.
            girisModel.Email = girisModel.Email?.Trim().ToLower();

            // Veri tabanında eşleşen email ve şifreye sahip kullanıcıyı bulur ve oturumu başlatır.
            var kullanicilar = db.tbl_kullanici.FirstOrDefault(kullanici => kullanici.email == girisModel.Email);
            if (kullanicilar != null && kullanicilar.sifre == girisModel.Sifre)
            {
                Session["kullaniciID"] = kullanicilar.kullanici_ID;

                // "Beni" Hatırla" seçeneği işaretliyse
                if (girisModel.BeniHatirla)
                {
                    // Yeni bir Http çerezi oluşturur ve çerezin geçerlilik süresini ayarlayarak tarayıcıya ekler.
                    var httpCookie = new HttpCookie(beniHatirla_ID, kullanicilar.kullanici_ID.ToString())
                    {
                        Expires = DateTime.Now.AddDays(beniHatirlaGun),
                        HttpOnly = true // JavaScript erişimini engellemek için kullanılır.
                    };
                    Response.Cookies.Add(httpCookie);
                }

                // "Beni" Hatırla" seçeneği işaretli değilse veya daha önce eklenen çerez varsa
                else if (Request.Cookies[beniHatirla_ID] != null)
                {
                    // Çerezi silmek için eski çerezin üzerine yazılan geçerlilik süresini geçmiş bir tarihle yeni bir çerez oluşturur.
                    var httpCookie = new HttpCookie(beniHatirla_ID)
                    {
                        Expires = DateTime.Now.AddDays(-1)
                    };
                    Response.Cookies.Add(httpCookie);
                }

                TempData["SuccessMessage"] = "Başarıyla giriş yaptınız. Hoş geldiniz!";

                // Kullanıcının rolüne göre belirlenen sayfalara yönlendirme yapar.
                if (kullanicilar.rol_ID == admin_ID)
                {
                    return RedirectToAction("Index", "Rapor");
                }
                else
                {
                    return RedirectToAction("Index", "Musteri");
                }
            }

            TempData["InfoMessage"] = "Email veya şifre hatalı. Lütfen tekrar deneyiniz.";
            return View("~/Views/Musteri/Giris/Giris.cshtml", girisModel);
        }

        // Şifremi unuttum modalını görüntüler.
        [HttpGet]
        public ActionResult SifremiUnuttum()
        {
            return RedirectToAction("Giris");
        }

        // Kullanıcının email adresini alarak mail adresine şifre sıfırlama bağlantısını gönderir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SifremiUnuttum(SifreUnuttumViewModel sifreUnuttumModel)
        {
            // ViewModel'deki validasyonlara uyulmazsa giriş sayfasını tekrar yükler.
            if (!ModelState.IsValid)
            {
                TempData["WarningMessage"] = "Lütfen geçerli bir email adresi giriniz.";
                ViewBag.OpenForgotPassword = true;
                return RedirectToAction("Giris");
            }

            // Email adresini standartlaştırmak için boşlukları kaldırır ve küçük harfe dönüştürür.
            sifreUnuttumModel.Email = sifreUnuttumModel.Email?.Trim().ToLower();

            // Veri tabanında yöneticiyi hariç tutarak eşleşen emaile sahip kullanıcıyı bulur.
            var kullanicilar = db.tbl_kullanici.FirstOrDefault(kullanici => kullanici.email == sifreUnuttumModel.Email);
            if (kullanicilar == null || kullanicilar.rol_ID == admin_ID)
            {
                TempData["WarningMessage"] = "Bu email adresi sistemde kayıtlı değildir.";
                return RedirectToAction("Giris", new { openForgot = 1 });
            }

            // Benzersiz bir şifre sıfırlama token'ı oluşturur.
            var rawToken = Guid.NewGuid().ToString("N");

            // Kullanıcının veri tabanı kaydına token ve token son geçerlilik tarihini kaydeder.
            kullanicilar.reset_token_hash = rawToken;
            kullanicilar.reset_token_sona_erme = DateTime.Now.AddMinutes(tokenGecerlilikSure);
            db.SaveChanges();

            // Şifre sıfırlama linkini oluşturur.
            var resetURL = Url.Action("SifreSifirla", "Giris", new { token = rawToken }, Request.Url.Scheme);

            // Hazırlanan şifre sıfırlama linki oluşturulan şablona ekleyerek kullanıcıya gönderir.
            var AdSoyad = $"{kullanicilar.ad} {kullanicilar.soyad}".Trim();
            SifreSifirlamaEmail(kullanicilar.email, resetURL, AdSoyad);

            TempData["SuccessMessage"] = "Şifreniz sıfırlama bağlantısı email adresinize gönderilmiştir.";
            return RedirectToAction("Giris");
        }

        // Token kontrolü yapar ve token geçerliyse şifre sıfırlama modalını görüntüler.
        [HttpGet]
        public ActionResult SifreSifirla(string token)
        {
            // URL'de token yoksa giriş sayfasını tekrar yükler.
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Giris");
            }

            // Veri tabanında geçerlilik süresi geçmemiş token'a sahip olan müşteri rolündeki kullanıcıları arar.
            var kullanicilar = db.tbl_kullanici.FirstOrDefault(kullanici =>
                kullanici.reset_token_hash == token &&
                kullanici.reset_token_sona_erme > DateTime.Now &&
                kullanici.rol_ID != admin_ID);

            if (kullanicilar == null)
            {
                return RedirectToAction("Giris");
            }

            // Token geçerliyse, şifre sıfırlama modalını açmak için token ve bayrakla birlikte giriş sayfasına yönlendirir.
            return RedirectToAction("Giris", new { token, openReset = 1 });
        }

        // Belirlene yeni şifreyi veri tabanına kaydeder ve tokenı geçersiz kılar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SifreSifirla(SifreSifirlaViewModel sifirlamaModel)
        {
            // ViewModel'deki validasyonlara uyulmazsa giriş sayfasını tekrar yükler.
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Giris");
            }

            // Şifre sıfırlama işlemi için token'ı ve geçerlilik süresini kontrol eder.
            var kullanicilar = db.tbl_kullanici.FirstOrDefault(kullanici =>
                kullanici.reset_token_hash == sifirlamaModel.Token &&
                kullanici.reset_token_sona_erme > DateTime.Now &&
                kullanici.rol_ID != admin_ID);

            if (kullanicilar == null)
            {
                return RedirectToAction("Giris");
            }

            // Kullanıcının yeni şifresini veri tabanına kaydeder, token'ı ve son geçerlilik tarihini temizleyerek veri tabanına kaydeder.
            kullanicilar.sifre = sifirlamaModel.YeniSifre;
            kullanicilar.reset_token_hash = null;
            kullanicilar.reset_token_sona_erme = null;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Şifreniz başarıyla güncellenmiştir. Yeni şifrenizle giriş yapabilirsiniz.";
            return RedirectToAction("Giris");
        }

        // Kayıt formunu görüntüler.
        [HttpGet]
        public ActionResult Kayit()
        {
            return View("~/Views/Musteri/Giris/Kayit.cshtml");
        }

        // Kullanıcı bilgileri veri tabanına kaydeder ve başarılı kayıt sonrasında giriş sayfasına yönlendirir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Kayit(KayitViewModel kayitModel)
        {
            // ViewModel'deki validasyonlara uyulmazsa sayfayı tekrar yükler.
            if (!ModelState.IsValid)
            {
                return View("~/Views/Musteri/Giris/Kayit.cshtml", kayitModel);
            }

            // Kullanıcıdan gelen verilerdeki boşlukları kaldırır ve ad / soyadı büyük harfe, emaili küçük harfe dönüştürür.
            kayitModel.Ad = kayitModel.Ad?.Trim().ToUpper();
            kayitModel.Soyad = kayitModel.Soyad?.Trim().ToUpper();
            kayitModel.Email = kayitModel.Email?.Trim().ToLower();
            kayitModel.Telefon = kayitModel.Telefon?.Trim();
            kayitModel.Adres = kayitModel.Adres?.Trim();

            // Veri tabanında tekrar eden kayıt kontrolü
            bool emailVarMi = db.tbl_kullanici.Any(kullanici => kullanici.email == kayitModel.Email);
            if (emailVarMi)
            {
                TempData["InfoMessage"] = "Bu email adresi ile daha önce kayıt yapılmıştır.";
                return RedirectToAction("Kayit", kayitModel);
            }
            bool telefonVarMi = db.tbl_kullanici.Any(k => k.telefon_no == kayitModel.Telefon);
            if (telefonVarMi)
            {
                TempData["InfoMessage"] = "Bu telefon numarası ile daha önce kayıt yapılmıştır.";
                return RedirectToAction("Kayit", kayitModel);
            }

            // Yeni bir kullanıcı nesnesi oluşturur ve kayıt sayfasından gelen verileri ekler.
            var yeniKullanici = new tbl_kullanici
            {
                ad = kayitModel.Ad,
                soyad = kayitModel.Soyad,
                email = kayitModel.Email,
                telefon_no = kayitModel.Telefon,
                adres = kayitModel.Adres,
                sifre = kayitModel.Sifre,
                rol_ID = 1,
                kayit_tarihi = DateTime.Now,
                reset_token_hash = null,
                reset_token_sona_erme = null
            };

            // Yeni kullanıcıyı veri tabanına kaydeder.
            db.tbl_kullanici.Add(yeniKullanici);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Kaydınız başarıyla oluşturulmuştur. Giriş yapabilirsiniz.";
            return RedirectToAction("Giris", "Giris");
        }

        // Kullanıcının sistemden çıkış yapmasını sağlar ve çerez, oturum verilerini temizler.
        [HttpGet]
        public ActionResult Cikis()
        {
            // Eğer "Beni Hatırla" çerezi varsa siler.
            if (Request.Cookies[beniHatirla_ID] != null)
            {
                // Çerezi silmek için çerezin üzerine yazılan geçerlilik süresini geçmiş bir tarihle yeni bir çerez oluşturur.
                var httpCookie = new HttpCookie(beniHatirla_ID)
                {
                    Expires = DateTime.Now.AddDays(-1),
                    HttpOnly = true
                };
                Response.Cookies.Add(httpCookie); // Çerezi tarayıcıya göndererek siler.
            }

            // Oturum verilerini temizler ve oturumu sonlandırır.
            Session.Clear();
            Session.Abandon();

            // Tarayıcının sayfayı önbelleğe almasını engeller.
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            return RedirectToAction("Giris", "Giris");
        }
    }
}