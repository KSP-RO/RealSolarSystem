#define CIBUILD_disabled
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("RealSolarSystem")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("RealSolarSystem")]
[assembly: AssemblyCopyright("Copyright © 2014 - 2022, KSP-RO Team")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM.

[assembly: Guid("c44fc31a-d718-4d96-b789-91c4e9bfd950")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

#if CIBUILD
[assembly: AssemblyVersion("@MAJOR@.@MINOR@.@PATCH@.@BUILD@")]
[assembly: AssemblyFileVersion("@MAJOR@.@MINOR@.@PATCH@.@BUILD@")]
[assembly: KSPAssembly("RealSolarSystem", @MAJOR@, @MINOR@, @PATCH@)]
#else
[assembly: AssemblyVersion("18.5.0.0")]
[assembly: AssemblyFileVersion("18.5.0.0")]
[assembly: KSPAssembly("RealSolarSystem", 21, 0, 0)]
#endif

[assembly: KSPAssemblyDependency("Kopernicus", 1, 0)]
[assembly: KSPAssemblyDependency("Kopernicus.Parser", 1, 0)]
