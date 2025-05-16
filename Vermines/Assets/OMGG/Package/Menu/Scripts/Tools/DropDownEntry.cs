using System.Collections.Generic;
using System.Linq;
using System;

namespace OMGG.Menu.Tools {

    using Dropdown = TMPro.TMP_Dropdown;

    /// <summary>
    /// A helper class that maps a option name into the actual value.
    /// Is used to simplifiy dropdown UI, for example in <see cref="Configuration.MenuGraphicsSettings"/>.
    /// </summary>
    /// <typeparam name="T">The option value type</typeparam>
    public class DropDownEntry<T> where T : IEquatable<T> {

        /// <summary>
        /// Creates an option for this dropdown element.
        /// </summary>
        /// <param name="dropdown">Dropdown UI element</param>
        /// <param name="onValueChanged">Forward the value chaged callback</param>
        public DropDownEntry(Dropdown dropdown, Action onValueChanged)
        {
            _Dropdown = dropdown;
            _Dropdown.onValueChanged.RemoveAllListeners();
            _Dropdown.onValueChanged.AddListener(_ => onValueChanged.Invoke());
        }

        private Dropdown _Dropdown;

        private List<T> _Options;

        /// <summary>
        /// Returns the value of this option.
        /// </summary>
        public T Value => _Options == null || _Options.Count == 0 ? default(T) : _Options[_Dropdown.value];

        /// <summary>
        /// Clear all options and set new.
        /// </summary>
        /// <param name="options">List of options</param>
        /// <param name="current">The current selected option</param>
        /// <param name="ToString">A callback to format the option text</param>
        public void SetOptions(List<T> options, T current, Func<T, string> ToString = null)
        {
            _Options = options;

            _Dropdown.ClearOptions();
            _Dropdown.AddOptions(options.Select(o => ToString != null ? ToString(o) : o.ToString()).ToList());

            var index = _Options.FindIndex(0, o => o.Equals(current));

            if (index >= 0)
                _Dropdown.SetValueWithoutNotify(index);
            else
                _Dropdown.SetValueWithoutNotify(0);
        }
    }
}
