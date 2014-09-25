namespace KerbalPackageManager
{
    public class UnresolvedPackage
    {
        public UnresolvedPackage(string Name)
        {
            this.Name = Name;
        }

        public string Name { get; private set; }

        public Package Resolve()
        {
            return Manager.Resolve(this.Name);
        }
    }
}