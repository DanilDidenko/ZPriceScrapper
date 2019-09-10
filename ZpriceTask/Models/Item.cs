using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZpriceTask
{
    public class Item
    {

        public Item(string Name, decimal Price, int Id)
        {

            
            this.Id = Id;
            this.Name = Name;
            this.Price = this.Price = Price;
        }

        public int Id { get; set; }
        public string Name {get; set;}

        public decimal Price { get; set; }
    }
}
