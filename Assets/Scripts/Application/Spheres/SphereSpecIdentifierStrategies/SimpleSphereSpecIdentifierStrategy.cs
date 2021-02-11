namespace Assets.Scripts.Application.Spheres
{
    public class SimpleSphereSpecIdentifierStrategy: AbstractSphereSpecIdentifierStrategy
    {
        private readonly string _id;

        public SimpleSphereSpecIdentifierStrategy(string id)
        {
            _id = id;
        }

        public override string GetIdentifier()
        {
            return _id;
        }

        public override string GetLabel()
        {
            return _id;
        }
    }
}