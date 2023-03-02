using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using SteveCadwallader.CodeMaid.Logic.Cleaning;
using SteveCadwallader.CodeMaid.Properties;

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
            cleanup.PaddingWriter = explicitLogic.AddPadding;
            return cleanup;
        }

        public (SyntaxNode, bool) AddPadding(SyntaxNode original, SyntaxNode newNode, bool previousRequiresPaddingStart, bool isFirst)
        {
            // Assume that whitespace blank lines are considered valid padding.
            // Add padding to start
            // If addPaddingStart
            //      when no padding at first leading trivia
            //      when not first node.
            // Else
            //      when settings require padding
            //      when does not contain a new line in any of the trivia
            //      when not first node

            // Return addTrailingPadding when setting.AddPaddingEnd

            // If Settings require padding at the end set true

            // Cannot add padding to end, if padding is needed let the next node add padding.
            bool requiresPaddingAfter = RequiresPaddingAfter(newNode);

            bool shouldAddPaddingBefore = RequiresPaddingBefore(newNode);

            if (isFirst || StartHasPadding(newNode))
            {
                return (newNode, requiresPaddingAfter);
            }

            var containsAnyPadding = HasAnyPadding(newNode);

            if (previousRequiresPaddingStart || (shouldAddPaddingBefore && containsAnyPadding))
            {
                newNode = InternalGenerator.AddPadding(newNode);
            }

            return (newNode, requiresPaddingAfter);
        }

        private static bool RequiresPaddingBefore(SyntaxNode newNode)
        {
            bool shouldAddPaddingBefore = (newNode.Kind(), Settings.Default) switch
            {
                (SyntaxKind.UsingStatement, { Cleaning_InsertBlankLinePaddingBeforeUsingStatementBlocks: true }) => true,
                (SyntaxKind.NamespaceDeclaration or SyntaxKind.FileScopedNamespaceDeclaration, { Cleaning_InsertBlankLinePaddingBeforeNamespaces: true }) => true,
                //(RegionDirectiveTriviaSyntax, { Cleaning_InsertBlankLinePaddingBeforeRegionTags: true}) => true,
                (SyntaxKind.ClassDeclaration, { Cleaning_InsertBlankLinePaddingBeforeClasses: true }) => true,
                (SyntaxKind.DelegateDeclaration, { Cleaning_InsertBlankLinePaddingBeforeDelegates: true }) => true,
                (SyntaxKind.EnumDeclaration, { Cleaning_InsertBlankLinePaddingBeforeEnumerations: true }) => true,
                (SyntaxKind.EventDeclaration, { Cleaning_InsertBlankLinePaddingBeforeEvents: true }) => true,
                // TODO: (MultiLineIfBlockSyntax, { Cleaning_InsertBlankLinePaddingBeforeFieldsMultiLine: true}) => true,
                (SyntaxKind.FieldDeclaration, { Cleaning_InsertBlankLinePaddingBeforeFieldsSingleLine: true }) => true,
                (SyntaxKind.InterfaceDeclaration, { Cleaning_InsertBlankLinePaddingBeforeInterfaces: true }) => true,
                (SyntaxKind.MethodDeclaration, { Cleaning_InsertBlankLinePaddingBeforeMethods: true }) => true,
                (SyntaxKind.PropertyDeclaration, { Cleaning_InsertBlankLinePaddingBeforePropertiesMultiLine: true }) => true,
                (SyntaxKind.PropertyDeclaration, { Cleaning_InsertBlankLinePaddingBeforePropertiesSingleLine: true }) => true,
                (SyntaxKind.StructDeclaration, { Cleaning_InsertBlankLinePaddingBeforeStructs: true }) => true,

                (SyntaxKind.RecordDeclaration, { Cleaning_InsertBlankLinePaddingBeforeStructs: true }) => true,
                (SyntaxKind.RecordStructDeclaration, { Cleaning_InsertBlankLinePaddingBeforeStructs: true }) => true,

                _ => false,
            };
            return shouldAddPaddingBefore;
        }

        private static bool RequiresPaddingAfter(SyntaxNode newNode)
        {
            bool shouldAddPaddingAfter = (newNode.Kind(), Settings.Default) switch
            {
                (SyntaxKind.UsingStatement, { Cleaning_InsertBlankLinePaddingAfterUsingStatementBlocks: true }) => true,
                (SyntaxKind.NamespaceDeclaration or SyntaxKind.FileScopedNamespaceDeclaration, { Cleaning_InsertBlankLinePaddingAfterNamespaces: true }) => true,
                //(RegionDirectiveTriviaSyntax, { Cleaning_InsertBlankLinePaddingAfterRegionTags: true}) => true,
                (SyntaxKind.ClassDeclaration, { Cleaning_InsertBlankLinePaddingAfterClasses: true }) => true,
                (SyntaxKind.DelegateDeclaration, { Cleaning_InsertBlankLinePaddingAfterDelegates: true }) => true,
                (SyntaxKind.EnumDeclaration, { Cleaning_InsertBlankLinePaddingAfterEnumerations: true }) => true,
                (SyntaxKind.EventDeclaration, { Cleaning_InsertBlankLinePaddingAfterEvents: true }) => true,
                // TODO: (MultiLineIfBlockSyntax, { Cleaning_InsertBlankLinePaddingAfterFieldsMultiLine: true}) => true,
                (SyntaxKind.FieldDeclaration, { Cleaning_InsertBlankLinePaddingAfterFieldsSingleLine: true }) => true,
                (SyntaxKind.InterfaceDeclaration, { Cleaning_InsertBlankLinePaddingAfterInterfaces: true }) => true,
                (SyntaxKind.MethodDeclaration, { Cleaning_InsertBlankLinePaddingAfterMethods: true }) => true,
                (SyntaxKind.PropertyDeclaration, { Cleaning_InsertBlankLinePaddingAfterPropertiesMultiLine: true }) => true,
                (SyntaxKind.PropertyDeclaration, { Cleaning_InsertBlankLinePaddingAfterPropertiesSingleLine: true }) => true,
                (SyntaxKind.StructDeclaration, { Cleaning_InsertBlankLinePaddingAfterStructs: true }) => true,

                (SyntaxKind.RecordDeclaration, { Cleaning_InsertBlankLinePaddingAfterStructs: true }) => true,
                (SyntaxKind.RecordStructDeclaration, { Cleaning_InsertBlankLinePaddingAfterStructs: true }) => true,

                _ => false,
            };
            return shouldAddPaddingAfter;
        }

        private static bool StartHasPadding(SyntaxNode node)
        {
            foreach (var item in node.GetLeadingTrivia())
            {
                var kind = item.Kind();
                if (kind == SyntaxKind.WhitespaceTrivia)
                {
                    continue;
                }
                if (kind == SyntaxKind.EndOfLineTrivia)
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        // TODO handle XML comments?

        private static bool HasAnyPadding(SyntaxNode node)
        {
            var isPadding = true;
            foreach (var item in node.GetLeadingTrivia())
            {
                var kind = item.Kind();
                if (kind == SyntaxKind.WhitespaceTrivia)
                {
                    continue;
                }
                if (kind == SyntaxKind.EndOfLineTrivia)
                {
                    if (isPadding)
                    {
                        return true;
                    }
                    isPadding = true;
                    continue;
                }

                isPadding = false;
            }

            return false;
        }
    }
}