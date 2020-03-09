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

        private const string CONST_STOREFRONTS_FOLDER = "STOREFRONT_DISTRIBUTIONS";

        
        public Distribution(BuildStorefront storefront, BuildProduct product, BuildOS os)
        {
            _storefront = storefront;
            _product = product;
            _os = os;
        }

        public void CopyFilesFromEnvironment(BuildEnvironment fromEnvironment)
        {
            string fromDirectory = fromEnvironment.GetProductWithOSBuildPath(_product, _os);

            string toDirectory = GetDistributionDestinationPath(fromEnvironment);

            if (!Directory.Exists(fromEnvironment.GetProductWithOSBuildPath(_product, _os)))
            {
                fromEnvironment.Log("Can't find source path: terminating distribution creation -  " + fromDirectory);
                return;
            }
            if (Directory.Exists(GetDistributionDestinationPath(fromEnvironment)))
            {
                fromEnvironment.Log("Deleting distribution output path: " + toDirectory);
                Directory.Delete(toDirectory,true);
            }
            else
            {
                fromEnvironment.Log("Distribution output path does not yet exist: " + toDirectory);
                fromEnvironment.Log("Creating distribution output path: " + toDirectory);
                Directory.CreateDirectory(toDirectory);
            }

            NoonUtility.CopyDirectoryRecursively(fromDirectory, toDirectory);

            if(!_product.IsDLC)
                File.WriteAllText(GetStoreFileDestinationPath(fromEnvironment), _storefront.StoreId.ToString());
        }

        public string GetDistributionDestinationPath(BuildEnvironment fromEnvironment)
        {
            return NoonUtility.JoinPaths(fromEnvironment.BuildRoot, CONST_STOREFRONTS_FOLDER, _storefront.GetRelativePath(), _product.GetRelativePath(),_os.GetRelativePath());
        }

        public string GetStoreFileDestinationPath(BuildEnvironment fromEnvironment)
        {
            return NoonUtility.JoinPaths(GetDistributionDestinationPath(fromEnvironment), _os.GetStreamingAssetsPath(), NoonConstants.STOREFRONT_PATH_IN_STREAMINGASSETS);
        }
    }
}
 