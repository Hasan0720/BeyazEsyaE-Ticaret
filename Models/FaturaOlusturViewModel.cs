// Yönetici panelindeki fatura modülündeki fatura oluşturma sayfasında kullanılacak verileri taşıyan ViewModel sınıfıdır.
// Bu model, yöneticinin fatura oluşturma işlemi öncesinde siparişle ilgili tüm detayları görüntülemesini ve onaylamasını sağlar.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class FaturaOlusturViewModel
    {
        public int SiparisID { get; set; }
        public DateTime SiparisTarihi { get; set; }
        public string TeslimatAdresi { get; set; }
        public decimal ToplamTutar {  get; set; }
        
        public string MusteriAdSoyad { get; set; }
        
        public List<FaturaUrunViewModel> Urunler {  get; set; }
    }

    // Faturada listelenecek her bir ürünün detaylarını taşıyan yardımcı ViewModel sınıfıdır.
    // Bu model, 'FaturaOlusturViewModel' içindeki 'Urunler' listesi için kullanılır.
    public class FaturaUrunViewModel
    {
        public string UrunAdi { get; set; }
        public byte Adet { get; set;}
        public decimal BirimFiyat { get; set; }
        public decimal ToplamFiyat { get; set; }
    }
}