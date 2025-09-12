// Şifremi unuttum akışında kullanıcının email adresini alarak ve doğrulamak için kullanılan ViewModel sınıfıdır.
// Bu model, kullanıcının girdiği email adresini doğrulayarak şifre sıfırlama sürecinin ilk adımını başlatır.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BeyazEsyaE_Ticaret.Models
{
    public class SifreUnuttumViewModel
    {
        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [RegularExpression(@"^(?!.*\.\.)[a-z0-9._%/*\-+]+@[a-z.]+\.[a-z]{2,}$", ErrorMessage = "Lütfen geçerli formatta bir email adresi giriniz.")]
        public string Email { get; set; }
    }
}