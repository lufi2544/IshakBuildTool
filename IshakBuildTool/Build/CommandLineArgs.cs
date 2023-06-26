using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Build
{
    internal class CommandLineArgs
    {
        /** Dictionary that holds all the command line arguments.  */
        Dictionary<string, string> ArgsDictionary;

        public CommandLineArgs(string[] commandLineArgs) 
        {
            ArgsDictionary = new Dictionary<string, string>();
            
            string cachedArgCategory = string.Empty;   
            foreach (string arg in commandLineArgs) 
            {
                if (cachedArgCategory == string.Empty)
                {
                    cachedArgCategory = arg;
                    continue;
                }
                else
                {
                    // Add the category with its equivalent argument
                    ArgsDictionary[cachedArgCategory] = arg;
                    cachedArgCategory = string.Empty;
                }                
            }
        }


        public string GetArgumentFromCategory(string category, out bool bFound)
        {
            if (ArgsDictionary.ContainsKey(category))
            {
                bFound = true;
                return ArgsDictionary[category];
            }

            bFound = false;
            return string.Empty;
        }

        
    }
}
