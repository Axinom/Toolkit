﻿using System.Reflection;
using System.Runtime.CompilerServices;

// This is the real version, displayed in UI and not used for reference purposes.
[assembly: AssemblyFileVersion("14.1.0")]

// Only use major version here, with others zero, to avoid breaking references.
[assembly: AssemblyVersion("14.0.0")]

[assembly: AssemblyCompany("Axinom")]
[assembly: AssemblyProduct("Axinom Toolkit")]
[assembly: AssemblyCopyright("Copyright © Axinom")]

[assembly: InternalsVisibleTo("Tests.NetFramework")]
[assembly: InternalsVisibleTo("Tests.NetCore")]