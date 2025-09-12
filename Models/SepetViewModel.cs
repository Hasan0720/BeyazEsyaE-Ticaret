// Müşteri panelindeki sepet sayfasında gösterilecek tüm verileri tek bir yapıda toplayan ViewModel sınıfıdır.
// Bu model, sepetin içeriği, ara toplam, KDV ve genel toplam gibi bilgileri hesaplayarak kullanıcıya sepetinin güncel durumunu sunar.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class SepetViewModel
    {
        public List<SepetUrunViewModel> Urunler { get; set; } = new List<SepetUrunViewModel>();
        public decimal AraToplam => Urunler.Sum(SepetUrunViewModel => SepetUrunViewModel.ToplamFiyat);
        public decimal KDV => AraToplam * 0.2m;
        public decimal GenelToplam => AraToplam + KDV;
    }
}