namespace SecretHistories.Constants.Modding
{
    public class NullMod : Mod
    {
        public override bool IsValid => false;

        public NullMod(): base("null mod","")
        {

        }


    }
}