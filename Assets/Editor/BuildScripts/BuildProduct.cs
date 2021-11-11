using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Enums;


namespace SecretHistories.Editor.BuildScripts
{
    public class BuildProduct
    {
        public Product Id { get; private set; }
        public bool IsDLC { get; private set; }

        public GameId GetGameId()
        {
            if ((int) Id > 200)
                return GameId.LG;
            if ((int) Id > 100)
                return GameId.BH;
            return GameId.CS;
        }

   


        public BuildProduct(Product id, bool isDLC)
        {
   
            Id = id;
            IsDLC = isDLC;
        }

        public string GetRelativePath()
        {
            if (IsDLC)
                return NoonUtility.JoinPaths("DLC\\", Id.ToString());
            else
                return Id.ToString();
        }


    }


    public enum Product
    {
        VANILLA=1,
        PERPETUAL_ALLDLC=2,
        PERPETUAL=3,
        DANCER=4,
        PRIEST=5,
        GHOUL=6,
        EXILE=7,
        BH=101

    }
}
