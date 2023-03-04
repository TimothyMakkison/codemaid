using Microsoft.CodeAnalysis;
using System;

namespace CodeMaidShared.Logic.Cleaning
{
    internal class InsertPaddingCleanupMiddleware : IRoslynMiddleware
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

            IsFirstNode = false;
            return newNode;
        }

        public void SetDelegate(Func<SyntaxNode, SyntaxNode, SyntaxNode> next)
        {
            Next = next;
        }
    }
}