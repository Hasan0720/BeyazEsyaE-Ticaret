// Müşteri panelindeki hesap modülünde kullanıcının şifresini değiştirmesi için gerekli verileri taşımak ve doğrulamak amacıyla kullanılan ViewModel sınıfıdır.
// Bu model, mevcut şifre, yeni şifre ve yeni şifrenin tekrarı gibi bilgileri alarak şifre değiştirme formunun güvenli bir şekilde çalışmasını sağlar.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class SifreDegistirViewModel
    {
        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Şifreniz en az 8, en fazla 20 karakterden oluşabilir.")]
        public string EskiSifre { get; set; }

        [Required(ErrorMessage = "Yeni şifre alanı zorunludur.")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Şifreniz en az 8, en fazla 20 karakterden oluşabilir.")]
        public string YeniSifre { get; set; }

        [Required(ErrorMessage = "Yeni şifre tekrar alanı zorunludur.")]
        [Compare("YeniSifre", ErrorMessage = "Girilen şifreler birbiriyle uyuşmuyor.")]
        public string YeniSifreTekrar { get; set; }
    }
}