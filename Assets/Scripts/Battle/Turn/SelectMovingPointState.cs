using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;

namespace Battle.Turn
{
	public class SelectMovingPointState
	{
		public static IEnumerator Run(BattleManager battleManager)
		{
			while (battleManager.currentState == CurrentState.SelectMovingPoint)
			{
				// List<GameObject> movableTiles = CheckMovableTiles(selectedUnitObject);
				Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(battleManager.selectedUnitObject);
				List<GameObject> movableTiles = new List<GameObject>();
				foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
				{
					movableTiles.Add(movableTileWithPath.Value.tile);
				}

				battleManager.tileManager.ChangeTilesToSeletedColor(movableTiles, TileColor.Blue);

				battleManager.rightClicked = false;
				battleManager.cancelClicked = false;
				battleManager.uiManager.EnableCancelButtonUI();

				battleManager.isWaitingUserInput = true;
				battleManager.isSelectedTileByUser = false;
				while (!battleManager.isSelectedTileByUser)
				{
					//yield break 넣으면 코루틴 강제종료
					if (battleManager.rightClicked || battleManager.cancelClicked)
					{
						battleManager.rightClicked = false;
						battleManager.cancelClicked = false;
						battleManager.uiManager.DisableCancelButtonUI();

						battleManager.tileManager.ChangeTilesFromSeletedColorToDefaultColor(movableTiles);

						battleManager.currentState = CurrentState.FocusToUnit;
						battleManager.isWaitingUserInput = false;
						yield break;
					}
					yield return null;
				}
				battleManager.isSelectedTileByUser = false;
				battleManager.isWaitingUserInput = false;


				// FIXME : 어딘가로 옮겨야 할 텐데...
				GameObject destTile = battleManager.tileManager.GetTile(battleManager.selectedTilePosition);
				List<GameObject> destPath = movableTilesWithPath[battleManager.selectedTilePosition].path;
				Vector2 currentTilePos = battleManager.selectedUnitObject.GetComponent<Unit>().GetPosition();
				Vector2 distanceVector = battleManager.selectedTilePosition - currentTilePos;
				int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
				int totalUseActionPoint = movableTilesWithPath[battleManager.selectedTilePosition].requireActivityPoint;

				battleManager.moveCount += distance;

				battleManager.tileManager.ChangeTilesFromSeletedColorToDefaultColor(movableTiles);
				battleManager.currentState = CurrentState.CheckDestination;
				battleManager.uiManager.DisableCancelButtonUI();
				yield return battleManager.StartCoroutine(battleManager.CheckDestination(movableTiles, destTile, destPath, totalUseActionPoint, distance));
			}
			yield return null;
		}
	}
}