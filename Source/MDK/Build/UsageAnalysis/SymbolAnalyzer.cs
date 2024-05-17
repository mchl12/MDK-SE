using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Malware.MDKServices;
using Malware.MDKUtilities;
using MDK.Build.Annotations;
using MDK.Build.Solution;
using Microsoft.CodeAnalysis;

namespace MDK.Build.UsageAnalysis
{
    class SymbolAnalyzer
    {
        static bool IsNotSpecialDefinitions(SymbolDefinitionInfo s)
        {
            return s.Symbol != null && !s.Symbol.IsOverride && !s.Symbol.IsInterfaceImplementation()
                && !s.Symbol.GetAttributes().Any(attributeData => attributeData.AttributeClass?.GetFullName() == "Malware.MDKUtilities.NoRenameAttribute");
        }

        public HashSet<string> ProtectedSymbols { get; } = new HashSet<string>
        {
            "Program",
            "Program.Main",
            "Program.Save"
        };

        public HashSet<string> ProtectedNames { get; } = new HashSet<string>
        {
            ".ctor",
            ".cctor",
            "Finalize",
        };

        public ImmutableArray<SymbolDefinitionInfo> FindSymbols(ProgramComposition composition, MDKProjectProperties config)
        {
            var root = composition.RootNode;
            var semanticModel = composition.SemanticModel;

            return root.DescendantNodes().Where(node => node.IsSymbolDeclaration())
                .Select(n => new SymbolDefinitionInfo(semanticModel.GetDeclaredSymbol(n), n))
                .Where(IsNotSpecialDefinitions)
                .Select(WithUpdatedProtectionFlag)
                .ToImmutableArray();
        }

        SymbolDefinitionInfo WithUpdatedProtectionFlag(SymbolDefinitionInfo d)
        {
            return ProtectedNames.Contains(d.Symbol.Name)
                   || ProtectedSymbols.Contains(d.Symbol.GetFullName(DeclarationFullNameFlags.WithoutNamespaceName))
                   || d.SyntaxNode.ShouldBePreserved()
                ? d.AsProtected()
                : d.AsUnprotected();
        }
    }
}