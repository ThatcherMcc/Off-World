using System;
using System.Collections.Generic;
using UnityEngine;
using BossFight.Strategies;
using System.Linq;
using UnityEditor;


namespace BossFight.BehaviorTrees
{
    public class RandomSelector : PrioritySelector
    {
        protected override List<Node> SortChildren() => children.Shuffle().ToList();

        public RandomSelector(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            // 1. Get the newly shuffled list. This triggers SortChildren()
            //    and creates a new, randomly ordered list every time.
            var randomChildren = SortedChildren;

            // 2. We only want to execute the FIRST child in the random list.
            if (randomChildren.Count == 0)
            {
                // Reset the list so it can be shuffled again next time
                sortedChildren = null;
                return Status.Failure;
            }

            // 3. Execute the first child (the one chosen randomly)
            int selectedIndex = 0;
            var status = randomChildren[selectedIndex].Process();

            switch (status)
            {
                case Status.Running:
                    // Note: We don't need runningChildIndexInSorted here 
                    // because we only process one child and expect it to finish 
                    // or keep running until the next tick.
                    runningChildIndexInSorted = selectedIndex;
                    return Status.Running;

                case Status.Success:
                    // Success: The strategy is complete. Reset for next random pick.
                    randomChildren[selectedIndex].Reset();
                    Reset(); // Call the base reset to clear state
                    return Status.Success;

                case Status.Failure:
                default:
                    // Failure: The one random choice failed. The whole selector fails.
                    Reset(); // Call the base reset to clear state
                    return Status.Failure;
            }
        }

        // --- New Reset Method for Clean Slate ---
        public override void Reset()
        {
            base.Reset();
            // Crucially, reset the sortedChildren list so it is re-shuffled next time
            // The base Reset() already does this, but it's good to be explicit.
            sortedChildren = null;
        }
    }

    // selector but adds a priority concept that sorts children then loops through them based on priority int.
    public class PrioritySelector : Selector
    {
        protected List<Node> sortedChildren;
        protected List<Node> SortedChildren => sortedChildren ??= SortChildren();
        protected int runningChildIndexInSorted = -1;

        protected virtual List<Node> SortChildren() => children.OrderByDescending(child => child.priority).ToList();

        public PrioritySelector(string name, int priority = 0) : base(name, priority) { }

        public override void Reset()
        {
            base.Reset();
            runningChildIndexInSorted = -1;
            sortedChildren = null;
        }

        public override Status Process()
        {
            var sorted = SortedChildren;

            // If we have a running child, continue with it
            if (runningChildIndexInSorted >= 0 && runningChildIndexInSorted < sorted.Count)
            {
                var status = sorted[runningChildIndexInSorted].Process();
                if (status == Status.Running)
                {
                    return Status.Running;
                }
                else if (status == Status.Success)
                {
                    runningChildIndexInSorted = -1;
                    return Status.Success; // DON'T call Reset() here!
                }
                else // Failure
                {
                    runningChildIndexInSorted = -1;
                    // Continue checking other children
                }
            }

            // Try children by priority
            int startIndex = (runningChildIndexInSorted >= 0) ? runningChildIndexInSorted + 1 : 0;

            for (int i = startIndex; i < sorted.Count; i++)
            {
                var status = sorted[i].Process();

                switch (status)
                {
                    case Status.Running:
                        runningChildIndexInSorted = i;
                        return Status.Running;
                    case Status.Success:
                        sorted[i].Reset();
                        runningChildIndexInSorted = -1;
                        sortedChildren = null;
                        return Status.Success;
                    default: // Failure
                        continue;
                }
            }

            runningChildIndexInSorted = -1;
            sortedChildren = null;
            return Status.Failure;
        }
    }

    // essentially your 'OR' operator. loops through until it finds a success then resets.
    public class Selector : Node
    {
        protected int runningChildIndex = -1; // Track which child is currently running

        public Selector(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            // If we have a running child, continue with it
            if (runningChildIndex >= 0)
            {
                var status = children[runningChildIndex].Process();
                if (status == Status.Running)
                {
                    return Status.Running;
                }
                else if (status == Status.Success)
                {
                    runningChildIndex = -1;
                    currentChild = 0; // Reset to start
                    return Status.Success;
                }
                else // Failure
                {
                    runningChildIndex = -1;
                    currentChild++; // Try next child
                }
            }

            // Try children in order
            while (currentChild < children.Count)
            {
                var status = children[currentChild].Process();

                switch (status)
                {
                    case Status.Running:
                        runningChildIndex = currentChild;
                        return Status.Running;
                    case Status.Success:
                        currentChild = 0; // Reset for next time, but DON'T call Reset()
                        return Status.Success;
                    default: // Failure
                        currentChild++;
                        break;
                }
            }

            currentChild = 0; // Reset for next time
            return Status.Failure;
        }

        public override void Reset()
        {
            runningChildIndex = -1;
            base.Reset();
        }
    }

    // essentially an 'AND' operator where it will loop through the children left to right and if one fails it resets
    public class Sequence : Node
    {
        public Sequence(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            // If we're past the end, we completed successfully last time
            if (currentChild >= children.Count)
            {
                currentChild = 0;
                return Status.Success;
            }

            // Get current child and process it
            var child = children[currentChild];
            var status = child.Process();

            Debug.Log($"Sequence '{name}': Child {currentChild} '{child.name}' returned {status}");

            switch (status)
            {
                case Status.Running:
                    return Status.Running;

                case Status.Failure:
                    Debug.Log($"Sequence '{name}': Failed at child {currentChild}, resetting");
                    currentChild = 0;
                    return Status.Failure;

                default: // Success
                    currentChild++;

                    // If we completed all children
                    if (currentChild >= children.Count)
                    {
                        Debug.Log($"Sequence '{name}': Completed all children successfully");
                        currentChild = 0;
                        return Status.Success;
                    }

                    Debug.Log($"Sequence '{name}': Moving to child {currentChild}");
                    // Continue to next child on next Process() call
                    return Status.Running;
            }
        }

        public override void Reset()
        {
            Debug.Log($"Sequence '{name}': Reset called");
            base.Reset();
        }
    }

    // Sequence that runs onStart when it begins and onComplete when it ends (success or failure)
    public class GuardedSequence : Node 
    {
        private readonly Action onStart; // Runs when sequence starts
        private readonly Action onComplete; // Runs on success OR failure
        private bool hasStarted = false; // Tracks if sequence has started
        private bool cleanupRan = false; // Tracks if cleanup has run

        public GuardedSequence(string name, Action onStart = null, Action onComplete = null, int priority = 0)
            : base(name, priority)
        {
            this.onStart = onStart;
            this.onComplete = onComplete;
        }

        public override Status Process()
        {
            // Don't call onStart until we actually start processing children successfully

            // If we're past the end, we completed successfully last time
            if (currentChild >= children.Count)
            {
                if (!cleanupRan)
                {
                    onComplete?.Invoke();
                    cleanupRan = true;
                }
                // Reset for next time
                currentChild = 0;
                hasStarted = false;
                cleanupRan = false;
                base.Reset();
                return Status.Success; // we've made it to the end will no issues, SUCCESS
            }

            // Get current child and process it
            var child = children[currentChild];
            var status = child.Process();

            Debug.Log($"GuardedSequence '{name}': Child {currentChild} '{child.name}' returned {status}");

            switch (status)
            {
                case Status.Running:
                    // First time we get a Running status, call onStart
                    if (!hasStarted)
                    {
                        onStart?.Invoke();
                        hasStarted = true;
                    }
                    return Status.Running;

                case Status.Failure:
                    Debug.Log($"GuardedSequence '{name}': Failed at child {currentChild}, running cleanup");
                    if (!cleanupRan && hasStarted)
                    {
                        onComplete?.Invoke(); // Cleanup even on failure
                        cleanupRan = true;
                    }
                    currentChild = 0;
                    hasStarted = false;
                    cleanupRan = false;
                    base.Reset();
                    return Status.Failure;

                default: // Success
                         // First success means we've started executing
                    if (!hasStarted && currentChild == 0)
                    {
                        onStart?.Invoke();
                        hasStarted = true;
                    }

                    currentChild++;

                    // If we completed all children
                    if (currentChild >= children.Count)
                    {
                        Debug.Log($"GuardedSequence '{name}': Completed all children successfully");
                        if (!cleanupRan)
                        {
                            onComplete?.Invoke();
                            cleanupRan = true;
                        }
                        currentChild = 0;
                        hasStarted = false;
                        cleanupRan = false;
                        base.Reset();
                        return Status.Success;
                    }

                    Debug.Log($"GuardedSequence '{name}': Moving to child {currentChild}");
                    return Status.Running;
            }
        }

        public override void Reset()
        {
            if (!cleanupRan && hasStarted)
            {
                Debug.Log($"GuardedSequence '{name}': Reset called, running cleanup");
                onComplete?.Invoke();
            }
            hasStarted = false;
            cleanupRan = false;
            base.Reset();
        }
    }
    // the ends of our tree that either have action to complete or conditions to follow
    public class Leaf : Node
    {
        readonly IStrategy strategy; // the strategy (its function or job) that this leaf will perform

        public Leaf(string name, IStrategy strategy, int priority = 0) : base(name, priority)
        {
            this.strategy = strategy;
        }

        public override Status Process() => strategy.Process(); // process the strategy and return its status

        public override void Reset() => strategy.Reset(); // reset the strategy if needed
    }

    // our root node that will loop through all children and make sure they run Successfully
    public class BehaviorTree : Node
    {
        public BehaviorTree(string name) : base(name) { }

        public override Status Process()
        {
            // Behavior trees typically just run their main child continuously
            // They don't cycle through multiple children like a sequence

            if (children.Count == 0)
            {
                return Status.Failure;
            }

            // Just process the first (and typically only) child
            // The child is usually a Selector or Sequence that handles the logic
            var status = children[0].Process();

            // Behavior tree always returns Running to keep the loop going
            // The internal nodes handle success/failure
            return Status.Running;
        }

        public override void Reset()
        {
            // Only reset if explicitly called (like when restarting the entire behavior)
            base.Reset();
        }
    }

    // the base class of node that all these pieces fall under. everything is a node
    public class Node
    {
        public enum Status { Success, Failure, Running } // the possible states a node can be in

        public readonly string name; // name of the node for debugging
        public readonly int priority; // priority of the node for priority selectors

        public readonly List<Node> children = new(); // list of child nodes
        protected int currentChild; // index of the current child being processed

        public Node(string name = "Node", int priority = 0)
        {
            this.name = name;
            this.priority = priority;
        }

        public void AddChild(Node child) => children.Add(child); // add a child node

        public virtual Status Process() => children[currentChild].Process(); // process the current child node

        public virtual void Reset() // reset the node and its children
        {
            currentChild = 0;
            foreach (var child in children)
            {
                child.Reset();
            }
        }
    }

}

