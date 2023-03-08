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
<<<<<<< HEAD
        public Func<SyntaxNode, SyntaxNode, SyntaxNode> MemberWriter { get; set; }

        // Use this messy functions to ensure that the current node is not a descendant of an interface.
        // This is to mimic the recursive CSharpAddAccessibilityModifiersDiagnosticAnalyzer.ProcessMemberDeclaration
        // search where any non structs/classes are ignored.
        // FindAncestorOrSelf might help but would be slower.
        // Dont terminate on finding an interface in case I want to roslynize more cleanup functions.

        private bool InsideInterface { get; set; }

        public RoslynCleanup()
        {
            MemberWriter = (_, x) => x;
            InsideInterface = false;
        }

        public override SyntaxNode Visit(SyntaxNode node)
        {
            var inInterface = InsideInterface;
            if (node.IsKind(SyntaxKind.InterfaceDeclaration))
                InsideInterface = true;

            var newNode = base.Visit(node);

            if (inInterface == false)
            {
                newNode = MemberWriter(node, newNode);
            }

            InsideInterface = inInterface;

            return newNode;
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

            var document = Global.GetActiveDocument();
=======
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
>>>>>>> roslyn_middleware

            if (document == null || !document.TryGetSyntaxRoot(out SyntaxNode root))
            {
                throw new InvalidOperationException();
            }

            var semanticModel = document.GetSemanticModelAsync().Result;
            var syntaxGenerator = SyntaxGenerator.GetGenerator(document);

            var cleaner = new RoslynCleanup();
            RoslynInsertExplicitAccessModifierLogic.Initialize(cleaner, semanticModel, syntaxGenerator);
<<<<<<< HEAD
            cleaner.Process(root, Global.Workspace);

            document = document.WithSyntaxRoot(root);
            Global.Workspace.TryApplyChanges(document.Project.Solution);
=======
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
>>>>>>> roslyn_middleware
        }
    }
}