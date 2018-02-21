using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using Vision;
//
 namespace BayesianUtils
 {
 
     public class BayesNode
     {
    
        public List<BayesNode>              _children;
        public List<float>                  _inputMod;
        public Dictionary<string, float>    _input;
        public Dictionary<string, float>    _fuzzyStates;
        
        public BayesNode testNode;

        public BayesNode
        (
            //optional parameter
            List<BayesNode>             children,
            List<float>                 inputMod,
            Dictionary<string, float>   input   ,
            Dictionary<string, float>   states
        )
        {
            //checks if parameter exists, if not, default to empty list
            _children =     children ?? new List<BayesNode>();
            _inputMod =     inputMod ?? new List<float>();
            _input =        input    ?? new Dictionary<string, float>();
            _fuzzyStates =  states   ?? new Dictionary<string, float>();

            //testNode = new BayesNode();
            foreach(BayesNode child in _children)
            {
                foreach(KeyValuePair<string, float> parent in _fuzzyStates)
                {
                    child._input.Add( parent.Key, parent.Value);
                }
            }


            // this._fuzzyStates["running"] = 0.1f;

            // foreach(BayesNode child in _children)
            // {
            //     foreach(KeyValuePair<string, float> parent in _fuzzyStates)
            //     {
            //         //child._input.Add(parent.Key, parent.Value);
            //         //Debug.Log("init child input:"+ child._input["running"]);
            //         float temp = 0.0f;
            //         if(child._input.TryGetValue("running", out temp)){
            //             testNode = child;
            //             //Debug.Log("saved node with input examples");
            //             Debug.Log("changed savedNode input:"+ testNode._input["running"]);
            //         }
            //     }
            // }


            //need to test the above to see if the input values will update automatically from parent to child.  If it's copy by value
            //then I will need to push the changes manually.
            //AFTER TESTING, IT DOES NOT UPDATE AUTOMATICALLY, WILL HAVE TO LOOP THROUGH AGAIN AND UPDATE THEM.


         }
        //~~~~~~~~~~~~~~needs to be changed to init input of child when added~~~~~~~~~~~~~~~~~//
        // public void AddChild(BayesNode child)
        // {
        //     _children.Add(child);
        // }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
        // public void PushInputs()
        // {
        //     for(int i=0; i< _children.Count;i++)
        //     {
                
        //         foreach(KeyValuePair<K, V> parent in _fuzzyStates)
        //         {
        //             foreach(KeyValuePair<K, V> child in _children[i]._input)
        //             {
        //                 if(child.Key == parent.Key)
        //                 {
        //                     child.Value = parent.Value;
        //                 }
        //                 else
        //                 {
        //                     child.Add(parent.Key,parent.Value);
        //                 }
        //             } 
        //         }
        //     }
        // }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
//UPDATE FUNCTION NEEDS TO BE TESTED THOROUGHLY.  ALSO NOTE THAT INFO IS NEVER USED, HEADER SENSOR INPUT INTO FIRST TREE NODE NEEDS TO BE FIGURED OUT
        public void Update(float information, Queue<BayesNode> nodeQueue)
        {
            // Do something with information to update dictionary of states

            int counter = 0;
            int counter2 =0;
            List<float> values = new List<float>();

            foreach(KeyValuePair<string, float> input in _input)
            {
                values.Add( _inputMod[counter] * input.Value );
                counter++;
            }

            foreach(KeyValuePair<string, float> state in _fuzzyStates)
            {
                _fuzzyStates[state.Key] = values[counter2];
                counter2++;
            }

            nodeQueue.Dequeue();
            counter = 0;
            counter2 =0;

            foreach(BayesNode child in _children)
            {
                foreach(KeyValuePair<string, float> parent in _fuzzyStates)
                {
                    child._input[parent.Key] = parent.Value;
                }
            }

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

        // public void AddNode(BayesNode parent, BayesNode child)
        // {
        //     _nodes.Add(child);
        //     parent.AddChild(child);
        // }

        // Update Bayes Net with sensor information, and then return the best option
        public Dictionary<string, float> Predict(float information)
        {
            //int info = information;
            BayesNode header = _nodes[_nodes.Count-1];
            var nodeQueue = new Queue<BayesNode>();
            // Loop through the tree, and have each node update it's probability values based on info

            nodeQueue.Enqueue(header);
            for(int i=0;i< header._children.Count;i++)
            {
                nodeQueue.Enqueue(header._children[i]);
                // header._children[i].Update(information, nodeQueue);
            }

            while(nodeQueue.Count>0)
            {
               BayesNode tempNode = nodeQueue.Peek();
               tempNode.Update(information, nodeQueue);
               //nodeQueue.Dequeue();
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




            // //optional parameter
            // List<BayesNode>             children  = default(List<BayesNode>),
            // List<float>                 inputMod  = default(List<float>),
            // Dictionary<string, float>   input     = default(Dictionary<string, float>),
            // Dictionary<string, float>   states    = default(Dictionary<string, float>)