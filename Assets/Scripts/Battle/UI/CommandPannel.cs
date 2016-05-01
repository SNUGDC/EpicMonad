using UnityEngine;

namespace BattleUI
{
	public class CommandPannel : MonoBehaviour
	{
		private BattleManager gameManager;

		public void Start()
		{
			gameManager = FindObjectOfType<BattleManager>();
		}

		public void CallbackMoveCommand()
		{
			gameManager.CallbackMoveCommand();
		}

		public void CallbackAttackCommand()
		{
			gameManager.CallbackAttackCommand();
		}

		public void CallbackRestCommand()
		{
			gameManager.CallbackRestCommand();
		}

		public void CallbackStandbyCommand()
		{
			gameManager.CallbackStandbyCommand();
		}
	}
}
