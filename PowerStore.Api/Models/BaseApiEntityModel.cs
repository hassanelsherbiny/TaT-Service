﻿using System.ComponentModel.DataAnnotations;

namespace PowerStore.Api.Models
{
    public partial class BaseApiEntityModel
    {
        [Key]
        public string Id { get; set; }
    }
}
