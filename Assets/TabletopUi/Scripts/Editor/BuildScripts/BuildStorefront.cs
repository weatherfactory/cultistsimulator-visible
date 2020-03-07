using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.TabletopUi.Scripts.Editor.BuildScripts
{
    public class Storefront
    {
        public string StoreId { get; private set; }
        public List<Product> ProductList;


        public Storefront(string id,List<Product> list)
        {
            StoreId = id;
            ProductList = list;
        }

        public string GetRelativePath()
        {
            return StoreId;
        }
    }
}
