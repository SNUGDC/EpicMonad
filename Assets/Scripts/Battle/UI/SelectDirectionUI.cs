using UnityEngine;

namespace BattleUI
{
	public class SelectDirectionUI : MonoBehaviour
	{
		private BattleManager gameManager;

		public void Start()
		{
			gameManager = FindObjectOfType<BattleManager>();
		}

		public void CallbackDirection(string directionString)
		{
			gameManager.CallbackDirection(directionString);
		}
	}
}
