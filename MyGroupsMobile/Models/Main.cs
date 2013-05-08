using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PlatformSDK;

namespace MyGroups.Models
{
    public class Main
    {

        public PlatformUser CurrentUser { get; set; }

        public List<Group> Groups { get; set; }


    }
}