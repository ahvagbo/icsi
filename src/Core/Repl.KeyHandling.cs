using System;
using System.Collections.ObjectModel;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ICsi.Core
{
    internal sealed partial class Repl
    {
        private void HandleKey(ConsoleKeyInfo key,
                               ObservableCollection<string> document,
                               SubmissionView view)
        {
            if (key.Modifiers == default)
            {
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        HandleEnter(document, view);
                        break;
                    case ConsoleKey.Escape:
                        HandleEscape(document, view);
                        break;
                    case ConsoleKey.Backspace:
                        HandleBackspace(document, view);
                        break;
                    case ConsoleKey.Tab:
                        HandleTab(document, view);
                        break;
                    case ConsoleKey.Home:
                        HandleHome(document, view);
                        break;
                    case ConsoleKey.End:
                        HandleEnd(document, view);
                        break;
                    case ConsoleKey.PageUp:
                        HandlePageUp(document, view);
                        break;
                    case ConsoleKey.PageDown:
                        HandlePageDown(document, view);
                        break;
                    case ConsoleKey.LeftArrow:
                        HandleLeftArrow(document, view);
                        break;
                    case ConsoleKey.RightArrow:
                        HandleRightArrow(document, view);
                        break;
                    case ConsoleKey.UpArrow:
                        HandleUpArrow(document, view);
                        break;
                    case ConsoleKey.DownArrow:
                        HandleDownArrow(document, view);
                        break;
                }
            }

            else if (key.Modifiers == ConsoleModifiers.Control)
            {
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        HandleControlEnter(document, view);
                        break;
                }
            }

            if (key.Key != ConsoleKey.Backspace
             && key.KeyChar >= ' ')
            {
                HandleTyping(document, view, key.KeyChar.ToString());
            }
        }

        private void LoadSubmissionFromHistory(ObservableCollection<string> document, SubmissionView view)
        {
            if (_history.Count == 0)
                return;

            document.Clear();

            string[] submission = _history[_historyIndex].Split(Environment.NewLine);
            foreach (string line in submission)
            {
                document.Add(line);
            }

            view.CurrentLine = document.Count - 1;
            view.CurrentCharacter = document[view.CurrentLine].Length;
        }

        private void HandlePageUp(ObservableCollection<string> document, SubmissionView view)
        {
            _historyIndex--;
            if (_historyIndex < 0)
                _historyIndex = _history.Count - 1;
            LoadSubmissionFromHistory(document, view);
        }

        private void HandlePageDown(ObservableCollection<string> document, SubmissionView view)
        {
            _historyIndex++;
            if (_historyIndex > _history.Count - 1)
                _historyIndex = 0;
            LoadSubmissionFromHistory(document, view);
        }

        private void HandleEscape(ObservableCollection<string> document, SubmissionView view)
        {
            document.Clear();
            document.Add(string.Empty);
            view.CurrentLine = 0;
            view.CurrentCharacter = 0;
        }

        private void HandleDownArrow(ObservableCollection<string> document, SubmissionView view)
        {
            if (view.CurrentLine < document.Count - 1)
                view.CurrentLine++;
        }

        private void HandleUpArrow(ObservableCollection<string> document, SubmissionView view)
        {
            if (view.CurrentLine > 0)
                view.CurrentLine--;
        }

        private void HandleRightArrow(ObservableCollection<string> document, SubmissionView view)
        {
            if (view.CurrentCharacter == document[view.CurrentLine].Length)
                return;
            view.CurrentCharacter++;
        }

        private void HandleLeftArrow(ObservableCollection<string> document, SubmissionView view)
        {
            if (view.CurrentCharacter == 0)
                return;
            view.CurrentCharacter--;
        }

        private void HandleHome(ObservableCollection<string> document, SubmissionView view)
        {
            view.CurrentCharacter = 0;
        }

        private void HandleEnd(ObservableCollection<string> document, SubmissionView view)
        {
            view.CurrentCharacter = document[view.CurrentLine].Length;
        }

        private void InsertLine(ObservableCollection<string> document, SubmissionView view)
        {
            string remainder = document[view.CurrentLine].Substring(view.CurrentCharacter);
            document[view.CurrentLine] = document[view.CurrentLine].Substring(0, view.CurrentCharacter);

            var lineIndex = view.CurrentLine + 1;
            document.Insert(lineIndex, remainder);
            view.CurrentCharacter = 0;
            view.CurrentLine = lineIndex;
        }

        private void HandleEnter(ObservableCollection<string> document, SubmissionView view)
        {
            string submissionText = string.Join(Environment.NewLine, document);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(submissionText,
                                                         CSharpParseOptions.Default
                                                                           .WithKind(SourceCodeKind.Script));

            if (submissionText.StartsWith('.') || SyntaxFactory.IsCompleteSubmission(tree))
            {
                _done = true;
                return;
            }

            InsertLine(document, view);
        }

        private void HandleControlEnter(ObservableCollection<string> document, SubmissionView view)
        {
            InsertLine(document, view);
        }

        private void HandleBackspace(ObservableCollection<string> document, SubmissionView view)
        {
            int start = view.CurrentCharacter;
            if (start == 0)
            {
                if (view.CurrentLine == 0)
                    return;

                string currentLine = document[view.CurrentLine];
                string previousLine = document[view.CurrentLine - 1];
                document.RemoveAt(view.CurrentLine);
                view.CurrentLine--;
                document[view.CurrentLine] = previousLine + currentLine;
                view.CurrentCharacter = previousLine.Length;
            }

            else
            {
                int lineIndex = view.CurrentLine;
                string line = document[lineIndex];
                string before = line.Substring(0, start - 1);
                string after = line.Substring(start);
                document[lineIndex] = before + after;
                view.CurrentCharacter--;
            }
        }

        private void HandleTyping(ObservableCollection<string> document, SubmissionView view, string v)
        {
            int lineIndex = view.CurrentLine;
            int start = view.CurrentCharacter;
            document[lineIndex] = document[lineIndex].Insert(start, v);
            view.CurrentCharacter += v.Length;
        }

        private void HandleTab(ObservableCollection<string> document, SubmissionView view)
        {
            string tab = new string(' ', 4);
            int line = view.CurrentLine;
            int start = view.CurrentCharacter;
            document[line] = document[line].Insert(start, tab);
            view.CurrentCharacter += tab.Length;
        }
    }
}