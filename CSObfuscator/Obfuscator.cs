using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSObfuscator
{
    class Obfuscator
    {
        public string code;
        public ObfuscateParameters oParams;

        public Obfuscator(string c, ObfuscateParameters p)
        {
            code = c;
            oParams = p;
        }
        

        public string Obfuscate()
        {
            var tree = CSharpSyntaxTree.ParseText(code);

            if (tree.GetDiagnostics().Count() != 0)
                throw new Exception("Код содержит синтаксические ошибки");

            var compilation = CSharpCompilation.Create("obfuscation", new[] { tree });
            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            var stObf = new SyntaxTreeObfuscator(model, oParams);

            root = stObf.Obfuscate(root);

            if (oParams.HasFlag(ObfuscateParameters.RemoveFormattingCharacters))
                root = root.ReplaceTrivia(root.DescendantTrivia(), (oldt, newt) => SyntaxFactory.Space);

            return root.ToFullString();
        }
    }
}
