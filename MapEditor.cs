

namespace ITC.Extensions 
{
	using UnityEngine;
	using System.Collections.Generic;
	using PE = PlacementEnum;
	using GE = GroupingEnum;

	public enum PlacementEnum 
	{
		BottomLeft, BottomRight, TopLeft, TopRight, Center
	};

	public enum GroupingEnum 
	{
		Horizontal, Vertical, None
	};


	[AddComponentMenu("Extensions/Map Editor")]
	public class MapEditor : MonoBehaviour 
	{
		public int rows = 10;
		public int columns = 10;

		public float rowScale = 1;
		public float columnScale = 1;

		public float selectionRows = 1;
		public float selectionColumns = 1;

		public float sphereRadius = 0.15f;

		public Color borderColor = Color.white;
		public Color lineColor = Color.gray;
		public Color sphereColor = Color.green;
		public Color selectedColor = Color.red;

		public List<GameObject> tileObjects;

		public PlacementEnum placement = PE.Center;
		public GroupingEnum grouping = GE.None;
		public bool changeScale = false;


		public bool drawAlways = false;

		[HideInInspector]
		public Vector3 mousePosition;

		[HideInInspector]
		public bool isSelected;

		private void OnDrawGizmos() 
		{
			if (!this.drawAlways) 
			{
				return;
			}

			this.DrawGrid();

			this.isSelected = false;
		}

		private void OnDrawGizmosSelected()
		{
			this.isSelected = true;

			if (this.drawAlways) 
			{
				return;
			}

			this.DrawGrid();
		}


		private void DrawGrid() {
			var width = this.columns * this.columnScale;
			var height = this.rows * this.rowScale;
			var position = this.transform.position;
			var initialColor = Gizmos.color;

			{
				Gizmos.color = this.borderColor;
				var bottomLeft = position;
				var bottomRight = position + new Vector3(x: width, y: 0);
				var topLeft = position + new Vector3(x: 0, y: height);
				var topRight = position + new Vector3(x: width, y: height);
				Gizmos.DrawLine(from: bottomLeft, to: bottomRight);
				Gizmos.DrawLine(from: bottomRight, to: topRight);
				Gizmos.DrawLine(from: topRight, to: topLeft);
				Gizmos.DrawLine(from: topLeft, to: bottomLeft);

				Gizmos.color = this.lineColor;

				for (int row = 1; row < this.rows; row++) 
				{
					Gizmos.DrawLine(
						from: position + new Vector3(x: 0, y: row * this.rowScale),
						to: position + new Vector3(x: width, y: row * this.rowScale));
				}

				for (int column = 1; column < this.columns; column++)
				{
					Gizmos.DrawLine(
						from: position + new Vector3(x: column * this.columnScale, y: 0),
						to: position + new Vector3(x: column * this.columnScale, y: height));
				}


				if (this.isSelected) {

					for (int column = 0; column < this.selectionColumns; column++)
					{
						for (int row = 0; row < this.selectionRows; row++)
						{
							var objectPosition = /*position + */new Vector3(x: column * this.columnScale, y: row * this.rowScale);
							Gizmos.color = this.sphereColor;
							var spherePosition = objectPosition + this.mousePosition;

							if (!(spherePosition.x < position.x + width && spherePosition.y < position.y + height))
							{
								continue;
							}

							switch (this.placement)
							{
							case PE.BottomLeft:
								break;
							case PE.BottomRight:
								spherePosition += new Vector3(x: this.columnScale, y: 0);
								break;
							case PE.TopLeft:
								spherePosition += new Vector3(x: 0, y: this.rowScale);
								break;
							case PE.TopRight:
								spherePosition += new Vector3(x: this.columnScale, y: this.rowScale);
								break;
							case PE.Center:
								spherePosition += new Vector3(x: 0.5f * this.columnScale, y: 0.5f * this.rowScale);
								break;
							default:
								break;
							}

							Gizmos.DrawSphere(spherePosition, this.sphereRadius);

							Gizmos.color = this.selectedColor;
							var centerPosition = objectPosition + this.mousePosition + new Vector3(x: 0.5f * this.columnScale, y: 0.5f * this.rowScale);
							Gizmos.DrawWireCube(centerPosition, size: new Vector3(x: this.columnScale, y: this.rowScale, z: 0));
						}
					}
				}
			}



			Gizmos.color = initialColor;
		}
	}
}