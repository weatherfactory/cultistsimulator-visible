using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noon;

namespace Assets.TabletopUi.Scripts.Editor.BuildScripts
{
    public class Distribution
    {
        Storefront _storefront;
        Product _product;
        OS _os;
        private BuildEnvironment _env;

        public Distribution(BuildEnvironment environment, Storefront storefront, Product product, OS os)
        {
            _env = environment;
            _storefront = storefront;
            _product = product;
            _os = os;
        }

        public void Create(BuildEnvironment fromEnvironment)
        {

            if (!Directory.Exists(_env.GetProductWithOSBuildPath(_product, _os)))
            {
                fromEnvironment.Log("Can't find source path: terminating distribution creation-  " + _env.GetProductWithOSBuildPath(_product,_os));
                return;
            }
            if (Directory.Exists(GetDistributionDestinationPath()))
            {
                fromEnvironment.Log("Deleting distribution output path: " + GetDistributionDestinationPath());
                Directory.Delete(GetDistributionDestinationPath());
            }
            else
            {
                fromEnvironment.Log("Distribution output path does not yet exist: " + GetDistributionDestinationPath());
            }

            fromEnvironment.Log("Creating distribution output path: " + GetDistributionDestinationPath());
            Directory.CreateDirectory(GetDistributionDestinationPath());

            NoonUtility.CopyDirectoryRecursively(fromEnvironment.GetProductWithOSBuildPath(_product, _os), GetDistributionDestinationPath());

            File.WriteAllText(GetStoreFileDestinationPath(), _storefront.StoreId);
        }

        public string GetDistributionDestinationPath()
        {
            return NoonUtility.JoinPaths(_env.BasePath, _storefront.GetRelativePath(), _os.GetRelativePath(),_product.GetRelativePath());
        }

        public string GetStoreFileDestinationPath()
        {
            return NoonUtility.JoinPaths(GetDistributionDestinationPath(), _os.GetStreamingAssetsLocation(), "store.txt");
        }
    }
}
 