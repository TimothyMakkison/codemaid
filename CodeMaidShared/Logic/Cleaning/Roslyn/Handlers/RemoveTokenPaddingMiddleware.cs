using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SteveCadwallader.CodeMaid.Logic.Cleaning;
using SteveCadwallader.CodeMaid.Properties;
using System;
using System.Linq;

namespace CodeMaidShared.Logic.Cleaning
{
    internal class RemoveTokenPaddingMiddleware : IRoslynTokenMiddleware
    {
        public bool BeforeIsOpen { get; set; } = false;
        private Func<SyntaxToken, SyntaxToken, SyntaxToken> Next { get; set; }

        public SyntaxToken Invoke(SyntaxToken token, SyntaxToken newToken)
        {
            var beforeIsOpenToken = BeforeIsOpen;

            if (!token.IsKind(SyntaxKind.None))
            {
                BeforeIsOpen = token.IsKind(SyntaxKind.OpenBraceToken);
            }
            newToken = Next(token, newToken);

            // Read trivia:
            // Assume that leading trivia must start on a new line.
            // Valid line is a single line with white space and endofline, preceeded by a non blank line.
            // Also check that
            return TryPadTrivia(newToken, beforeIsOpenToken);
        }
        public void SetTokenDelegate(Func<SyntaxToken, SyntaxToken, SyntaxToken> next)
        {
            Next = next;
        }

        public static void Initialize(RoslynCleaner roslynCleanup)
        {
            var tokenPadding = new InsertTokenPaddingMiddleware();
            roslynCleanup.AddTokenMiddleware(tokenPadding);
        }

        // TODO Revisit logic, cleanup, refactor. Consider calculating the trivia lines on the go while inserting padding.
        // Might not work if all trivia is on the same line.
        private static SyntaxToken TryPadTrivia(SyntaxToken newToken, bool priorIsOpen)
        {
            var trivia = newToken.LeadingTrivia.ToList();

            var (triviaLines, last) = RoslynExtensions.ReadTriviaLines(trivia);
            triviaLines.Add((newToken.Kind(), last));

            // Set previous line type to either open brace or a place holder.
            var prevLine = priorIsOpen ? SyntaxKind.OpenBraceToken : SyntaxKind.BadDirectiveTrivia;

            var insertCount = 0;

            for (int i = 0; i < triviaLines.Count; i++)
            {
                var (currentLine, pos) = triviaLines[i];
                if (RemoveLine(prevLine, currentLine))
                {
                    trivia.Insert(pos + insertCount, SyntaxFactory.EndOfLine(Environment.NewLine));
                    insertCount++;
                }
                prevLine = currentLine;
            }

            if (insertCount > 0)
            {
                return newToken.WithLeadingTrivia(trivia);
            }

            return newToken;
        }

        private static bool RemoveLine(SyntaxKind prior, SyntaxKind current)
        {
            return current switch
            {
                SyntaxKind.WhitespaceTrivia when prior == SyntaxKind.WhitespaceTrivia &&Settings.Default.Cleaning_RemoveMultipleConsecutiveBlankLines => true,

                _ => false,
            };
        }
    }
}