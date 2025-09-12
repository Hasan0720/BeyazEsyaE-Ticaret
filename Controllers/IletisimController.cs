// Müşteri panelinde müşterilerin mesaj gönderme işlemlerinin yönetimini sağlar.

using BeyazEsyaE_Ticaret.Models; // ViewModel'ler için
using System;
using System.Collections.Generic;
using System.Configuration; // Web.config'den ayarları okumak için
using System.Linq;
using System.Net;
using System.Net.Configuration; // SMTP konfigürasyonu için
using System.Net.Mail; // Email gönderme işlemleri için
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BeyazEsyaE_Ticaret.Controllers
{
    // Fatura işlemlerinin yönetimini sağlayan controller sınıfı, TemelMusteriController'dan miras alır.
    public class IletisimController : TemelMusteriController
    {
        ETicaretEntities db = new ETicaretEntities();

        // İletişim formunu görüntüler.
        public ActionResult Index()
        {
            // İletişim sayfasında arama kutusunun gösterilmesini engeller.
            ViewBag.AramaGoster = false;
            return View("~/Views/Musteri/Iletisim/Index.cshtml");
        }

        // Gönderilen mesajları işleyerek hem veri tabanına kaydeder hem de site mail adresine gönderir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(IletisimViewModel iletisimModel)
        {
            // İletişim sayfasında arama kutusunun gösterilmesini engeller.
            ViewBag.AramaGoster = false;

            // Herhangi bir hata varsa sayfayı tekrar yükler.
            if (!ModelState.IsValid)
            {
                return View("~/Views/Musteri/Iletisim/Index.cshtml", iletisimModel);
            }

            // Oturumdaki kullanıcının ID'sini alır, Kullanıcıdan gelen verilerdeki boşlukları kaldırır ve ad / soyadı büyük harfe, emaili küçük harfe dönüştürür.
            int kullaniciID = OturumKullaniciID();
            iletisimModel.Ad = iletisimModel.Ad?.Trim().ToUpper();
            iletisimModel.Soyad = iletisimModel.Soyad?.Trim().ToUpper();
            iletisimModel.Email = iletisimModel.Email?.Trim().ToLower();
            iletisimModel.Mesaj = iletisimModel.Mesaj?.Trim();

            // İletişim için yeni bir iletişim nesnesi oluşturur ve oluşturulan mesajı veri tabanına ekleyerek değişiklikleri kaydeder.
            tbl_iletisim yeni = new tbl_iletisim
            {
                kullanici_ID = kullaniciID,
                ad = iletisimModel.Ad,
                soyad = iletisimModel.Soyad,
                email = iletisimModel.Email,
                mesaj = iletisimModel.Mesaj,
                gonderim_tarihi = DateTime.Now
            };
            db.tbl_iletisim.Add(yeni);
            db.SaveChanges();

            // Mesajı mail olarak gönderen yardımcı metodu çağırarak mesajı sistem mail adresinie de gönderir.
            IletisimMailGonder(iletisimModel.Ad + " " + iletisimModel.Soyad,
                iletisimModel.Email,
                iletisimModel.Mesaj);
            TempData["SuccessMessage"] = "Mesajınız alınmıştır. En kısa sürede sizinle iletişime geçeceğiz.";
            return RedirectToAction("Index");
        }

        // Sistem mailine gönderilecek mesajın hazırlandığı yardımcı metot.
        private void IletisimMailGonder(string adSoyad, string email, string mesaj)
        {
            // Email konu başlığı ve içeriği oluşturur.
            var konu = "Yeni İletişim Mesajı";
            var icerik = $@"
                <p>Gönderen: {(string.IsNullOrWhiteSpace(adSoyad) ? "Belirtilmemiş" : adSoyad)}</p>
                <p>E-posta: {email}</p>
                <p>Mesaj:</p>
                <p>{HttpUtility.HtmlEncode(mesaj)}</p>
            ";

            // Projenin Web.config dosyasındaki SMTP ayarlarını okur.
            var smtpCfg = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");

            // MailMessage nesnesi üzerinden email mesajı oluşturur.
            using (var mail = new MailMessage(email, smtpCfg.From, konu, icerik))
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
    }
}