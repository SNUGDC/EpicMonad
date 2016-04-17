using UnityEngine;

namespace BattleUI
{
	public class SelectDirectionUI : MonoBehaviour
	{
		private GameManager gameManager;

		public void Start()
		{
			gameManager = FindObjectOfType<GameManager>();
		}

		public void CallbackDirection(string directionString)
		{
			gameManager.CallbackDirection(directionString);
		}
	}
}
