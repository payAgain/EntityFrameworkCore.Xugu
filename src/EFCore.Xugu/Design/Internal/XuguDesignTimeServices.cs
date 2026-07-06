using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Xugu.Scaffolding.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Xugu.Design.Internal;

/// <summary>
///     Design-time services for <c>dotnet ef</c> with XuguDB.
/// </summary>
public class XuguDesignTimeServices : IDesignTimeServices
{
    public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddEntityFrameworkXugu();

        new EntityFrameworkRelationalDesignServicesBuilder(serviceCollection)
            .TryAdd<IAnnotationCodeGenerator, XuguAnnotationCodeGenerator>()
            .TryAdd<IDatabaseModelFactory, XuguDatabaseModelFactory>()
            .TryAdd<IProviderConfigurationCodeGenerator, XuguCodeGenerator>()
            .TryAddCoreServices();
    }
}
