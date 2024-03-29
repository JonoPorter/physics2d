﻿using System;
using System.Reflection;
using System.Security.Permissions;

using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("AdvanceMath")]
[assembly: AssemblyDescription("A Matrix, Vector, and Geometry Math Library for .Net")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("AdvanceMath")]
[assembly: AssemblyCopyright("Copyright ©  2005-2008 Jonathan Mark Porter.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(true)]

#if !(CompactFramework || WindowsCE || PocketPC || XBOX360 || SILVERLIGHT)
#if !UNSAFE
[assembly: SecurityPermission(SecurityAction.RequestRefuse, UnmanagedCode = true)]
#endif
[assembly: FileIOPermission(SecurityAction.RequestOptional, Unrestricted = true)]
#endif



// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("b536df6a-2bb5-4beb-832f-319515b1da21")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("2.0.0.*")]
#if !(CompactFramework || WindowsCE || PocketPC || XBOX360)
[assembly: AssemblyFileVersion("2.0.0.0")]
#endif
