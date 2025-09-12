// Yönetici panelindeki fatura modülündeki fatura listeleme sayfasının verilerini taşımak için kullanılan ViewModel sınıfıdır.
// Bu model, hem faturalandırılmış siparişleri hem de henüz faturalandırılmamış siparişleri tek bir sayfada görüntülemek için tasarlanmıştır.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class FaturaIndexViewModel
    {
        public List<tbl_fatura> Faturalar {  get; set; }

        public List<tbl_siparis> Faturalanmayanlar { get; set; }
    }
}