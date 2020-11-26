using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabletopUi.Scripts.Services;

namespace Assets.TabletopUi.Scripts.Editor.BuildScripts
{
    public class BuildStorefront
    {
        public Storefront StoreId { get; private set; }
        public List<BuildOS> OSList;
        public List<BuildProduct> ProductList;



        public BuildStorefront(Storefront id,List<BuildOS> oss,List<BuildProduct> products)
        {
            StoreId = id;
            ProductList = products;
            OSList = oss;

        }

        public string GetRelativePath()
        {
            return StoreId.ToString();
        }

        public List<Distribution> GetDistributionsForStorefront(BuildEnvironment env)
        {
            List<Distribution> distributions=new List<Distribution>();
            foreach (var o in OSList)
            {
                foreach (var p in ProductList)
                {
                   distributions.Add(new Distribution(this,p,o));
                   
                }
            }

            return distributions;
        }
    }
}
