﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSnacks.Models
{
    public class OrderDetails
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        public decimal Total { get; set; }

        public string? ProductName { get; set; }

        public string? ProductImage { get; set; }

        public string ImagePath => AppConfig.BaseUrl + ProductImage;

        public decimal ProductPrice { get; set; }
    }
}
