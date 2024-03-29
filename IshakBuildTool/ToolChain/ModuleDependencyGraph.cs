﻿using IshakBuildTool.Project.Modules;
using System.Text;

namespace IshakBuildTool.ToolChain
{

    public class ModuleDependencyTreeNode
    {


        public IshakModule? Module;
        public List<ModuleDependencyTreeNode> Depencencies = new List<ModuleDependencyTreeNode>();

        public ModuleDependencyTreeNode() { }

        public ModuleDependencyTreeNode(IshakModule module)
        {
            Module = module;
        }

        public void SetDepencency(ModuleDependencyTreeNode depencencyNode)
        {
            if (!Depencencies.Contains(depencencyNode))
            {
                Depencencies.Add(depencencyNode);
            }
        }

        
    }

    /**
     * Dependency Tree that outputs a sorted List of Nodes that indicates the order of compilation of the Engine Modules.
     * In this case we explore each node and its dependencies, when a node with no dependency is found, we add it to the 
     * sorted list, following this method, we build the modules from the less dependant to the most dependant.
     *      
     * */
    public class ModuleDependencyTree
    {
        public List<ModuleDependencyTreeNode> Nodes = new List<ModuleDependencyTreeNode>();

        public ModuleDependencyTree() { }

        public ModuleDependencyTree(List<ModuleDependencyTreeNode> nodes)
        {
            Nodes = nodes;
        }

        public List<IshakModule> GetDependentSortedModules()
        {
            List<IshakModule> sortedModules = new List<IshakModule>();
            List<IshakModule> visited = new List<IshakModule>();

            foreach (ModuleDependencyTreeNode nodeToExplore in Nodes)
            {
                ExploreNodeAndSortModule(sortedModules, nodeToExplore, visited);
            }

            return sortedModules;
        }

        // We explore nodes until we find a node with no dependencies, then we build the list from bottom up.
        void ExploreNodeAndSortModule(List<IshakModule> out_sortedModule, ModuleDependencyTreeNode nodeToExplore, List<IshakModule> visited)
        {
            foreach (ModuleDependencyTreeNode dependentNode in nodeToExplore.Depencencies)
            {
                if (visited.Contains(dependentNode.Module))
                {
                    continue;
                }
                else
                {
                    visited.Add(dependentNode.Module);
                }

                ExploreNodeAndSortModule(out_sortedModule, dependentNode, visited);

            }

            if (nodeToExplore.Module == null)
            {
                return;
            }

            if (!out_sortedModule.Contains(nodeToExplore.Module))
            {
                out_sortedModule.Add(nodeToExplore.Module);
            }
        }

    }   
}
