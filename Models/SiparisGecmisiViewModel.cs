// Müşteri panelinde hesap modülündeki sipariş geçmişi sayfasında her bir siparişi özetlemek için kullanılan ViewModel sınıfıdır.
// Bu model, bir siparişin temel bilgilerini ve hızlı bir görsel önizlemesini tek bir satırda gösterebilmek için tasarlanmıştır.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class SiparisGecmisiViewModel
    {
        public int SiparisID { get; set; }
        public DateTime SiparisTarihi { get; set; }
        public decimal ToplamTutar { get; set; }
        public string SiparisDurumu {  get; set; }
        public List<string> Gorsel {  get; set; }
        public string AliciAdi { get; set; }
        public byte ToplamUrunSayisi { get; set; }
        public byte TeslimatSayisi { get; set; }
    }
}