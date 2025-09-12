// Şifremi unuttum akışında şifre sıfırlama işlemi için gerekli verileri taşıyan ve doğrulamaları yapan ViewModel sınıfıdır.
// Bu model, email ile gönderilen özel bir token ve kullanıcının belirlediği yeni şifreyi içerir.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class SifreSifirlaViewModel
    {
        public string Token { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Şifreniz en az 8, en fazla 20 karakterden oluşabilir.")]
        public string YeniSifre { get; set; }

        [Required(ErrorMessage = "Şifre tekrar alanı zorunludur.")]
        [Compare("YeniSifre", ErrorMessage = "Girilen şifreler birbiriyle uyuşmuyor.")]
        public string YeniSifreTekrar { get; set; }
    }
}