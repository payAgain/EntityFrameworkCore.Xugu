using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Design;

[assembly: DesignTimeProviderServices("Microsoft.EntityFrameworkCore.Xugu.Design.Internal.XuguDesignTimeServices")]
[assembly: InternalsVisibleTo("EFCore.Xugu.Tests")]
[assembly: InternalsVisibleTo("EFCore.Xugu.Tests.Shared")]
[assembly: InternalsVisibleTo("EFCore.Xugu.Tests.Unit")]
[assembly: InternalsVisibleTo("EFCore.Xugu.Tests.Integration")]
