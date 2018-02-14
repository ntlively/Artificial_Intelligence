using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using Vision;

 namespace BayesianUtils
 {
 
     public class BayesNode
     {
    
        public List<BayesNode> _children;
        // public List<BayesNode> _siblings;
        public Dictionary<string, float> _fuzzyStates;

        public BayesNode
        (
            //optional parameter
            List<BayesNode> children = default(List<BayesNode>),
            Dictionary<string, float> states = default(Dictionary<string, float>)
        )
        {
            //checks if parameter exists, if not, default to empty list
             _children = children ?? new List<BayesNode>();
             _fuzzyStates = states ?? new Dictionary<string, float>();

         }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
        public void AddChild(BayesNode child)
        {
            _children.Add(child);
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
        // public void AddSibling(BayesNode child)
        // {
        //     _siblings.Add(child);
        // }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//

        public void Update(int information, Queue nodeQueue)
        {
            // Do something with information to update dictionary of states




            nodeQueue.Dequeue();
            // Tell siblings to do the same
            // for(int i=0;i<_siblings.Count;i++)
            // {
            //     nodeQueue.Enqueue(_siblings[i]);
            // }
            // Tell children to do the same
            for(int i=0;i<_children.Count;i++)
            {
                //_children[i].information = information;
                //information = newInfo;
                nodeQueue.Enqueue(_children[i]);;
            }

        }
     }

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
	public class BayesNet
     {
          
        //public BayesNode _header;
        public string name;
        public List<BayesNode> _nodes;
          
        public BayesNet
        (
            //optional parameter
            List<BayesNode> nodes = default(List<BayesNode>)
        )
        {
             //_header = new BayesNode();
             name = "NETWORK";

            //checks if parameter exists, if not, default to empty list
             _nodes = nodes ?? new List<BayesNode>();

            //first node is a blank header node
            // _nodes.Add(new BayesNode());

         }

        public void AddNode(BayesNode parent, BayesNode child)
        {
            _nodes.Add(child);
            parent.AddChild(child);
        }

        // Update Bayes Net with sensor information, and then return the best option
        public Dictionary<string, float> Predict(int information)
        {
            int info = information;
            BayesNode header = _nodes[0];
            var nodeQueue = new Queue<BayesNode>();
            // Loop through the tree, and have each node update it's probability values based on info
            for(int i=0;i< header._children.Count;i++)
            {
                nodeQueue.Enqueue(header._children[i]);
                // header._children[i].Update(information, nodeQueue);
            }

            while(nodeQueue.Count>0)
            {
               // BayesNode tempNode = nodeQueue.Peek();
               // tempNode.Update(info, nodeQueue);
            }

            //assuming that the last node in the tree will be the final options
            return _nodes.Last()._fuzzyStates;
        }

        public void Update()
        {
            //_header.Update();
        }
      
     }
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
     
 }
