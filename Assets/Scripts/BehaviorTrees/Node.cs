using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BossFight.Strategies;
using System.Linq;
using UnityEditor;


namespace BossFight.BehaviorTrees
{
    // Run a Node until failure
    public class UntilFail : Node
    {
        public UntilFail(string name) : base(name) { }

        public override Status Process()
        {
            if (children[0].Process() == Status.Failure)
            {
                Reset();
                return Status.Failure;
            }

            return Status.Running;
        }
    }
    // 'Not' : success = failure and failure = success
    public class Invertor : Node
    {
        public Invertor(string name) : base(name) { }

        public override Status Process()
        {
            switch (children[0].Process())
            {
                case Status.Running:
                    return Status.Running;
                case Status.Failure:
                    return Status.Success;
                default:
                    return Status.Failure;
            }
        }
    }
    // selector that randomizes the order of its children
    public class RandomSelector : PrioritySelector
    {
        protected override List<Node> SortChildren() => children.Shuffle().ToList();

        public RandomSelector(string name, int priority = 0) : base(name, priority) { }
    }
    // selector but adds a priority concept that sorts children then loops through them based on priority int.
    public class PrioritySelector : Selector
    {
        List<Node> sortedChildren;
        List<Node> SortedChildren => sortedChildren ??= SortChildren();

        protected virtual List<Node> SortChildren() => children.OrderByDescending(child => child.priority).ToList();
        public PrioritySelector(string name, int priority) : base(name, priority) { }

        public override void Reset()
        {
            base.Reset();
            sortedChildren = null;
        }

        public override Status Process()
        {
            foreach (var child in SortedChildren)
            {
                switch (child.Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        return Status.Success;
                    default:
                        continue;
                }
            }

            return Status.Failure;
        }
    }

    // essentially your 'OR' operator. loops through until it finds a success then resets.
    public class Selector : Node
    {
        public Selector(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            if (currentChild < children.Count)
            {
                switch (children[currentChild].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        currentChild++;
                        return Status.Failure;
                }
            }

            Reset();
            return Status.Failure;
        }
    }

    // essentially an 'AND' operator where it will loop through the children left to right and if one fails it resets
    public class Sequence : Node
    {
        public Sequence(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            if (currentChild < children.Count)
            {
                switch (children[currentChild].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        Reset();
                        return Status.Failure;
                    default:
                        currentChild++;
                        return (currentChild == children.Count) ? Status.Success : Status.Running;
                }
            }

            Reset();
            return Status.Success;
        }

    }


    // the ends of our tree that either have action to complete or conditions to follow
    public class Leaf : Node
    {
        readonly IStrategy strategy;

        public Leaf(string name, IStrategy strategy, int priority = 0) : base(name, priority)
        {
            this.strategy = strategy;
        }

        public override Status Process() => strategy.Process();

        public override void Reset() => strategy.Reset();
    }


    // the base class of node that all these pieces fall under. everything is a node
    public class Node
    {
        public enum Status { Success, Failure, Running }

        public readonly string name;
        public readonly int priority;

        public readonly List<Node> children = new();
        protected int currentChild;

        public Node(string name = "Node", int priority = 0)
        {
            this.name = name;
            this.priority = priority;
        }

        public void AddChild(Node child) => children.Add(child);

        public virtual Status Process() => children[currentChild].Process();

        public virtual void Reset()
        {
            currentChild = 0;
            foreach (var child in children)
            {
                child.Reset();
            }
        }
    }


    // our root node that will loop through all children and make sure they run Successfully
    public class BehaviorTree : Node
    {
        public BehaviorTree(string name) : base(name) { }

        public override Status Process()
        {
            // Process the current child
            var status = children[currentChild].Process();

            // If the current child is running, we continue to run.
            if (status == Status.Running)
            {
                return Status.Running;
            }
            else // Child returned Success or Failure
            {
                currentChild++; // Move to the next child

                // If we've processed all children, reset and restart the tree
                if (currentChild >= children.Count)
                {
                    Reset();
                    return Status.Running; 
                }
                else
                {
                    return Status.Running;
                }
            }
        }

        public override void Reset()
        {
            base.Reset();
        }
    }

}

