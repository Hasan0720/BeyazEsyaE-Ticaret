// Yönetici panelinin temel giriş noktasıdır.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BeyazEsyaE_Ticaret.Controllers
{
    // Yönetici panelindeki işlemler için ana controller sınıfı, TemelYoneticiController'dan miras alır.
    public class YoneticiController : TemelYoneticiController
    {
        // Yönetici paneli ana sayfasını görüntüler
        public ActionResult Index()
        {
            return View();
        }
    }
}