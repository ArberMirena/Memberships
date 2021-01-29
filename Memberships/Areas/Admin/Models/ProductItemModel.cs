using Memberships.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Memberships.Areas.Admin.Models
{
    public class ProductItemModel
    {
        public int ProductId { get; set; }

        public int ItemId { get; set; }

        public string ProductTitle { get; set; }

        public string ItemTitle { get; set; }

        public ICollection<Product> Products { get; set; }

        public ICollection<Item> Items { get; set; }
    }
}