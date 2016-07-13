﻿using Neptuo.Collections.Specialized;
using Neptuo.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptuo.Productivity.SolutionRunner.Services.Applications
{
    public class AdditionalApplicationCollection : ICompositeModel
    {
        private List<AdditionalApplicationModel> items;

        public IEnumerable<AdditionalApplicationModel> Items
        {
            get { return items; }
        }

        public AdditionalApplicationCollection()
            : this(new List<AdditionalApplicationModel>())
        { }

        public AdditionalApplicationCollection(IEnumerable<AdditionalApplicationModel> items)
        {
            Ensure.NotNull(items, "items");
            this.items = new List<AdditionalApplicationModel>(items);
        }

        public void Load(ICompositeStorage storage)
        {
            items.Clear();
            int count = storage.Get<int>("Count", 0);
            for (int i = 0; i < count; i++)
            {
                ICompositeStorage itemStorage;
                if (storage.TryGet(i.ToString(), out itemStorage))
                {
                    string name = itemStorage.Get<string>("Name");
                    string path = itemStorage.Get<string>("Path");
                    string arguments = itemStorage.Get<string>("Arguments", null);
                    items.Add(new AdditionalApplicationModel(name, path, arguments));
                }
            }
        }

        public void Save(ICompositeStorage storage)
        {
            storage.Add("Count", items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                AdditionalApplicationModel item = items[i];
                ICompositeStorage itemStorage = storage.Add(i.ToString());
                itemStorage.Add("Name", item.Name);
                itemStorage.Add("Path", item.Path);
                itemStorage.Add("Arguments", item.Arguments);
            }
        }
    }
}
