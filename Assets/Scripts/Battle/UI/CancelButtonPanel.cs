using UnityEngine;

namespace BattleUI
{
	public class CancelButtonPanel : MonoBehaviour
	{
		private GameManager gameManager;

		public void Start()
		{
			gameManager = FindObjectOfType<GameManager>();
		}

		public void CallbackCancel()
		{
			gameManager.CallbackCancel();
		}
	}
}
