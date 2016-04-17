using UnityEngine;

namespace BattleUI
{
	public class SkillCheckPannel : MonoBehaviour
	{
		private GameManager gameManager;

		public void Start()
		{
			gameManager = FindObjectOfType<GameManager>();
		}

		public void CallbackApplyCommand()
		{
			gameManager.CallbackApplyCommand();
		}

		public void CallbackChainCommand()
		{
			gameManager.CallbackChainCommand();
		}

		public void CallbackCancel()
		{
			gameManager.CallbackCancel();
		}
	}
}
