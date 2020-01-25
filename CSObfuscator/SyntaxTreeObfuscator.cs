using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSObfuscator
{
    [Flags]
    enum ObfuscateParameters
    {
        RemoveFormattingCharacters = 0x01,
        RenameLocals = 0x02,
        RenameParameters = 0x04,
        RenameGlobals = 0x08,
        ObfuscateLiterals = 0x10,
        //ReplaceConstants = 0x20,
        ReplaceConstructions = 0x40,
        //EncryptSourceCode = 0x80
    }

    class SyntaxTreeObfuscator : CSharpSyntaxRewriter
    {
        SemanticModel model;
        Dictionary<ISymbol, string> replacements;
        IdentifierGenerator idGen;
        ObfuscateParameters oParams;

        public SyntaxTreeObfuscator(SemanticModel mdl, ObfuscateParameters op)
            : base(false)
        {
            model = mdl;
            oParams = op;
            idGen = new IdentifierGenerator();
            replacements = new Dictionary<ISymbol, string>();
        }


        public SyntaxNode Obfuscate(SyntaxNode node)
        {
            return Visit(node);
        }

        #region RenameGlobals

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            ISymbol symbol = model.GetDeclaredSymbol(node);

            if (symbol != null && oParams.HasFlag(ObfuscateParameters.RenameGlobals))
            {
                replacements.Add(symbol, idGen.NewId());
                return ((ClassDeclarationSyntax)base.VisitClassDeclaration(node))
                    .WithIdentifier(SyntaxFactory.Identifier(replacements[symbol]));
            }
            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            ISymbol symbol = model.GetDeclaredSymbol(node);

            if (symbol != null && oParams.HasFlag(ObfuscateParameters.RenameGlobals))
            {
                replacements.Add(symbol, idGen.NewId());
                return ((StructDeclarationSyntax)base.VisitStructDeclaration(node))
                    .WithIdentifier(SyntaxFactory.Identifier(replacements[symbol]));
            }
            return base.VisitStructDeclaration(node);
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            ISymbol symbol = model.GetDeclaredSymbol(node.Parent);

            if (symbol != null && oParams.HasFlag(ObfuscateParameters.RenameGlobals))
                return ((ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node))
                    .WithIdentifier(SyntaxFactory.Identifier(replacements[symbol]));
            return base.VisitConstructorDeclaration(node);
        }
        public override SyntaxNode VisitDestructorDeclaration(DestructorDeclarationSyntax node)
        {
            ISymbol symbol = model.GetDeclaredSymbol(node.Parent);

            if (symbol != null && oParams.HasFlag(ObfuscateParameters.RenameGlobals))
                return ((DestructorDeclarationSyntax)base.VisitDestructorDeclaration(node))
                    .WithIdentifier(SyntaxFactory.Identifier(replacements[symbol]));
            return base.VisitDestructorDeclaration(node);
        }

        public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            ISymbol symbol = model.GetDeclaredSymbol(node);

            if (symbol != null && oParams.HasFlag(ObfuscateParameters.RenameGlobals))
            {
                replacements.Add(symbol, idGen.NewId());
                return ((EnumDeclarationSyntax)base.VisitEnumDeclaration(node))
                    .WithIdentifier(SyntaxFactory.Identifier(replacements[symbol]));
            }
            return base.VisitEnumDeclaration(node);
        }

        public override SyntaxNode VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            ISymbol symbol = model.GetDeclaredSymbol(node);

            if (symbol != null && oParams.HasFlag(ObfuscateParameters.RenameGlobals))
            {
                replacements.Add(symbol, idGen.NewId());
                return ((EnumMemberDeclarationSyntax)base.VisitEnumMemberDeclaration(node))
                    .WithIdentifier(SyntaxFactory.Identifier(replacements[symbol]));
            }
            return base.VisitEnumMemberDeclaration(node);
        }

        #endregion

        #region RenameParameters

        public override SyntaxNode VisitParameter(ParameterSyntax node)
        {
            ISymbol symbol = model.GetDeclaredSymbol(node);

            if (symbol != null && oParams.HasFlag(ObfuscateParameters.RenameParameters) && symbol.Kind == SymbolKind.Parameter)
            {
                replacements.Add(symbol, idGen.NewId());
                return ((ParameterSyntax)base.VisitParameter(node))
                    .WithIdentifier(SyntaxFactory.Identifier(replacements[symbol]));
            }
            return base.VisitParameter(node);
        }

        #endregion

        #region RenameLocals and Fields

        public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            ISymbol symbol = model.GetDeclaredSymbol(node);

            // VariableDeclaration могут быть как обычные переменные, так и поля, поэтому нужны доп.проверки
            if (symbol != null &&
                ((oParams.HasFlag(ObfuscateParameters.RenameLocals) && symbol.Kind == SymbolKind.Local) ||
                (oParams.HasFlag(ObfuscateParameters.RenameGlobals) && symbol.Kind == SymbolKind.Field)))
            {
                replacements.Add(symbol, idGen.NewId());
                return ((VariableDeclaratorSyntax)base.VisitVariableDeclarator(node))
                    .WithIdentifier(SyntaxFactory.Identifier(replacements[symbol]));
            }
            return base.VisitVariableDeclarator(node);
        }

        #endregion

        #region Rename

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            ISymbol symbol = model.GetSymbolInfo(node).Symbol;

            if (symbol != null && replacements.ContainsKey(symbol))
                return ((IdentifierNameSyntax)base.VisitIdentifierName(node))
                    .WithIdentifier(SyntaxFactory.Identifier(replacements[symbol]));
            return base.VisitIdentifierName(node);
        }

        #endregion

        #region ObfuscateLiterals

        public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (oParams.HasFlag(ObfuscateParameters.ObfuscateLiterals))
            {
                if (node.Token.Kind() == SyntaxKind.StringLiteralToken)
                    return ((LiteralExpressionSyntax)base.VisitLiteralExpression(node))
                        .WithToken(SyntaxFactory.ParseToken("\"" + ObfuscateString(node.Token.ValueText) + "\""));

                if (node.Token.Kind() == SyntaxKind.NumericLiteralToken)
                {
                    return SyntaxFactory.ParseExpression(ObfuscateNumerical(node.Token.ValueText));
                }
            }

            return base.VisitLiteralExpression(node);
        }

        public override SyntaxNode VisitInterpolatedStringText(InterpolatedStringTextSyntax node)
        {
            string obf = ObfuscateString(node.TextToken.ValueText);
            var token = SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.InterpolatedStringTextToken,
                obf, obf, SyntaxTriviaList.Empty);
            return ((InterpolatedStringTextSyntax)base.VisitInterpolatedStringText(node))
                .WithTextToken(token);
        }

        private string ObfuscateString(string str)
        {
            var buffer = new StringBuilder(str.Length * 2);
            foreach (char ch in str)
                buffer.AppendFormat("\\u{0:X4}", (ushort)ch);
            return buffer.ToString();
        }
        private string ObfuscateNumerical(string numerical)
        {
            Random rand = new Random();

            int s64;
            if (int.TryParse(numerical, out s64))
            {
                int second;
                byte operation = (byte)rand.Next(2);
                switch (operation)
                {
                    case 0: // +
                        second = rand.Next(s64);
                        return String.Format("({0} + {1})", s64 - second, second);
                    case 1: // -
                        second = rand.Next(0, int.MaxValue - s64);
                        return String.Format("({0} - {1})", s64 + second, second);
                }
            }
            return numerical;
            
        }

        #endregion

        #region ReplaceConstructions

        public override SyntaxNode VisitBlock(BlockSyntax block)
        {
            block = (BlockSyntax)base.VisitBlock(block);

            // если параметр изменения конструкций установлен
            if (oParams.HasFlag(ObfuscateParameters.ReplaceConstructions))
            {
                for (int i = 0; i < block.Statements.Count; i++)
                {
                    // если это блок условия
                    if (block.Statements[i] is IfStatementSyntax)
                    {
                        // проводим замену
                        var newIf = ReplaceIfStatement((IfStatementSyntax)block.Statements[i]);
                        block = block.WithStatements(block.Statements.ReplaceRange(block.Statements[i], newIf));
                    }
                }
            }

            return block;
        }
        private List<StatementSyntax> ReplaceIfStatement(IfStatementSyntax currentIf)
        {
            var ifId = idGen.NewId();
            var endifId = idGen.NewId();
            var ifLabel = SyntaxFactory.LabeledStatement(ifId, SyntaxFactory.EmptyStatement());
            var endifLabel = SyntaxFactory.LabeledStatement(endifId, SyntaxFactory.EmptyStatement());
            var ifGotoStatement = SyntaxFactory.GotoStatement(SyntaxKind.GotoStatement,
                SyntaxFactory.IdentifierName(ifId).WithLeadingTrivia(SyntaxFactory.Space));
            var endifGotoStatement = SyntaxFactory.GotoStatement(SyntaxKind.GotoStatement,
                SyntaxFactory.IdentifierName(endifId).WithLeadingTrivia(SyntaxFactory.Space));
            var newIf = SyntaxFactory.IfStatement(currentIf.Condition, ifGotoStatement);

            /*
             Шаблон замены следующий:
             if (условие) goto метка-if-блока
             else-блок
             goto метка-конца-цикла
             метка-if-блока:
             if-блок
             метка-конца-цикла:
             */

            var newStatements = new List<StatementSyntax>();
            newStatements.Add(newIf);
            if (currentIf.Else != null)
            {
                if (currentIf.Else.Statement is BlockSyntax)
                    newStatements.AddRange((currentIf.Else.Statement as BlockSyntax).Statements);
                else
                    newStatements.Add(currentIf.Else.Statement);
            }
            newStatements.Add(endifGotoStatement);
            newStatements.Add(ifLabel);
            if (currentIf.Statement is BlockSyntax)
                newStatements.AddRange((currentIf.Statement as BlockSyntax).Statements);
            else
                newStatements.Add(currentIf.Statement);
            newStatements.Add(endifLabel);

            return newStatements;
        }

        #endregion
    }
}