using UnityEngine;

namespace BattleUI
{
	public class SkillCheckPannel : MonoBehaviour
	{
		private BattleManager gameManager;

		public void Start()
		{
			gameManager = FindObjectOfType<BattleManager>();
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
