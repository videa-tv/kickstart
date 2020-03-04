using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kickstart.Interface;

namespace Kickstart.Utility
{
    public class CodeWriter : ICodeWriter
    {
        private int _indentCount;

        private bool _needsIndent = true;
        private StringBuilder _stringBuilder;
        private int _savedIndent;

        public int LineCount {
            get
            {
                int count = 0;
                var text = this.ToString();
                if (!string.IsNullOrEmpty(text))
                {
                    count = text.Length - text.Replace("\n", string.Empty).Length;

                    // if the last char of the string is not a newline, make sure to count that line too
                    if (text[text.Length - 1] != '\n')
                    {
                        ++count;
                    }
                }

                return count;
            }
        }
        private StringBuilder StringBuilder
        {
            get
            {
                if (_stringBuilder == null)
                    _stringBuilder = new StringBuilder();
                return _stringBuilder;
            }
        }

        public void WriteLine(string line)
        {
            var result = Regex.Split(line, "\r\n|\r|\n");
            var lines = new List<string>();
            lines.AddRange(result);

            if (lines.Count > 1)
            {

                //break into separate strings, so each one can be indented
                
                int indent = ComputeIndent(lines);
                int i = 0;
                foreach (var l in lines)
                {
                    var ll = l;
                    
                    if (i> 0 && indent > -1 && ll.Length >= indent && ll.Substring(0,indent).Trim().Length == 0)
                    {
                        //remove indent used to make code look pretty
                        
                        ll = ll.Substring(indent, l.Length - indent);
                    }

                    if (!string.IsNullOrWhiteSpace(ll))
                    {
                        if ((i == 0 && _needsIndent) || i > 0)
                        StringBuilder.Append(GetTabs());

                        StringBuilder.AppendLine(ll.TrimEnd());
                    }
                    else
                        StringBuilder.AppendLine();

                    i++;
                }

                _needsIndent = true;
                
                return;
            }

            if (!string.IsNullOrWhiteSpace(line))
            {
                if (_needsIndent)
                {
                    StringBuilder.Append(GetTabs());
                }
                StringBuilder.AppendLine(line.TrimEnd());
            }
            else
                StringBuilder.AppendLine();

            
            _needsIndent = true;
        }

        private int ComputeIndent(List<string> lines)
        {
            int i = 0;
            int indent= -1;
            foreach (var l in lines)
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }
                if (string.IsNullOrWhiteSpace(l))
                    continue;

                int indentTemp = l.TakeWhile(Char.IsWhiteSpace).Count();
                if (indent == -1)
                    indent = indentTemp;

                if (indentTemp > 0 && indentTemp < indent)
                    indent = indentTemp;
                i++;
            }
            return indent;
        }

        public void Write(string text)
        {
            if (_needsIndent)
            {
                StringBuilder.Append(GetTabs());
                _needsIndent = false;
            }
            StringBuilder.Append(text);
        }

        public void Indent()
        {
            _indentCount++;
        }

        public void Unindent()
        {
            //StringBuilder.AppendLine();

            _indentCount--;
        }


        public void Clear()
        {
            StringBuilder.Clear();
        }

        public override string ToString()
        {
            return StringBuilder.ToString();
        }

        public void WriteLine()
        {
            WriteLine(string.Empty);
        }

        private string GetTabs()
        {
            var tabs = string.Empty;
            for (var i = 0; i < _indentCount; i++)
                tabs += "    "; //VS uses 4 spaces as a tab
            return tabs;
        }

        public void RestoreIndent()
        {
            _indentCount = _savedIndent;
        }

        public void SaveIndent()
        {
            _savedIndent = _indentCount;
        }
    }
}