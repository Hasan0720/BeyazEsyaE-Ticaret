// Müşteri panelinde müşteri ürün modülündeki ürün detay sayfasında gösterilecek tüm verileri tek bir yapıda toplayan ViewModel sınıfıdır.
// Bu model, ana ürün bilgilerini ve bu bilgilere ek olarak doğrudan veri tabanı modelinde bulunmayan ancak görüntüleme için gerekli olan marka, model ve kategori adları gibi ek bilgileri taşır.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class UrunDetayViewModel
    {
        public tbl_urun Urun {  get; set; }

        public string MarkaAdi { get; set; }

        public string ModelAdi { get; set; }
        
        public string KategoriAdi { get; set; }

        public byte StokAdedi { get; set; }
    }
}