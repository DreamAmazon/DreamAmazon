using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DreamAmazon.Controls
{
    class CustomNumericUpDown : NumericUpDown
    {
        private int _currentIndex;

        private decimal[] _possibleValues;

        public decimal[] PossibleValues
        {
            get
            {
                if (_possibleValues == null)
                {
                    _possibleValues = GetPossibleValues().ToArray();
                }
                return _possibleValues;
            }
        }

        public override void UpButton()
        {
            if (UserEdit)
            {
                ParseEditText();
            }
            var values = PossibleValues;
            _currentIndex = Math.Min(_currentIndex + 1, values.Length - 1);
            Value = values[_currentIndex];
        }

        public override void DownButton()
        {
            if (UserEdit)
            {
                ParseEditText();
            }
            var values = PossibleValues;
            _currentIndex = Math.Max(_currentIndex - 1, 0);
            Value = values[_currentIndex];
        }

        private IEnumerable<decimal> GetPossibleValues()
        {
            foreach (var value in new decimal[] { -1, 1, 2, 3, 4, 6, 12, 24 })
            {
                yield return value;
            }
            for (decimal i = 24; i < Maximum; i += 24)
            {
                yield return i;
            }
        }
    }
}