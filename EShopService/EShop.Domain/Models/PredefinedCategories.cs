using EShopDomain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop.Domain.Models
{
    public class PredefinedCategories : Category
    {
        public static readonly Category Indoor = new Category
        {
            Id = 1,
            Name = "Indoor"
        };

        public static readonly Category Outdoor = new Category
        {
            Id = 2,
            Name = "Outdoor"
        };

        public static readonly Category IndoorOutdoor = new Category
        {
            Id = 3,
            Name = "IndoorOutdoor"
        };
    }
}
