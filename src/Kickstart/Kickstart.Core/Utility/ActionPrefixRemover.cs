using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Utility
{
    public static class ActionPrefixRemover
    {
        public static string Remove(string name)
        {
            if (name.StartsWith("Get"))
            {
                name = name.Substring(3, name.Length - 3);
            }
            if (name.StartsWith("Is"))
            {
                name = name.Substring(2, name.Length - 2);
            }

            if (name.StartsWith("Read"))
            {
                name = name.Substring(4, name.Length - 4);
            }
            if (name.StartsWith("List"))
            {
                name = name.Substring(4, name.Length - 4);
            }
            if (name.StartsWith("Add"))
            {
                name = name.Substring(3, name.Length - 3);
            }
            if (name.StartsWith("Save"))
            {
                name = name.Substring(4, name.Length - 4);
            }
            if (name.StartsWith("Approve"))
            {
                name = name.Substring(7, name.Length - 7);
            }
            if (name.StartsWith("Find"))
            {
                name = name.Substring(4, name.Length - 4);
            }
            if (name.StartsWith("Check"))
            {
                name = name.Substring(5, name.Length - 5);
            }
            if (name.StartsWith("Create"))
            {
                name = name.Substring(6, name.Length - 6);
            }
            if (name.StartsWith("Update"))
            {
                //name = name.Substring(6, name.Length - 6);
            }

            if (name.StartsWith("Queue"))
            {
                name = name.Substring(5, name.Length - 5);
            }
            if (name.StartsWith("Dequeue"))
            {
                name = name.Substring(7, name.Length - 7);
            }

            if (name.StartsWith("All"))
            {
                name = name.Substring(3, name.Length - 3);
            }

            return name;
        }
    }
}
