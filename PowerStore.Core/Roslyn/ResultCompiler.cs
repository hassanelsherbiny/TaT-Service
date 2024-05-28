﻿using System.Collections.Generic;
using System.Reflection;

namespace PowerStore.Core.Roslyn
{
    public partial class ResultCompiler
    {
        public ResultCompiler()
        {
            ErrorInfo = new List<string>();
        }

        public Assembly ReferencedAssembly { get; internal set; }
        public string OriginalFile { get; internal set; }
        public virtual string DLLAssemblyFile { get; internal set; }
        public bool IsCompiled { get; internal set; }
        public IList<string> ErrorInfo { get; internal set; }
    }
}