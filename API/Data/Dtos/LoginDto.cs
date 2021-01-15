﻿using System.ComponentModel.DataAnnotations;

namespace API.Data.Dtos
{
    /// <summary>
    /// Dto for the properties sent up in a user login request.
    /// </summary>
    public class LoginDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}