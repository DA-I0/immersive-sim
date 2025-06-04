using Godot;

namespace ImmersiveSim.Systems
{
	public partial class LevelManager : Node
	{
		public Statics.StatusUpdate UpcomingLevelChange;
		public Statics.StatusUpdate LevelLoaded;
		public Statics.StatusValueUpdate SubLevelLoaded;

		private bool _levelChangeInProgress = false;
		private System.Collections.Generic.Dictionary<string, Node3D> _loadingQueue = new();

		private string _targetSpawnPoint;

		public bool LevelChangeInProgress
		{
			get { return _levelChangeInProgress; }
		}

		public Node ActiveScene
		{
			get { return GetNode(ActiveScenePath); }
		}

		public string ActiveSceneName
		{
			get { return GetChild(0).Name; }
		}

		public string ActiveScenePath
		{
			get { return GetChild(0).GetPath(); }
		}

		public bool IsInMenu
		{
			get { return GetChildCount() <= 0 || ActiveSceneName.Contains("Menu"); }
		}

		public override void _Ready()
		{
			ChildEnteredTree += (Node scene) => CallDeferred("FinalizeSceneSetup", scene);
		}

		public override void _Process(double delta)
		{
			CheckLoadProgress();
		}

		public void LoadMenu()
		{
			LevelChangePrep();
			string scenePath = $"{ProjectSettings.GetSetting("global/LevelFolderPath")}/menu.tscn".ToLower();
			LoadNewScene(scenePath);
		}

		public void ChangeLevel(string newLevelName, string targetSpawnPoint = "")
		{
			LevelChangePrep();
			_targetSpawnPoint = targetSpawnPoint;
			ProceedWithLevelChange(newLevelName);
		}

		public Vector3 GetSpawnPosition()
		{
			Vector3 spawnPosition = Vector3.Zero;

			if (_targetSpawnPoint != string.Empty)
			{
				Node3D targetNode = ActiveScene.GetNodeOrNull<Node3D>($"SpawnPoints/{_targetSpawnPoint}");

				if (targetNode != null)
				{
					spawnPosition = targetNode.GlobalPosition;
				}
			}

			_targetSpawnPoint = string.Empty;
			return spawnPosition;
		}

		public void LoadSubLevel(string levelName, Node3D parentNode)
		{
			if (GetNodeOrNull(levelName) == null)
			{
				string scenePath = $"{ProjectSettings.GetSetting("global/LevelFolderPath")}/levels/{levelName}.tscn".ToLower();
				LoadNewScene(scenePath, parentNode);
			}
		}

		public void RemoveSubLevel(string levelName, Node3D parentNode)
		{
			Node sceneNode = parentNode.GetNodeOrNull(levelName);
			SetSceneForDestruction(sceneNode);
		}

		private void LevelChangePrep()
		{
			UpcomingLevelChange?.Invoke();
			_levelChangeInProgress = true;
			ClearActiveScenes();
		}

		private void ProceedWithLevelChange(string newLevelName)
		{
			string scenePath = $"{ProjectSettings.GetSetting("global/LevelFolderPath")}/levels/{newLevelName}.tscn".ToLower();
			LoadNewScene(scenePath);
		}

		private bool ClearActiveScenes()
		{
			foreach (Node levelNode in GetChildren())
			{
				SetSceneForDestruction(levelNode);
			}

			return true;
		}

		private void SetSceneForDestruction(Node targetScene)
		{
			if (targetScene != null)
			{
				targetScene.Name = "toDelete";
				targetScene.GetParent().CallDeferred(MethodName.RemoveChild, targetScene);
				targetScene.QueueFree();
			}
		}

		private void LoadNewScene(string scenePath, Node3D parentNode = null)
		{
			_loadingQueue.TryAdd(scenePath, parentNode);
			ResourceLoader.LoadThreadedRequest(scenePath);
		}

		private void CheckLoadProgress()
		{
			foreach (string scenePath in _loadingQueue.Keys)
			{
				if (ResourceLoader.LoadThreadedGetStatus(scenePath) == ResourceLoader.ThreadLoadStatus.Loaded)
				{
					InstantiateLoadedScene(scenePath, _loadingQueue[scenePath]);
				}
			}
		}

		private void InstantiateLoadedScene(string scenePath, Node3D parentNode)
		{
			Node newScene = (ResourceLoader.LoadThreadedGet(scenePath) as PackedScene).Instantiate();

			if (parentNode != null)
			{
				parentNode.AddChild(newScene);

				if ((Node)parentNode != this)
				{
					parentNode.ChildEnteredTree += InformAboutSubsceneLoading;
				}
			}
			else
			{
				CallDeferred(Node.MethodName.AddChild, newScene);
			}

			_loadingQueue.Remove(scenePath);
		}

		private void FinalizeSceneSetup(Node newChild)
		{
			_levelChangeInProgress = false;
			LevelLoaded?.Invoke();
		}

		private void InformAboutSubsceneLoading(Node newChild)
		{
			newChild.GetParent().ChildEnteredTree -= InformAboutSubsceneLoading;
			SubLevelLoaded?.Invoke(newChild.GetPath());
		}
	}
}