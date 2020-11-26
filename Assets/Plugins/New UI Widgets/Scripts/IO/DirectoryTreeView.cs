namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using UIWidgets.Styles;
	using UnityEngine;

	/// <summary>
	/// DirectoryTreeView
	/// </summary>
	public class DirectoryTreeView : TreeViewCustom<DirectoryTreeViewComponent, FileSystemEntry>
	{
		/// <summary>
		/// Root directory, if not specified drives will be used as root.
		/// </summary>
		[SerializeField]
		protected string rootDirectory = string.Empty;

		/// <summary>
		/// Root directory, if not specified drives will be used as root.
		/// </summary>
		public string RootDirectory
		{
			get
			{
				return rootDirectory;
			}

			set
			{
				SetRootDirectory(value);
			}
		}

		/// <summary>
		/// Display IO errors.
		/// </summary>
		[SerializeField]
		public IOExceptionsView ExceptionsView;

		bool isInited = false;

		/// <summary>
		/// Init.
		/// </summary>
		public override void Init()
		{
			if (isInited)
			{
				return;
			}

			isInited = true;

			base.Init();

			SetRootDirectory(rootDirectory);
		}

		/// <summary>
		/// Set root directory.
		/// </summary>
		/// <param name="root">New root.</param>
		protected virtual void SetRootDirectory(string root)
		{
			rootDirectory = root;

			if (string.IsNullOrEmpty(root))
			{
				var drives = GetDrives();
				LoadNodes(drives);

				Nodes = drives;
			}
			else
			{
				var nodes = GetDirectoriesNodes(root);
				LoadNodes(nodes);

				Nodes = nodes;
			}
		}

		/// <summary>
		/// Get directories list from current to root.
		/// </summary>
		/// <param name="directory">Directory.</param>
		/// <returns>Directories list from current to root.</returns>
		protected static List<string> GetPaths(string directory)
		{
			var paths = new List<string>();
			var temp = directory;

			do
			{
				paths.Add(temp);
				temp = Path.GetDirectoryName(temp);
			}
			while (!string.IsNullOrEmpty(temp));

			return paths;
		}

		/// <summary>
		/// Get node for specified directory.
		/// </summary>
		/// <param name="directory">Directory.</param>
		/// <returns>Node if directory found; otherwise, null.</returns>
		public virtual TreeNode<FileSystemEntry> GetNodeByPath(string directory)
		{
			if (!Directory.Exists(directory))
			{
				return null;
			}

			var paths = GetPaths(directory);
			var nodes = Nodes;
			TreeNode<FileSystemEntry> node;

			do
			{
				node = FindNode(nodes, paths);
				if (node == null)
				{
					return null;
				}

				paths.Remove(node.Item.FullName);

				if (node.Nodes == null)
				{
					node.Nodes = GetDirectoriesNodes(node.Item.FullName);
				}

				nodes = node.Nodes;
			}
			while (directory != node.Item.FullName);

			return node;
		}

		/// <summary>
		/// Find node with one of the specified paths.
		/// </summary>
		/// <param name="nodes">Nodes.</param>
		/// <param name="paths">Paths.</param>
		/// <returns>Node.</returns>
		protected TreeNode<FileSystemEntry> FindNode(ObservableList<TreeNode<FileSystemEntry>> nodes, List<string> paths)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				if (paths.Contains(nodes[i].Item.FullName))
				{
					return nodes[i];
				}
			}

			return null;
		}

		/// <summary>
		/// Select node with specified directory.
		/// </summary>
		/// <param name="directory">Directory.</param>
		/// <returns>true if directory found; otherwise, false.</returns>
		public virtual bool SelectDirectory(string directory)
		{
			var node = GetNodeByPath(directory);
			if (node == null)
			{
				return false;
			}

			var parent = node.Parent;
			while (parent != null)
			{
				parent.IsExpanded = true;
				parent = parent.Parent;
			}

			SelectNode(node);
			ScrollTo(Node2Index(node));

			return true;
		}

		/// <summary>
		/// Toggles the node.
		/// </summary>
		/// <param name="index">Index.</param>
		protected override void ToggleNode(int index)
		{
			var node = NodesList[index];

			// disable node observation
			node.Node.PauseObservation = true;
			node.Node.IsExpanded = !node.Node.IsExpanded;
			node.Node.PauseObservation = false;

			var nodes = node.Node.Nodes;
			nodes.BeginUpdate();

			// force update because node observation was disabled and if nothing found update will not be called
			nodes.CollectionChanged();

			LoadNodes(nodes);

			nodes.EndUpdate();
		}

		/// <summary>
		/// Ger drives.
		/// </summary>
		/// <returns>Drives list.</returns>
		public virtual ObservableList<TreeNode<FileSystemEntry>> GetDrives()
		{
			return ExceptionsView.Execute<ObservableList<TreeNode<FileSystemEntry>>>(FillDrivesList);
		}

		/// <summary>
		/// Fill the drives list.
		/// </summary>
		/// <param name="list">list.</param>
		protected void FillDrivesList(ObservableList<TreeNode<FileSystemEntry>> list)
		{
#if !NETFX_CORE
			var drives = Directory.GetLogicalDrives();
			for (int i = 0; i < drives.Length; i++)
			{
				var item = new FileSystemEntry(drives[i], drives[i], false);
				list.Add(new TreeNode<FileSystemEntry>(item, null));
			}
#endif
		}

		/// <summary>
		/// Load sub-nodes data for specified nodes.
		/// </summary>
		/// <param name="nodes">Nodes.</param>
		public virtual void LoadNodes(ObservableList<TreeNode<FileSystemEntry>> nodes)
		{
			nodes.BeginUpdate();

			try
			{
				nodes.ForEach(LoadNode);
			}
			finally
			{
				nodes.EndUpdate();
			}
		}

		/// <summary>
		/// Get sub-nodes for specified directory.
		/// </summary>
		/// <param name="path">Directory.</param>
		/// <returns>Sub-nodes for specified directory.</returns>
		public virtual ObservableList<TreeNode<FileSystemEntry>> GetDirectoriesNodes(string path)
		{
			var nodes = ExceptionsView.Execute<ObservableList<TreeNode<FileSystemEntry>>, string>(FillDirectoriesList, path);
			ExceptionsView.CurrentError = null;

			return nodes;
		}

		/// <summary>
		/// Fill the directories list.
		/// </summary>
		/// <param name="list">List.</param>
		/// <param name="path">Path.</param>
		protected void FillDirectoriesList(ObservableList<TreeNode<FileSystemEntry>> list, string path)
		{
			var directories = Directory.GetDirectories(path);
			for (int i = 0; i < directories.Length; i++)
			{
				var item = new FileSystemEntry(directories[i], Path.GetFileName(directories[i]), false);
				list.Add(new TreeNode<FileSystemEntry>(item, null));
			}
		}

		/// <summary>
		/// Load sub-nodes data for specified node.
		/// </summary>
		/// <param name="node">Node.</param>
		public virtual void LoadNode(TreeNode<FileSystemEntry> node)
		{
			if (node.Nodes != null)
			{
				return;
			}

			node.Nodes = GetDirectoriesNodes(node.Item.FullName);
		}

		#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <returns><c>true</c>, if style was set for children gameobjects, <c>false</c> otherwise.</returns>
		/// <param name="style">Style data.</param>
		public override bool SetStyle(Style style)
		{
			if (ExceptionsView != null)
			{
				ExceptionsView.SetStyle(style);
			}

			return base.SetStyle(style);
		}
		#endregion
	}
}