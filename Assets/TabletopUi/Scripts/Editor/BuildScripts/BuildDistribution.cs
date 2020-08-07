using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noon;
using TabletopUi.Scripts.Services;

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
            try
            {


            string fromDirectory = fromEnvironment.GetProductWithOSBuildPath(_product, _os);

            string toDirectory = GetDistributionDestinationPath(fromEnvironment);

            if (!Directory.Exists(fromEnvironment.GetProductWithOSBuildPath(_product, _os)))
            {
                fromEnvironment.LogError("Can't find source path: terminating distribution creation -  " + fromDirectory);
                return;
            }
            if (Directory.Exists(toDirectory))
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
            { 
               File.WriteAllText(GetStoreFileDestinationPath(fromEnvironment), _storefront.StoreId.ToString());

//            if (_storefront.StoreId == Storefront.Steam)
//            {

//                string conscienceText = @"2020-06-08

//Hello. If you're reading this, you probably downloaded the game during the Steam Free Weekend, and haven't updated it yet.

//This might be because you're still playing it, but didn't get around to buying it.

//If you own the game, updating it will make the error message go away.

//If you don't own the game, deleting this file will make the error message go away. But if you enjoyed the game, we'd really appreciate if you bought it. We're two people in a flat, and supporting our work will help us keep making more games like Cultist.

//Have a spooky day

//Lottie and Alexis";

//                File.WriteAllText(GetConscienceFileDestinationPath(fromEnvironment),conscienceText);


//            }
            }

            }
            catch (Exception e)
            {
            NoonUtility.Log(e.Message,2);
                throw;
            }

        }

        public string GetDistributionDestinationPath(BuildEnvironment fromEnvironment)
        {
            return NoonUtility.JoinPaths(fromEnvironment.BuildRoot, CONST_STOREFRONTS_FOLDER, _storefront.GetRelativePath(), _product.GetRelativePath(),_os.GetRelativePath());
        }

        public string GetStoreFileDestinationPath(BuildEnvironment fromEnvironment)
        {
            return NoonUtility.JoinPaths(GetDistributionDestinationPath(fromEnvironment), _os.GetStreamingAssetsPath(), NoonConstants.STOREFRONT_PATH_IN_STREAMINGASSETS);
        }


        public string GetConscienceFileDestinationPath(BuildEnvironment fromEnvironment)
        {
            return NoonUtility.JoinPaths(GetDistributionDestinationPath(fromEnvironment), _os.GetStreamingAssetsPath(), NoonConstants.CONSCIENCE_PATH_IN_STREAMINGASSETS);
        }
    }
}
 