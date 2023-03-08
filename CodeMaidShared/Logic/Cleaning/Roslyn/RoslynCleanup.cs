using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.VisualStudio.Shell;
using SteveCadwallader.CodeMaid.Logic.Cleaning;
using System;

namespace CodeMaidShared.Logic.Cleaning
{
    internal class RoslynCleanup : CSharpSyntaxRewriter
    {
        public RoslynCleanup()
        {
            UpdateNodePipeline = (x, _) => base.Visit(x);
            UpdateTokenPipeline = (x, _) => base.VisitToken(x);
        }

        private Func<SyntaxNode, SyntaxNode, SyntaxNode> UpdateNodePipeline { get; set; }
        private Func<SyntaxToken, SyntaxToken, SyntaxToken> UpdateTokenPipeline { get; set; }

        public override SyntaxNode Visit(SyntaxNode original)
        {
            if (original == null)
            {
                return original;
            }
            var newNode = original;

            return UpdateNodePipeline(original, newNode);
        }

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            if (token == null)
            {
                return token;
            }
            var newToken = token;

            return UpdateTokenPipeline(token, newToken);
        }

        public SyntaxNode Process(SyntaxNode root, Workspace workspace)
        {
            var rewrite = Visit(root);
            return rewrite;

            //return Formatter.Format(rewrite, SyntaxAnnotation.ElasticAnnotation, workspace);
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

        public void AddNodeMiddleware(IRoslynNodeMiddleware middleware)
        {
            middleware.SetNodeDelegate(UpdateNodePipeline);

            UpdateNodePipeline = middleware.Invoke;
        }

        public void AddTokenMiddleware(IRoslynTokenMiddleware middleware)
        {
            middleware.SetTokenDelegate(UpdateTokenPipeline);

            UpdateTokenPipeline = middleware.Invoke;
        }
    }
}