# Dependencies

Full list from `CT_Dashboard/CT_Dashboard/packages.config` (target framework: net472):

| Package | Version | Purpose |
|---|---|---|
| EntityFramework | 6.5.1 | ORM / data access (Database-First) |
| EPPlus | 8.0.1 | Excel read/write (non-commercial personal license, see `Global.asax.cs`) |
| EPPlus.Interfaces | 8.0.0 | EPPlus support library |
| ClosedXML | 0.104.2 | Excel read/write (used alongside EPPlus) |
| ClosedXML.Parser | 1.2.0 | ClosedXML support library |
| DocumentFormat.OpenXml | 3.1.1 | Underlying OOXML support for Excel libraries |
| DocumentFormat.OpenXml.Framework | 3.1.1 | Same as above |
| ExcelNumberFormat | 1.1.0 | Excel number formatting support |
| Microsoft.AspNet.Mvc | 5.2.9 | ASP.NET MVC framework |
| Microsoft.AspNet.Razor | 3.2.9 | Razor view engine |
| Microsoft.AspNet.WebPages | 3.2.9 | ASP.NET Web Pages support |
| Microsoft.AspNet.Web.Optimization | 1.1.3 | Bundling/minification (`BundleConfig.cs`) |
| Microsoft.jQuery.Unobtrusive.Validation | 4.0.0 | Client-side validation |
| Newtonsoft.Json | 13.0.3 | JSON serialization |
| PagedList / PagedList.Mvc | 1.17.0.0 / 4.5.0.0 | Server-side pagination, used in `BaoCaoController` and `ChiTieuController` |
| jQuery | 3.7.1 | Client-side scripting |
| jQuery.Validation | 1.21.0 | Client-side validation |
| bootstrap | 5.3.5 | CSS framework |
| Antlr | 3.5.0.2 | Razor parser dependency (transitive) |
| Modernizr | 2.8.3 | Front-end feature detection |
| WebGrease | 1.6.0 | Bundling/minification support |
| Microsoft.Bcl.Cryptography, Microsoft.Bcl.HashCode, Microsoft.CodeDom.Providers.DotNetCompilerPlatform, Microsoft.IO.RecyclableMemoryStream, Microsoft.Web.Infrastructure, RBush, SixLabors.Fonts, System.Buffers, System.ComponentModel.Annotations, System.Formats.Asn1, System.Memory, System.Numerics.Vectors, System.Runtime.CompilerServices.Unsafe, System.Security.Cryptography.Xml, System.ValueTuple | various | Transitive dependencies of the above (mostly EPPlus/ClosedXML/EF support libraries) |

## Visualization

**Highcharts** is used in Dashboard views for charting (confirmed via script references in `Views/Shared/_Layout.cshtml` and dashboard views). It is not listed in `packages.config` because it is referenced as a script/CDN asset rather than a NuGet package.

## Licensing note

`Global.asax.cs` registers EPPlus under a **non-commercial personal license** (`ExcelPackage.License.SetNonCommercialPersonal(...)`). Anyone reusing this codebase for commercial purposes needs to review EPPlus's licensing terms and obtain an appropriate license.
