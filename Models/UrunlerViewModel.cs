// Müşteri panelinde müşteri ürün modülündeki ürün listeleme sayfasında kullanılacak verileri taşıyan ViewModel sınıfıdır.
// Bu model, hem listelenecek ürünleri hem de sayfanın sol tarafında yer alabilecek kategori filtresi gibi ek bilgileri bir araya getirir.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class UrunlerViewModel
    {
        public List<tbl_urun> Urunler {  get; set; }

        public List<tbl_kategori> Kategoriler { get; set; }
    }
}