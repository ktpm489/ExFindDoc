using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileExplorer.SortItem
{
    class SortItems
    {
        public StorageFile File { get; set; }
        public DateTime LastModified { get; set; }
        public string DisplayName { get; set; }
    }
}
