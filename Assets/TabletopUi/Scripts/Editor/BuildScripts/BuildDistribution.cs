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
        private BuildStorefront _storefront;
        private BuildProduct _product;
        private BuildOS _os;

        
        public Distribution(BuildStorefront storefront, BuildProduct product, BuildOS os)
        {
            _storefront = storefront;
            _product = product;
            _os = os;
        }

        public void CopyFilesFromEnvironment(BuildEnvironment fromEnvironment)
        {

            if (!Directory.Exists(fromEnvironment.GetProductWithOSBuildPath(_product, _os)))
            {
                fromEnvironment.Log("Can't find source path: terminating distribution creation-  " + fromEnvironment.GetProductWithOSBuildPath(_product,_os));
                return;
            }
            if (Directory.Exists(GetDistributionDestinationPath(fromEnvironment)))
            {
                fromEnvironment.Log("Deleting distribution output path: " + GetDistributionDestinationPath(fromEnvironment));
                Directory.Delete(GetDistributionDestinationPath(fromEnvironment));
            }
            else
            {
                fromEnvironment.Log("Distribution output path does not yet exist: " + GetDistributionDestinationPath(fromEnvironment));
            }

            fromEnvironment.Log("Creating distribution output path: " + GetDistributionDestinationPath(fromEnvironment));
            Directory.CreateDirectory(GetDistributionDestinationPath(fromEnvironment));

            NoonUtility.CopyDirectoryRecursively(fromEnvironment.GetProductWithOSBuildPath(_product, _os), GetDistributionDestinationPath(fromEnvironment));

            File.WriteAllText(GetStoreFileDestinationPath(fromEnvironment), _storefront.StoreId.ToString());
        }

        public string GetDistributionDestinationPath(BuildEnvironment fromEnvironment)
        {
            return NoonUtility.JoinPaths(fromEnvironment.BaseBasePath, _storefront.GetRelativePath(), _os.GetRelativePath(),_product.GetRelativePath());
        }

        public string GetStoreFileDestinationPath(BuildEnvironment fromEnvironment)
        {
            return NoonUtility.JoinPaths(GetDistributionDestinationPath(fromEnvironment), _os.GetStreamingAssetsLocation(), NoonConstants.STOREFRONT_FILE_NAME);
        }
    }
}
 