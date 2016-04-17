using UnityEngine;

namespace BattleUI
{
	public class SkillPanel : MonoBehaviour
	{
		private GameManager gameManager;

		public void Start()
		{
			gameManager = FindObjectOfType<GameManager>();
		}

		public void CallbackSkillIndex(int index)
		{
			gameManager.CallbackSkillIndex(index);
		}

		public void CallbackSkillUICancel()
		{
			gameManager.CallbackSkillUICancel();
		}
	}
}
