using UnityEngine;

namespace BattleUI
{
	public class CancelButtonPanel : MonoBehaviour
	{
		private BattleManager gameManager;

		public void Start()
		{
			gameManager = FindObjectOfType<BattleManager>();
		}

		public void CallbackCancel()
		{
			gameManager.CallbackCancel();
		}
	}
}
