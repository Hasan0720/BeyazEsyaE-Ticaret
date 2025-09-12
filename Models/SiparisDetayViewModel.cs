// Müşteri panelinde hesap modülündeki sipariş detay sayfasında bir siparişin tüm detaylarını görüntülemek için kullanılan ViewModel sınıfıdır.
// Bu model, siparişin kapsamlı bir görünümünü sunar.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class SiparisDetayViewModel
    {
        public int SiparisID { get; set; }
        public int? FaturaID { get; set; }
        public DateTime SiparisTarihi { get; set; }
        public decimal ToplamTutar { get; set; }
        public string SiparisDurumu { get; set; }
        public string TeslimatAdresi { get; set; }
        public List<SiparisUrunViewModel> Urunler {  get; set; }
        public decimal AraToplam { get; set; }
        public decimal KdvTutar { get; set; }
        public decimal GenelToplam { get; set; }
    }
}