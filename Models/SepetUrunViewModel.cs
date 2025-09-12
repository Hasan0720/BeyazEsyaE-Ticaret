// Müşteri panelindeki sepet sayfasında ürün verilerini taşımak için kullanılan ViewModel sınıfıdır.
// Bu model, sepet sayfasında her bir ürünün detaylarını görüntülemek ve yönetmek amacıyla tasarlanmıştır.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class SepetUrunViewModel
    {
        public int UrunID { get; set; }
        public string UrunAdi { get;set; }
        public string Gorsel { get; set; }
        public decimal BirimFiyat { get; set; }
        public byte Adet { get; set; }
        public decimal ToplamFiyat => BirimFiyat * Adet;
        public byte StokAdedi {  get; set; }
    }
}