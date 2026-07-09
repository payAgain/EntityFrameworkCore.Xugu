using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
{
    private readonly QuerySqlGeneratorDependencies _dependencies;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    public XuguQuerySqlGeneratorFactory(
        QuerySqlGeneratorDependencies dependencies,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _dependencies = dependencies;
        _typeMappingSource = typeMappingSource;
    }

    public virtual QuerySqlGenerator Create()
        => new XuguQuerySqlGenerator(_dependencies, _typeMappingSource);
}
