using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PromptCLI
{
    public class CheckboxComponent<T> : ComponentBase, IComponent<IEnumerable<T>>
    {
        private readonly Input<IEnumerable<T>> _input;
        private readonly List<Option<T>> _selects;
        private readonly bool[] _status;
        public Action<Input<IEnumerable<T>>> CallbackAction { get; private set; }

        public Range Range => _range;

        public Input<IEnumerable<T>> Result => _input;
        public bool IsCompleted { get; set; }

        public CheckboxComponent(Input<IEnumerable<T>> input, List<Option<T>> selects)
        {
            _input = input;
            _selects = selects;
            _range = 1..2;
            _regex = "^[ ]";
            _status = new bool[_selects.Count];
        }


        public void Draw(bool defaultValue = true)
        {
            Console.Write(prefix, ConsoleColor.Green);
            Console.WriteLine(_input.Text);

            foreach(var item in _selects)
            {
                Console.WriteLine(string.Format("[ ] {0}", item.Text));
            }

            SetPosition();
        }

        public void Handle(ConsoleKeyInfo act)
        {
            var (result, key) = IsKeyAvailable(act);
            if (result == KeyInfo.Unknown)
            {
                ClearCurrentPosition();
                return;
            }
            else if (result == KeyInfo.Direction)
            {
                Direction(key);
                return;
            }

            var index = _cursorPointTop - _offsetTop - 1;

            _status[index] = !_status[index];

            WriteCurrent(_status[index] ? '•' : ' ', ConsoleColor.DarkRed);
        }

        public void SetTopPosition(int top)
        {
            _offsetTop = top;
            _cursorPointTop = top + 1; // offset 1 for input at the begining
            _cursorPointLeft = _range.Start.Value;
            _maxTop = _selects.Count + 1;
        }

        public int GetTopPosition()
        {
            return 1;
        }

        public void Complete()
        {
            // Clear all drawed lines and set the cursor into component start position
            for (int i = 0; i < _selects.Count + 1; i++)
            {
                Console.ClearLine(_offsetTop + i);
            }
            
            _cursorPointLeft = 0;
            _cursorPointTop = _offsetTop;
            SetPosition();

            // Write the result
            Console.Write(_input.Text);
            Console.Write(" > ");
            Console.WriteLine(string.Join(",", Result), ConsoleColor.Cyan);

            CallbackAction(this.Result);
        }

        public void Bind(Prompt prompt)
        {
            _prompt = prompt;
        }

        public Prompt Callback(Action<Input<IEnumerable<T>>> callback)
        {
            CallbackAction = callback;
            return _prompt;
        }
    }

}