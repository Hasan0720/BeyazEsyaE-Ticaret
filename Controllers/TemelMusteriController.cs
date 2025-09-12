// Yönetici panelindeki seçilen controllerlar için yetkilendirme ve oturum kontrolü sağlayan temel controller sınıfıdır.
// Seçilen controllerlarda tekrar edecek yetki kontrolü  ve oturum doğrulama kodlarının önüne geçer.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BeyazEsyaE_Ticaret.Controllers
{
    // Müşteri panelinde seçilen controllerlarının türediği temel controller sınıfıdır.
    public class TemelMusteriController : Controller
    {
        // Herhangi bir action metodu çalıştırılmadan önce çağrılarak kullanıcı oturum ve yetkisini doğrular.
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var oturumKullaniciID = Session["kullaniciID"];
            if (oturumKullaniciID == null)
            {
                // Kullanıcı oturumu yoksa giriş sayfasına yönlendirilir
                filterContext.Result = RedirectToAction("Giris", "Giris");
                return;
            }

            int kullanici_Id = Convert.ToInt32(oturumKullaniciID);

            // Veri tabanındaki kullanıcı bilgileri sorgulanır.
            using (var db = new ETicaretEntities())
            {
                var kullaniciBilgileri = db.tbl_kullanici.Find(kullanici_Id);
                if (kullaniciBilgileri == null)
                {
                    // Kullanıcı bulunamazsa oturum temizlenerek giriş sayfasına yönlendirilir.
                    Session.Clear();
                    Session.Abandon();
                    filterContext.Result = RedirectToAction("Giris", "Giris");
                    return;
                }

                if (kullaniciBilgileri.rol_ID != 1)
                {
                    // Kullanıcının rolü müşteri değilse giriş sayfasına yönlendirilir
                    filterContext.Result = RedirectToAction("Giris", "Giris");
                    return;
                }
            }

            // Tüm kontroller sonucunda bir sorun çıkmazsa action'ların çalışmasına izin verilir.
            base.OnActionExecuting(filterContext);
        }

        // Miras alan diğer controller dosyalarında kullanılabilir. Her seferinde "Session" okumak yerine aktif oturumdaki müşteri ID'sini alan yardımcı mettottur.
        protected int OturumKullaniciID()
        {
            return Convert.ToInt32(Session["kullaniciID"]);
        }
    }
}