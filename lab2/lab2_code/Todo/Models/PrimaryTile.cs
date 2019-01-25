using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.Models
{
    //已弃用
    public class PrimaryTile
    {
        public string title { get; set; } 
        public string description { get; set; } 
        public string branding { get; set; } 
        public string appName { get; set; } = "Todo";

        public PrimaryTile(string title, string description)
        {
            this.title = title;
            this.description = description;
        }
    }

}
