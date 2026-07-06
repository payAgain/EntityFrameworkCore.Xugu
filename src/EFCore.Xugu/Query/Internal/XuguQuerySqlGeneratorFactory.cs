using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
{
    private readonly QuerySqlGeneratorDependencies _dependencies;

    public XuguQuerySqlGeneratorFactory(QuerySqlGeneratorDependencies dependencies)
        => _dependencies = dependencies;

    public virtual QuerySqlGenerator Create()
        => new XuguQuerySqlGenerator(_dependencies);
}
