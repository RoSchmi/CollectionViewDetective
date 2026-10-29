using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace CollectionViewDetective.Models
{
    public partial class EntityDisplayItem : ObservableObject
    {
        [ObservableProperty]

        private string entityRow;

        //public partial string EntityRow { get; set; } = string.Empty;
        public EntityDisplayItem(string rowContent)
        {
            EntityRow = rowContent;
        }
    }
}
