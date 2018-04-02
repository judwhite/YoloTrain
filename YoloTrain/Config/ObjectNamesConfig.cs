using System;
using System.Collections.Generic;

namespace YoloTrain.Config
{
    public class ObjectNamesConfig
    {
        private readonly List<string> _names = new List<string>();

        public IReadOnlyCollection<string> Names => _names;

        public bool Add(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name), $"{nameof(name)} cannot be an empty string");

            var trimmedName = name.Trim();
            foreach (var existing in _names)
            {
                if (string.Compare(trimmedName, existing, StringComparison.OrdinalIgnoreCase) == 0)
                    return false;
            }

            _names.Add(trimmedName);
            return true;
        }

        public int IndexOf(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name), $"{nameof(name)} cannot be an empty string");

            var trimmedName = name.Trim();
            for (int i = 0; i < _names.Count; i++)
            {
                var existing = _names[i];
                if (string.Compare(trimmedName, existing, StringComparison.OrdinalIgnoreCase) == 0)
                    return i;
            }
            return -1;
        }
    }
}
