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
        public Func<SyntaxNode, SyntaxNode, SyntaxNode> MemberWriter { get; set; }

        // Use this messy functions to ensure that the current node is not a descendant of an interface.
        // This is to mimic the recursive CSharpAddAccessibilityModifiersDiagnosticAnalyzer.ProcessMemberDeclaration
        // search where any non structs/classes are ignored.
        // FindAncestorOrSelf might help but would be slower.
        // Dont terminate on finding an interface in case I want to roslynize more cleanup functions.

        private bool InsideInterface { get; set; }
        private bool ShouldAddPadding { get; set; }

        public RoslynCleanup()
        {
            MemberWriter = (_, x) => x;
            InsideInterface = false;
        }

        public override SyntaxNode Visit(SyntaxNode node)
        {
            var inInterface = InsideInterface;
            var shouldAddPadding = ShouldAddPadding;

            if (node.IsKind(SyntaxKind.InterfaceDeclaration))
                InsideInterface = true;

            // Might have to account for namespaces here.
            ShouldAddPadding = false;
            var newNode = base.Visit(node);

            if (inInterface == false)
            {
                newNode = MemberWriter(node, newNode);
            }



            //ShouldAddPadding = shouldAddPadding;
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

            if (document == null || !document.TryGetSyntaxRoot(out SyntaxNode root))
            {
                throw new InvalidOperationException();
            }

            var semanticModel = document.GetSemanticModelAsync().Result;
            var syntaxGenerator = SyntaxGenerator.GetGenerator(document);

            var cleaner = new RoslynCleanup();
            RoslynInsertExplicitAccessModifierLogic.Initialize(cleaner, semanticModel, syntaxGenerator);
            cleaner.Process(root, Global.Workspace);

            document = document.WithSyntaxRoot(root);
            Global.Workspace.TryApplyChanges(document.Project.Solution);
        }
    }

    public class AddAccessorCleanupMiddleware
    {
        //public SyntaxNode Process(SyntaxNode original, SyntaxNode newNode)
        //{
        //}
    }
    class MyClass
    {
    }
}