#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

using ICsi.SyntaxHighlighting;

namespace ICsi.Core
{
    internal sealed partial class Repl
    {
        private delegate object? LineRenderHandler(IReadOnlyList<string> lines, 
                                                   int lineIndex,
                                                   object? state);

        private sealed class SubmissionView
        {
            private readonly LineRenderHandler _lineRenderer;
            private readonly ObservableCollection<string> _submissionDocument;
            private int _cursorTop;
            private int _renderedLineCount;
            private int _currentLine;
            private int _currentCharacter;

            public SubmissionView(LineRenderHandler lineRenderer, ObservableCollection<string> submissionDocument)
            {
                _lineRenderer = lineRenderer;
                _submissionDocument = submissionDocument;
                _submissionDocument.CollectionChanged += SubmissionDocumentChanged;
                _cursorTop = Console.CursorTop;
                Render();
            }

            private void SubmissionDocumentChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                Render();
            }

            private void Render()
            {
                Console.CursorVisible = false;

                var lineCount = 0;
                var state = (object?)null;

                foreach (var line in _submissionDocument)
                {
                    if (_cursorTop + lineCount == Console.WindowHeight)
                    {
                        Console.SetCursorPosition(0, Console.WindowHeight - 1);
                        Console.WriteLine();
                        if (_cursorTop > 0)
                            _cursorTop--;
                    }

                    Console.SetCursorPosition(0, _cursorTop + lineCount);

                    if (lineCount == 0)
                        Console.Write("> ");
                    else
                        Console.Write("  ");


                    state = _lineRenderer(_submissionDocument, lineCount, state);
                    Console.Write(new string(' ', Console.WindowWidth - line.Length - 2));
                    lineCount++;
                }

                var numberOfBlankLines = _renderedLineCount - lineCount;
                if (numberOfBlankLines > 0)
                {
                    var blankLine = new string(' ', Console.WindowWidth);
                    for (var i = 0; i < numberOfBlankLines; i++)
                    {
                        Console.SetCursorPosition(0, _cursorTop + lineCount + i);
                        Console.WriteLine(blankLine);
                    }
                }

                _renderedLineCount = lineCount;

                Console.CursorVisible = true;
                UpdateCursorPosition();
            }

            private void UpdateCursorPosition()
            {
                Console.CursorTop = _cursorTop + _currentLine;
                Console.CursorLeft = 2 + _currentCharacter;
            }

            public int CurrentLine
            {
                get => _currentLine;
                set
                {
                    if (_currentLine != value)
                    {
                        _currentLine = value;
                        _currentCharacter = Math.Min(_submissionDocument[_currentLine].Length, _currentCharacter);

                        UpdateCursorPosition();
                    }
                }
            }

            public int CurrentCharacter
            {
                get => _currentCharacter;
                set
                {
                    if (_currentCharacter != value)
                    {
                        _currentCharacter = value;
                        UpdateCursorPosition();
                    }
                }
            }
        }

        private object? RenderLine(IReadOnlyList<string> submission,
                                   int lineIndex,
                                   object? state)
        {
            Console.Write(submission[lineIndex]);
            return state;
        }

        private bool _done;
        private string EditSubmission()
        {
            _done = false;

            var document = new ObservableCollection<string>() { "" };
            SubmissionView view;

            if (_enableSyntaxHighlighting)
                view = new SubmissionView(CSharpSyntaxHighlighter.RenderLine, document);
            else
                view = new SubmissionView(RenderLine, document);

            while (!_done)
            {
                var key = Console.ReadKey(true);
                HandleKey(key, document, view);
            }

            view.CurrentLine = document.Count - 1;
            view.CurrentCharacter = document[view.CurrentLine].Length;
            Console.WriteLine();

            return string.Join(Environment.NewLine, document);
        }
    }
}