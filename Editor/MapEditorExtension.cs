namespace NH.Extensions.Editor
{
	using System;
	using NH.Extensions;
	using UnityEditor;
	using UnityEngine;
	using PE = PlacementEnum;
	using GE = GroupingEnum;

	[CustomEditor(typeof(MapEditor))]

	public class MapEditorExtension : Editor
	{
		private MapEditor realTarget;
		private Vector3 mousePosition;

		private void OnSceneGUI() 
		{
			if (!this.realTarget) 
			{
				return;
			}

			if (this.MouseHitUpdated())
			{
				SceneView.RepaintAll();
			}

			this.ResetMousePosition();

			if (this.ContainsMouse())
			{
				Event currentEvent = Event.current;
				if (currentEvent.type == EventType.MouseDown 
				    || currentEvent.type == EventType.MouseDrag) {

					if (currentEvent.button == 0)
					{
						this.CreateObject();
						currentEvent.Use();
					}

					if (currentEvent.button == 1)
					{
						this.EraseObject();
						currentEvent.Use();
					}
				}
			}
		}

		private void OnEnable() 
		{
			this.realTarget = this.target as MapEditor;
		}

		override public void OnInspectorGUI() {

			base.OnInspectorGUI();

			if (!this.realTarget) 
			{
				return;
			}

			EditorGUILayout.Separator();
			EditorGUILayout.LabelField(label: string.Format("Children count: {0}", this.realTarget.transform.childCount));
			if (GUILayout.Button(text: "Clear children"))
			{
				if (EditorUtility.DisplayDialog("Clear map",
				                                "Are you sure", "Yes", "Cancel"))
				{
					var count = this.realTarget.transform.childCount;
					for (int index = count - 1; index >= 0; index--)
					{
						UnityEngine.Object.DestroyImmediate(this.realTarget.transform.GetChild(index).gameObject);
					}
				}
			}

			if (GUILayout.Button(text: "Reset if possible"))
			{
				for (int row = 0; row < this.realTarget.rows; row++)
				{
					for (int column = 0; column < this.realTarget.columns; column++)
					{
						var objectName = string.Format("{2}_childObject_{0}_{1}", column, row, this.realTarget.name);

						var @object = GameObject.Find(objectName);

						if (@object
						    && @object.transform.parent == this.realTarget.transform)
						{
							@object.transform.localScale = new Vector3( this.realTarget.changeScale ? this.realTarget.columnScale : 1, this.realTarget.changeScale ? this.realTarget.rowScale : 1, 1);

							@object.transform.position = this.realTarget.transform.position
								+ new Vector3(column * this.realTarget.columnScale + this.realTarget.columnScale / 2, row * this.realTarget.rowScale +  this.realTarget.rowScale / 2);

						}
					}
				}
			}

			EditorGUILayout.Separator();

			if (GUILayout.Button(text: "Add tile"))
			{
				EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "", 10312);
			}

			string commandName = Event.current.commandName;
			if (commandName == "ObjectSelectorClosed"
			    && EditorGUIUtility.GetObjectPickerControlID() == 10312) 
			{
				var @object = EditorGUIUtility.GetObjectPickerObject() as GameObject;

				if (@object) {
					this.realTarget.tileObjects.Add(@object);
				}

				Event.current.commandName = "";
			}

			if (GUILayout.Button(text: "Remove last tile object"))
			{
				if (EditorUtility.DisplayDialog("Remove last tile",
				                                "Are you sure", "Yes", "Cancel"))
				{
					this.realTarget.tileObjects.RemoveAt(this.realTarget.tileObjects.Count - 1);
				}
			}

			if (GUILayout.Button(text: "Clear tile list"))
			{
				if (EditorUtility.DisplayDialog("Clear tile",
				                            "Are you sure", "Yes", "Cancel"))
				{
					this.realTarget.tileObjects.Clear();
				}
			}
		}

		private void CreateObject()
		{
			if (!this.realTarget
			    || this.realTarget.tileObjects == null
			    || this.realTarget.tileObjects.Count == 0) 
			{
				return;
			}

			for (int column = 0; column < this.realTarget.selectionColumns; column++)
			{
				for (int row = 0; row < this.realTarget.selectionRows; row++)
				{
					var tilePosition = this.GetSelectedTile(additionalColumn: column, additionalRow: row);
					var objectName = string.Format("{2}_childObject_{0}_{1}", tilePosition.x, tilePosition.y, this.realTarget.name);
					
					var random = new System.Random();
					var selectedObject = this.realTarget.tileObjects[random.Next(this.realTarget.tileObjects.Count)];
					
					if (!selectedObject)
					{
						return;
					}
					
					var @object = GameObject.Find(objectName);
					
					if (@object
					    && @object.transform.parent != this.realTarget.transform)
					{
						return;
					}
					
					if (@object && @object.gameObject != selectedObject) 
					{
						UnityEngine.Object.DestroyImmediate(@object);
						@object = null;
					}
					
					if (!@object)
					{
						@object = GameObject.Instantiate(selectedObject) as GameObject;
					}
					
					if (this.realTarget.changeScale)
					{
						@object.transform.localScale = new Vector3(this.realTarget.columnScale, this.realTarget.rowScale, 1);
					}
					
					var newPosition = this.GetObjectPosition(column: (int)tilePosition.x, row: (int)tilePosition.y, scale: @object.transform.localScale);
					@object.transform.position = newPosition;

					switch (this.realTarget.grouping)
					{
					case GE.Vertical:
						{
							var groupName = string.Format("{1}_verticalGroup_{0}", tilePosition.x, this.realTarget.name);
							
							var groupObject = GameObject.Find(groupName);
							
							if (!groupObject)
							{
								groupObject = new GameObject();
								groupObject.name = groupName;
								groupObject.transform.position = this.realTarget.transform.position + new Vector3(x: tilePosition.x * this.realTarget.columnScale, y: 0);
								groupObject.transform.parent = this.realTarget.transform;
							}
							
							@object.transform.parent = groupObject.transform;
						}
						break;
					case GE.Horizontal:
						{
							var groupName = string.Format("{1}_horizontalGroup_{0}", tilePosition.y, this.realTarget.name);

							var groupObject = GameObject.Find(groupName);

							if (!groupObject)
							{
								groupObject = new GameObject();
								groupObject.name = groupName;
								groupObject.transform.position = this.realTarget.transform.position + new Vector3(x: 0, y: tilePosition.y * this.realTarget.rowScale);
								groupObject.transform.parent = this.realTarget.transform;
							}

							@object.transform.parent = groupObject.transform;
						}
						break;
					default:
						@object.transform.parent = this.realTarget.transform;
						break;
					}

					
					@object.name = objectName;
				}

			}

		}

		private Vector3 GetObjectPosition(int column, int row, Vector3 scale)
		{
			if (this.realTarget == null) 
			{
				return Vector3.zero;
			}

			var position = this.realTarget.transform.position + new Vector3(x: column * this.realTarget.columnScale, y: row * this.realTarget.rowScale);//this.realTarget.mousePosition;
			var width = this.realTarget.columnScale;
			var height = this.realTarget.rowScale;

			var scaleHalfWidth = scale.x / 2;
			var scaleHalfHeight = scale.y / 2;

			switch (this.realTarget.placement)
			{
			case PE.BottomLeft:
				position += new Vector3(x: scaleHalfWidth, y: scaleHalfHeight);
				break;
			case PE.BottomRight:
				position += new Vector3(x: width - scaleHalfWidth, y: scaleHalfHeight);
				break;
			case PE.TopLeft:
				position += new Vector3(x: scaleHalfWidth, y: height - scaleHalfHeight);
				break;
			case PE.TopRight:
				position += new Vector3(x: width - scaleHalfWidth, y: height - scaleHalfHeight);
				break;
			case PE.Center:
				position += new Vector3(x: 0.5f * width, y: 0.5f * height);
				break;
			default:
				break;
			}

			return position;
		}

		private void EraseObject()
		{
			if (!this.realTarget)
			{
				return;
			}

			for (int column = 0; column < this.realTarget.selectionColumns; column++)
			{
				for (int row = 0; row < this.realTarget.selectionRows; row++)
				{
					var tilePosition = this.GetSelectedTile(additionalColumn: column, additionalRow: row);

					var objectName = string.Format("{2}_childObject_{0}_{1}", tilePosition.x, tilePosition.y, this.realTarget.name);
					
					var @object = GameObject.Find(objectName);
					
					if (@object
					    && @object.transform.parent != this.realTarget.transform)
					{
						return;
					}

					UnityEngine.Object.DestroyImmediate(@object);
				}
			}
		}

		private bool ContainsMouse() 
		{
			if (!this.realTarget
			    || !this.realTarget.isSelected) 
			{
				return false;
			}

			return this.mousePosition.x > 0 
				&& this.mousePosition.x <= (this.realTarget.columns * this.realTarget.columnScale)
				&& this.mousePosition.y > 0
				&& this.mousePosition.y <= (this.realTarget.rows * this.realTarget.rowScale);
		}

		private Vector2 GetSelectedTile(int additionalColumn = 0, int additionalRow = 0) 
		{
			if (!this.realTarget) 
			{
				return Vector2.zero;
			}

			var position = new Vector3(
				x: this.mousePosition.x / this.realTarget.columnScale,
				y: this.mousePosition.y / this.realTarget.rowScale,
				z: this.realTarget.transform.position.z);

			position = new Vector3(
				x: (int)Math.Round(position.x, 5, MidpointRounding.ToEven), 
				y: (int)Math.Round(position.y, 5, MidpointRounding.ToEven), 
				z: 0);

			int column = (int)position.x + additionalColumn;
			int row = (int)position.y + additionalRow;

			column = (int)Mathf.Clamp(column, 0, this.realTarget.columns - 1);
			row = (int)Mathf.Clamp(row, 0, this.realTarget.rows - 1);

			return new Vector2(column, row);
		}

		private void ResetMousePosition()
		{
			if (!this.realTarget) 
			{
				return;
			}

			var currentTilePosition = this.GetSelectedTile();

			var initialPosition = this.realTarget.transform.position;
			var width = this.realTarget.columnScale;
			var height = this.realTarget.rowScale;

			this.realTarget.mousePosition = initialPosition + new Vector3(x: currentTilePosition.x * width, y: currentTilePosition.y * height);
		}

		private bool MouseHitUpdated() 
		{
			if (!this.realTarget) 
			{
				return false;
			}

			var plane = new Plane(
				inNormal: this.realTarget.transform.TransformDirection(Vector3.forward),
				inPoint: this.realTarget.transform.position);

			var mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

			var hitPosition = new Vector3();

			float distance;

			if (plane.Raycast(mouseRay, out distance)) 
			{
				hitPosition = mouseRay.origin + (mouseRay.direction.normalized * distance);
			}

			var localMousePotion = this.realTarget.transform.InverseTransformPoint(hitPosition);

			if (this.mousePosition != localMousePotion)
			{
				this.mousePosition = localMousePotion;
				return true;
			}

			return false;
		}
	}
}