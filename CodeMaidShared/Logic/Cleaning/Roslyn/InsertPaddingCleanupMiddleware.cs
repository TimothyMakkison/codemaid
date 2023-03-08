using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace CodeMaidShared.Logic.Cleaning
{
    internal class InsertPaddingCleanupMiddleware : IRoslynNodeMiddleware
    {
        private RoslynInsertBlankLine _insertBlankLine;
        public InsertPaddingCleanupMiddleware()
        {
            _insertBlankLine = new RoslynInsertBlankLine();
        }

        private bool ShouldAddPadding { get; set; }
        private bool IsFirstNode { get; set; }
        private Func<SyntaxNode, SyntaxNode, SyntaxNode> Next { get; set; }

        public SyntaxNode Invoke(SyntaxNode original, SyntaxNode newNode)
        {
            var shouldAddPadding = ShouldAddPadding;
            var isFirst = IsFirstNode;

            ShouldAddPadding = false;
            IsFirstNode = true;

            newNode = Next(original, newNode);

            (newNode, ShouldAddPadding) =  _insertBlankLine.AddPadding(original, newNode, shouldAddPadding, isFirst);

            // Have to ignore inheritance/type/attribute nodes until the first member node.
            IsFirstNode = isFirst ? newNode is TypeParameterListSyntax or AttributeArgumentListSyntax or BaseListSyntax or TypeParameterConstraintClauseSyntax : false;
            return newNode;
        }

        public void SetNodeDelegate(Func<SyntaxNode, SyntaxNode, SyntaxNode> next)
        {
            Next = next;
        }
    }
}