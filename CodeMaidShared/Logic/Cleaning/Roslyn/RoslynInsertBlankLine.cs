using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using SteveCadwallader.CodeMaid.Logic.Cleaning;
using SteveCadwallader.CodeMaid.Properties;
using System;
using System.Linq;

namespace CodeMaidShared.Logic.Cleaning
{
    /// <summary>
    /// A class for encapsulating insertion of explicit access modifier logic.
    /// </summary>
    internal class RoslynInsertBlankLine
    {
        #region Fields

        private readonly SemanticModel _semanticModel;
        private readonly SyntaxGenerator _syntaxGenerator;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// The singleton instance of the <see cref="RoslynInsertBlankLine" /> class.
        /// </summary>
        //private static RoslynInsertBlankLine _instance;

        ///// <summary>
        ///// Gets an instance of the <see cref="RoslynInsertBlankLine" /> class.
        ///// </summary>
        ///// <returns>An instance of the <see cref="RoslynInsertBlankLine" /> class.</returns>
        //internal static RoslynInsertBlankLine GetInstance(AsyncPackage package)
        //{
        //    return new RoslynInsertBlankLine(semanticModel, syntaxGenerator);
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="RoslynInsertBlankLine" /> class.
        /// </summary>
        public RoslynInsertBlankLine(SemanticModel semanticModel, SyntaxGenerator syntaxGenerator)
        {
            _semanticModel = semanticModel;
            _syntaxGenerator = syntaxGenerator;
        }

        #endregion Constructors

        public static RoslynCleanup Initialize(RoslynCleanup cleanup, SemanticModel model, SyntaxGenerator generator)
        {
            var explicitLogic = new RoslynInsertBlankLine(model, generator);
            //cleanup.MemberWriter = explicitLogic.AddPadding;
            return cleanup;
        }

        public static RoslynCleanup Initialize2(RoslynCleanup cleanup, SemanticModel model, SyntaxGenerator generator)
        {
            var explicitLogic = new RoslynInsertBlankLine(model, generator);
            //cleanup.MemberWriter = explicitLogic.AddPadding;
            return cleanup;
        }

        public (SyntaxNode, bool) AddPadding(SyntaxNode original, SyntaxNode newNode, bool addPaddingStart)
        {
            // Add padding to start when addPaddingStart or settings.AddPaddingStart when no padding at start.
            // Return addTrailingPadding when setting.AddPaddingEnd

            // If Settings require padding at the end set true
            // Check if padding is needed -> Exit if already
            // If addPadding or settings require padding at the start,

            if (!addPaddingStart)
            {
                addPaddingStart = newNode.Kind() switch
                {
                    SyntaxKind.ClassDeclaration when Settings.Default.Cleaning_InsertBlankLinePaddingBeforeClasses => true,
                    _ => false,
                };
            }

            if (newNode is ClassDeclarationSyntax cl)
            {
                var f = cl.DescendantNodesAndTokens().First();
                var t = f.GetLeadingTrivia();

                var containsNewLine = t.Any(x => x == SyntaxFactory.EndOfLine(""));
                if (!containsNewLine)
                {
                    var newTrivia = t.Add(SyntaxFactory.EndOfLine(""));
                    newNode = newNode.WithLeadingTrivia(newTrivia);
                }
            }

            return (newNode, Settings.Default.Cleaning_InsertBlankLinePaddingAfterClasses);

            //return newNode switch
            //{
            //    DelegateDeclarationSyntax when Settings.Default.Cleaning_InsertExplicitAccessModifiersOnDelegates => AddAccessibility(original, newNode),
            //    EventFieldDeclarationSyntax when Settings.Default.Cleaning_InsertExplicitAccessModifiersOnEvents => AddAccessibility(original, newNode),
            //    EnumDeclarationSyntax when Settings.Default.Cleaning_InsertExplicitAccessModifiersOnEnumerations => AddAccessibility(original, newNode),
            //    FieldDeclarationSyntax when Settings.Default.Cleaning_InsertExplicitAccessModifiersOnFields => AddAccessibility(original, newNode),
            //    InterfaceDeclarationSyntax when Settings.Default.Cleaning_InsertExplicitAccessModifiersOnInterfaces => AddAccessibility(original, newNode),

            //    PropertyDeclarationSyntax when Settings.Default.Cleaning_InsertExplicitAccessModifiersOnProperties => AddAccessibility(original, newNode),
            //    MethodDeclarationSyntax when Settings.Default.Cleaning_InsertExplicitAccessModifiersOnMethods => AddAccessibility(original, newNode),

            //    ClassDeclarationSyntax when Settings.Default.Cleaning_InsertExplicitAccessModifiersOnClasses => AddAccessibility(original, newNode),
            //    StructDeclarationSyntax when Settings.Default.Cleaning_InsertExplicitAccessModifiersOnStructs => AddAccessibility(original, newNode),

            //    //RecordDeclarationSyntax when node.IsKind(SyntaxKind.RecordDeclaration) && Settings.Default.Cleaning_InsertExplicitAccessModifiersOnRecords => AddAccessibility(original, node),
            //    //RecordDeclarationSyntax when node.IsKind(SyntaxKind.RecordStructDeclaration) && Settings.Default.Cleaning_InsertExplicitAccessModifiersOnRecordStructs => AddAccessibility(original, node),

            //    _ => newNode,
            //};

            return (newNode, false);
        }

        private SyntaxNode AddAccessibility(SyntaxNode original, SyntaxNode newNode)
        {
            if (!CSharpAccessibilityFacts.ShouldUpdateAccessibilityModifier(original as MemberDeclarationSyntax, AccessibilityModifiersRequired.Always, out var _, out var canChange))
            {
                return newNode;
            }

            var mapped = MapToDeclarator(original);

            var symbol = _semanticModel.GetDeclaredSymbol(mapped);
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            var preferredAccessibility = AddAccessibilityModifiersHelpers.GetPreferredAccessibility(symbol);
            return InternalGenerator.WithAccessibility(newNode, preferredAccessibility);
            //return _syntaxGenerator.WithAccessibility(newNode, preferredAccessibility);
        }

        private static SyntaxNode MapToDeclarator(SyntaxNode node)
        {
            return node switch
            {
                FieldDeclarationSyntax field => field.Declaration.Variables[0],
                EventFieldDeclarationSyntax eventField => eventField.Declaration.Variables[0],
                _ => node,
            };
        }
    }
    public class MyClass2
    {
        public class Inner
        {
        }
        public class Inner2
        {
        }
    }
}