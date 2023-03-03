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