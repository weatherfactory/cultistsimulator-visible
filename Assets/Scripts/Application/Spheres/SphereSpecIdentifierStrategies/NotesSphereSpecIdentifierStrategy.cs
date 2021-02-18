namespace SecretHistories.Spheres
{
    public class NotesSphereSpecIdentifierStrategy: AbstractSphereSpecIdentifierStrategy
    {
        private readonly int _index;
        private const string PREFIX = "notes";
        public NotesSphereSpecIdentifierStrategy(int index)
        {
            _index = index;
        }

        public override string GetIdentifier()
        {
            return string.Concat(PREFIX, _index.ToString());
        }

        public override string GetLabel()
        {
            return GetIdentifier();
        }
    }
}