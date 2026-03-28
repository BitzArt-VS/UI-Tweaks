using System;
using System.Collections.Generic;

namespace BitzArt.UI.Tweaks.Services;

internal partial class GameStatusService
{
    private class DetailRecordCollection
    {
        private readonly Dictionary<GameStatusDetailType, DetailRecord> _typeLookup;
        private readonly Dictionary<string, DetailRecord> _nameLookup;

        public DetailRecordCollection()
        {
            var maxValues = Enum.GetValues<GameStatusDetailType>().Length;

            _typeLookup = new(maxValues);
            _nameLookup = new(maxValues);
        }

        public void Add(DetailRecord record)
        {
            if (!_typeLookup.TryAdd(record.Detail, record))
            {
                throw new ArgumentException($"A record for detail {record.Detail} already exists.");
            }

            if (!_nameLookup.TryAdd(record.Name, record))
            {
                throw new ArgumentException($"A record with name {record.Name} already exists.");
            }
        }

        public DetailRecord Get(GameStatusDetailType detail)
        {
            return _typeLookup[detail];
        }

        public DetailRecord Get(string name)
        {
            return _nameLookup[name];
        }
    }
}
