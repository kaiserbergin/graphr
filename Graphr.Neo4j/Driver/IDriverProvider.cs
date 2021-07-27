using Neo4j.Driver;

namespace Graphr.Neo4j.Driver
{
    public interface IDriverProvider
    {
        IDriver Driver { get; }
    }
}