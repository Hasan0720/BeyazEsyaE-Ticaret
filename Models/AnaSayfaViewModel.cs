// Müşteri panelindeki ana sayfa için gerekli ürün listelerini taşıyan ViewModel sınıfıdır.
// Farklı ürün kategorilerini tek bir modelde bir araya getirerek ana sayfaya kolayca veri aktarımını sağlar.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class AnaSayfaViewModel
    {
        public List<tbl_urun> OneCikanlar {  get; set; }
        public List<tbl_urun> CokSatanlar { get; set; }
        public List<tbl_urun> StokAzalanlar { get; set; }
        public List<tbl_urun> YeniGelenler {  get; set; }
    }
}