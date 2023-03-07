using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace NugetUtility
{
    public class HtmlPrinter
    {
        private static readonly HashSet<string> _skippedNames = new()
        {
            "head",
            "meta",
            "script",
            "style",
            "link"
        };

        private static readonly HashSet<string> _blockLevelNames = new()
        {
            "address",
            "article",
            "aside",
            "blockquote",
            "details",
            "dialog",
            "dd",
            "div",
            "dl",
            "dt",
            "fieldset",
            "figcaption",
            "figure",
            "footer",
            "form",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "header",
            "hgroup",
            "hr",
            "li",
            "main",
            "nav",
            "ol",
            "p",
            "pre",
            "section",
            "table",
            "thead",
            "tr",
            "tbody",
            "ul"
        };

        private static readonly Dictionary<string, int> _indentLookup = new()
        {
            {"blockquote", 4},
            {"dd", 4},
            {"li", 4},
            {"ol", 2}
        };

        private readonly TextWriter _writer;

        private readonly int _pageWidth;

        private readonly HtmlDocument _document;

        private int _column;

        private readonly Stack<int> _indents = new();

        private int Indent => _indents.Sum();

        private int _consecutiveNewlines;

        private bool _lastCharWasSpace;

        private readonly Stack<HtmlElementLevel> _levels = new();

        private HtmlElementLevel Level
        {
            get
            {
                if (_levels.Any())
                {
                    return _levels.Peek();
                }

                return HtmlElementLevel.Block;
            }
        }

        private readonly Stack<HtmlListType> _listTypes = new();

        private HtmlListType ListType
        {
            get
            {
                if (_listTypes.Any())
                {
                    return _listTypes.Peek();
                }

                return HtmlListType.None;
            }
        }

        private bool _inUnopenedListItem;

        private readonly Stack<int> _itemNumbers = new();

        public HtmlPrinter(HtmlDocument document, TextWriter writer, int pageWidth, HashSet<string> tagNamesToSkip)
        {
            _document = document;
            _writer = writer;
            _pageWidth = pageWidth;
            if (tagNamesToSkip is not null)
            {
                _skippedNames.UnionWith(tagNamesToSkip);
            }
        }

        public void Print()
        {
            Print(_document.DocumentNode);
            _writer.WriteLine();
        }

        private void Print(HtmlNode node)
        {
            if (_skippedNames.Contains(node.Name))
            {
                return;
            }

            switch (node.NodeType)
            {
                case HtmlNodeType.Element:
                case HtmlNodeType.Document:
                    if (_blockLevelNames.Contains(node.Name))
                    {
                        PrintBlockLevel(node);
                    }
                    else
                    {
                        PrintInline(node);
                    }

                    break;
                case HtmlNodeType.Text:
                    PrintText((HtmlTextNode) node);
                    break;
            }
        }

        private void PrintBlockLevel(HtmlNode node)
        {
            _levels.Push(HtmlElementLevel.Block);

            if (_column != 0)
            {
                NewLine();
            }

            while ("p".Equals(node.Name) && _consecutiveNewlines < 2)
            {
                NewLine();
            }

            if ("hr".Equals(node.Name))
            {
                EnsureIndent();
                WriteChars('-', _pageWidth - _column);
            }

            bool shouldIndent = _indentLookup.ContainsKey(node.Name);
            if (shouldIndent)
            {
                _indents.Push(_indentLookup[node.Name]);
                if ("li".Equals(node.Name))
                {
                    _inUnopenedListItem = true;
                }
                else
                {
                    if ("ul".Equals(node.Name))
                    {
                        _listTypes.Push(HtmlListType.Unordered);
                    }
                    else if ("ol".Equals(node.Name))
                    {
                        _listTypes.Push(HtmlListType.Ordered);
                        _itemNumbers.Push(1);
                    }
                }
            }

            foreach (HtmlNode child in node.ChildNodes)
            {
                Print(child);
            }

            if (shouldIndent)
            {
                _indents.Pop();
                if ("ul".Equals(node.Name) || "ol".Equals(node.Name))
                {
                    _listTypes.Pop();
                    if ("ol".Equals(node.Name))
                    {
                        _itemNumbers.Pop();
                    }
                }
            }

            while ("p".Equals(node.Name) && _consecutiveNewlines < 2)
            {
                NewLine();
            }

            _levels.Pop();
        }

        private void PrintInline(HtmlNode node)
        {
            _levels.Push(HtmlElementLevel.Inline);

            if ("br".Equals(node.Name))
            {
                NewLine();
            }
            else if ("img".Equals(node.Name))
            {
                if (node.Attributes.Contains("alt"))
                {
                    string val = node.Attributes["alt"].Value;
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        PrintString("[" + val + "]");
                    }
                }
            }

            foreach (HtmlNode child in node.ChildNodes)
            {
                Print(child);
            }

            _levels.Pop();
        }

        private void PrintText(HtmlTextNode node)
        {
            PrintString(node.Text);
        }

        private void OpenListItem()
        {
            string itemLabel;
            if (ListType == HtmlListType.Ordered)
            {
                int num = _itemNumbers.Pop();
                _itemNumbers.Push(num + 1);
                itemLabel = num + ". ";
            }
            else
            {
                itemLabel = "* ";
            }

            int spaces = Math.Min(_pageWidth, Indent) - itemLabel.Length;
            WriteSpaces(spaces);
            WriteRaw(itemLabel);
            _lastCharWasSpace = true;
            _inUnopenedListItem = false;
        }

        private void PrintString(string text)
        {
            if (text == null)
            {
                return;
            }

            bool newLine = true;
            text = Regex.Replace(text, @"\s+", " ");
            text = HtmlEntity.DeEntitize(text);
            if (Level == HtmlElementLevel.Block || _lastCharWasSpace)
            {
                text = text.TrimStart();
            }

            if (text.Length == 0)
            {
                return;
            }

            EnsureIndent();
            string[] nonSpaceSegments = text.Split(' ');
            foreach (string nonSpaceSegment in nonSpaceSegments)
            {
                string[] words = nonSpaceSegment.Split('\n');
                bool first = true;
                foreach (string word in words)
                {
                    if (!first)
                    {
                        NewLine();
                        newLine = true;
                    }

                    if (_column > Indent && _column + word.Length >= _pageWidth)
                    {
                        NewLine();
                        newLine = true;
                    }

                    if (!newLine)
                    {
                        _consecutiveNewlines = 0;
                        _writer.Write(' ');
                        _lastCharWasSpace = true;
                        _column++;
                    }

                    EnsureIndent();
                    WriteRaw(word);
                    if (word.Length > 0)
                    {
                        _lastCharWasSpace = false;
                    }

                    _consecutiveNewlines = 0;
                    newLine = false;

                    first = false;
                }
            }
        }

        private void WriteRaw(string text)
        {
            _writer.Write(text);
            _column += text.Length;
        }

        private void NewLine()
        {
            _writer.WriteLine();
            _column = 0;
            _consecutiveNewlines++;
        }

        private void WriteSpaces(int numSpaces)
        {
            WriteChars(' ', numSpaces);
        }

        private void WriteChars(char val, int number)
        {
            for (int i = 0; i < number; i++)
            {
                _writer.Write(val);
            }

            _column += number;
            _consecutiveNewlines = 0;
            if (val == ' ')
            {
                _lastCharWasSpace = true;
            }
        }

        private void EnsureIndent()
        {
            if (_inUnopenedListItem)
            {
                OpenListItem();
                return;
            }
            int indent = Math.Min(Indent, _pageWidth);
            if (_column < indent)
            {
                WriteSpaces(indent - _column);
            }
        }

        private enum HtmlListType
        {
            None,
            Ordered,
            Unordered
        }

        private enum HtmlElementLevel
        {
            Block,
            Inline
        }
    }
}
