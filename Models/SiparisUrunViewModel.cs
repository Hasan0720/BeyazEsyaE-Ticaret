// Sipariş detaylarında yer alan her bir ürünün bilgilerini taşımak için kullanılan ViewModel sınıfıdır.
// Bu model, sipariş özetlerinde ve fatura detaylarında ürün bilgilerini göstermek amacıyla kullanılır.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class SiparisUrunViewModel
    {
        public int UrunID { get; set; }
        public string UrunAdi {  get; set; }
        public string Gorsel { get; set; }
        public bool Aktiflik { get; set; }
        public byte Adet { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal ToplamFiyat { get; set; }
    }
}