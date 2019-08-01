using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebLayer.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="Kullanıcı adı zorunludur.")]
        public string UserName { get; set; }

        [Required(ErrorMessage ="Şifre zoruldur.")]
        public string Password { get; set; }
    }
}
