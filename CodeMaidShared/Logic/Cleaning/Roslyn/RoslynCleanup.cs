using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.VisualStudio.Shell;
using SteveCadwallader.CodeMaid.Logic.Cleaning;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeMaidShared.Logic.Cleaning
{
    internal class RoslynCleanup : CSharpSyntaxRewriter
    {
        public RoslynCleanup()
        {
            InvokePipeline = (x, _) => base.Visit(x);
        }

        public override SyntaxNode Visit(SyntaxNode original)
        {
            if (original == null)
            {
                return original;
            }
            var newNode = original;

            return InvokePipeline(original, newNode);
        }

        public SyntaxNode Process(SyntaxNode root, Workspace workspace)
        {
            var rewrite = Visit(root);
            return rewrite;

            //return Formatter.Format(rewrite, SyntaxAnnotation.ElasticAnnotation, workspace);
        }

        public bool BeforeIsOpen { get; set; } = false;
        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            var beforeIsOpenToken = BeforeIsOpen;

            if (token.IsKind(SyntaxKind.OpenBraceToken))
            {
                BeforeIsOpen = true;
                return base.VisitToken(token);
            }

            BeforeIsOpen = false;
            var newToken = base.VisitToken(token);

            if (beforeIsOpenToken)
                return newToken;

            // Read trivia:
            // Assume that leading trivia must start on a new line.
            // Valid line is a single line with white space and endofline, preceeded by a non blank line.
            // Also check that

            newToken = TryPadComments(newToken);
            newToken = TryPadRegion(newToken);

            BeforeIsOpen = false;
            return newToken;
        }

        private static SyntaxToken TryPadRegion(SyntaxToken newToken)
        {
            var trivia = newToken.LeadingTrivia.ToArray();

            var list = new List<int>();

            var (read, last) = RoslynExtensions.ReadTrivia2(trivia);

            var prior = SyntaxKind.BadDirectiveTrivia;
            for (int i = 0; i < read.Count; i++)
            {
                var (val, pos) = read[i];
                Temp(list, prior, val, pos);
                prior = val;
            }
            if (read.Count > 0)
            {
                Temp(list, prior, newToken.Kind(), last);
            }

            if (list.Count > 0)
            {
                list = list.Distinct().ToList();
                var newTrivia = newToken.LeadingTrivia.ToList();
                for (int i = list.Count - 1; i >=0; i--)
                {
                    newTrivia.Insert(list[i], SyntaxFactory.EndOfLine(Environment.NewLine));
                }

                newToken = newToken.WithLeadingTrivia(newTrivia);
            }

            return newToken;
        }

        private static void Temp(List<int> list, SyntaxKind prior, SyntaxKind val, int pos)
        {
            if (val == SyntaxKind.RegionDirectiveTrivia && prior is not (SyntaxKind.WhitespaceTrivia or SyntaxKind.OpenBraceToken))
            {
                list.Add(pos);
            }
            if (val is not (SyntaxKind.WhitespaceTrivia or SyntaxKind.CloseBraceToken) && prior == SyntaxKind.RegionDirectiveTrivia)
            {
                list.Add(pos);
            }

            if (val == SyntaxKind.EndRegionDirectiveTrivia && prior is not (SyntaxKind.WhitespaceTrivia or SyntaxKind.OpenBraceToken))
            {
                list.Add(pos);
            }
            if (val is not (SyntaxKind.WhitespaceTrivia or SyntaxKind.CloseBraceToken) && prior == SyntaxKind.EndRegionDirectiveTrivia)
            {
                list.Add(pos);
            }
        }

        private static SyntaxToken TryPadComments(SyntaxToken newToken)
        {
            var trivia = newToken.LeadingTrivia.ToArray();

            var prior = LineType.NonBlank;
            var position = 0;

            var list = new List<int>();

            while (position < trivia.Length)
            {
                var (newPos, current) = RoslynExtensions.ReadTrivia(trivia, position);
                if (current == LineType.SingleComment && prior == LineType.NonBlank)
                {
                    list.Add(position);
                }

                prior = current;
                position = newPos + 1;
            }

            if (list.Count > 0)
            {
                var newTrivia = newToken.LeadingTrivia.ToList();
                for (int i = list.Count - 1; i >=0; i--)
                {
                    newTrivia.Insert(list[i], SyntaxFactory.EndOfLine(Environment.NewLine));
                }

                newToken = newToken.WithLeadingTrivia(newTrivia);
            }

            return newToken;
        }

        public static void BuildAndrun(AsyncPackage package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Global.Package = package;

            var document = Global.GetActiveDocument(package);

            if (document == null || !document.TryGetSyntaxRoot(out SyntaxNode root))
            {
                throw new InvalidOperationException();
            }

            var semanticModel = document.GetSemanticModelAsync().Result;
            var syntaxGenerator = SyntaxGenerator.GetGenerator(document);

            var cleaner = new RoslynCleanup();
            RoslynInsertExplicitAccessModifierLogic.Initialize(cleaner, semanticModel, syntaxGenerator);
            RoslynInsertBlankLine.Initialize(cleaner);

            var newRoot = cleaner.Process(root, Global.GetWorkspace(package));

            document = document.WithSyntaxRoot(newRoot);
            Global.GetWorkspace(package).TryApplyChanges(document.Project.Solution);
        }

        private Func<SyntaxNode, SyntaxNode, SyntaxNode> InvokePipeline { get; set; }
        public void AddMiddleware(IRoslynMiddleware middleware)
        {
            middleware.SetDelegate(InvokePipeline);

            InvokePipeline = middleware.Invoke;
        }
    }
}