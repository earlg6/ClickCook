using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClickCook.Data
{
    [Serializable]
    public sealed class UpgradeNodePrerequisite
    {
        public string nodeId;
        // -1 means the predecessor's own maximum; never a source screenshot's cap.
        public int rank;
    }

    [Serializable]
    public sealed class UpgradeTreeNode
    {
        public string id;
        public string familyId;
        public string title;
        public string description;
        public UpgradeNodePrerequisite[] prerequisites;
        public bool IsRoot => id == UpgradeTreeDefinition.RootId;
        public bool IsWorking => UpgradeNodeRules.MaximumRank(id) > 0;
    }

    /// <summary>Presentation contract. Does not expand the purchase catalog or save whitelist.</summary>
    [Serializable]
    public sealed class UpgradeTreeDefinition
    {
        public const string RootId = "node.kitchen_license.01";
        public const string ClickStrengthNodeId = "node.click_strength.01";
        public UpgradeTreeNode[] nodes;
        public UpgradeTreeNode Find(string id) => Array.Find(nodes, n => n.id == id);

        public static UpgradeTreeDefinition Load()
        {
            var source = Resources.Load<TextAsset>("UpgradeTreeNodes");
            if (source == null) throw new InvalidOperationException("Missing UpgradeTreeNodes content.");
            var tree = JsonUtility.FromJson<UpgradeTreeDefinition>(source.text.TrimStart('\uFEFF'));
            tree.Validate();
            return tree;
        }

        public void Validate()
        {
            if (nodes == null || nodes.Length != 92) throw new InvalidOperationException("Expected 92 upgrade tiles.");
            var ids = new HashSet<string>();
            foreach (var node in nodes)
                if (node == null || string.IsNullOrWhiteSpace(node.id) || !ids.Add(node.id) ||
                    string.IsNullOrWhiteSpace(node.familyId) || string.IsNullOrWhiteSpace(node.title) ||
                    string.IsNullOrWhiteSpace(node.description) || node.prerequisites == null)
                    throw new InvalidOperationException("Invalid upgrade tile.");
            if (Find(RootId) == null || Find(ClickStrengthNodeId) == null)
                throw new InvalidOperationException("Missing root or legacy Click Strength.");
            foreach (var node in nodes)
            {
                if (node.IsRoot != (node.prerequisites.Length == 0)) throw new InvalidOperationException("Disconnected upgrade tile.");
                foreach (var need in node.prerequisites)
                    if (Find(need.nodeId) == null || (need.rank != -1 && need.rank < 1))
                        throw new InvalidOperationException("Invalid prerequisite.");
                Depth(node, new HashSet<string>());
            }
        }

        private int Depth(UpgradeTreeNode node, HashSet<string> visiting)
        {
            if (!visiting.Add(node.id)) throw new InvalidOperationException("Upgrade dependency cycle.");
            int depth = 0;
            foreach (var need in node.prerequisites) depth = Math.Max(depth, 1 + Depth(Find(need.nodeId), visiting));
            visiting.Remove(node.id);
            return depth;
        }
        public int Depth(UpgradeTreeNode node) => Depth(node, new HashSet<string>());

        public static int PurchasedRank(UpgradeTreeNode node, int legacyRank) => node.IsRoot ? 1 : node.id == ClickStrengthNodeId ? legacyRank : 0;
        public static int PurchasedRank(UpgradeTreeNode node, Func<string,int> rank) => node.IsRoot ? 1 : rank(node.id);
        public static int MaximumRank(UpgradeTreeNode node) => node.IsRoot ? 1 : UpgradeNodeRules.MaximumRank(node.id);
        public bool PrerequisitesMet(UpgradeTreeNode node, int legacyRank)
            => PrerequisitesMet(node, id => id == ClickStrengthNodeId ? legacyRank : 0);
        public bool PrerequisitesMet(UpgradeTreeNode node, Func<string,int> rank)
        {
            foreach (var need in node.prerequisites)
            {
                var parent = Find(need.nodeId);
                int required = need.rank == -1 ? MaximumRank(parent) : need.rank;
                if (required == 0 || PurchasedRank(parent, rank) < required) return false;
            }
            return true;
        }
    }
}
